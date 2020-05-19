using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using NVorbis;
using OpenTK.Audio.OpenAL;

namespace NVorbis.OpenTKSupport
{
    public static class ALHelper
    {
        internal static readonly XRamExtension XRam = new XRamExtension();
        internal static readonly EffectsExtension Efx = new EffectsExtension();

        public static void CheckCapabilities(ILogger logger)
        {
            logger.Log(LogEventBoolean.IsOpenAlSoft, AL.Get(ALGetString.Version).Contains("SOFT"));
            logger.Log(LogEventBoolean.XRamSupport, XRam.IsInitialized);
            logger.Log(LogEventBoolean.EfxSupport, Efx.IsInitialized);
        }

        internal static void Check()
        {
            ALError error;
            if ((error = AL.GetError()) != ALError.NoError)
                throw new InvalidOperationException(AL.GetErrorString(error));
        }
    }

    public class OggStream : IDisposable
    {
        const int DefaultBufferCount = 3;

        internal readonly object stopMutex = new object();
        internal readonly object prepareMutex = new object();

        internal readonly int alSourceId;
        internal readonly int[] alBufferIds;

        readonly int alFilterId;
        readonly Stream underlyingStream;

        internal VorbisReader Reader { get; private set; }
        public bool Ready { get; private set; }
        internal bool Preparing { get; private set; }

        public int BufferCount { get; private set; }

        public ILogger Logger { private get; set; }

        internal EventHandler Finished;

        public OggStream(string filename, int bufferCount = DefaultBufferCount) : this(File.OpenRead(filename), bufferCount) { }
        public OggStream(Stream stream, int bufferCount = DefaultBufferCount)
        {
            BufferCount = bufferCount;

            alBufferIds = AL.GenBuffers(bufferCount);
            alSourceId = AL.GenSource();

            if (ALHelper.XRam.IsInitialized)
            {
                ALHelper.XRam.SetBufferMode(BufferCount, ref alBufferIds[0], XRamExtension.XRamStorage.Hardware);
                ALHelper.Check();
            }

            Volume = 1;

            if (ALHelper.Efx.IsInitialized)
            {
                alFilterId = ALHelper.Efx.GenFilter();
                ALHelper.Efx.Filter(alFilterId, EfxFilteri.FilterType, (int)EfxFilterType.Lowpass);
                ALHelper.Efx.Filter(alFilterId, EfxFilterf.LowpassGain, 1);
                LowPassHFGain = 1;
            }

            underlyingStream = stream;

            Logger = NullLogger.Default;
        }

        public void Prepare()
        {
            if (Preparing) return;

            var state = AL.GetSourceState(alSourceId);

            lock (stopMutex)
            {
                switch (state)
                {
                    case ALSourceState.Playing:
                    case ALSourceState.Paused:
                        return;

                    case ALSourceState.Stopped:
                        lock (prepareMutex)
                        {
                            Reader.DecodedTime = TimeSpan.Zero;
                            Ready = false;
                            Empty();
                        }
                        break;
                }

                if (!Ready)
                {
                    lock (prepareMutex)
                    {
                        Preparing = true;
                        Logger.Log(LogEvent.BeginPrepare, this);
                        Open(precache: true);
                        Logger.Log(LogEvent.EndPrepare, this);
                    }
                }
            }
        }

        public void Play()
        {
            var state = AL.GetSourceState(alSourceId);

            switch (state)
            {
                case ALSourceState.Playing: return;
                case ALSourceState.Paused:
                    Resume();
                    return;
            }

            Prepare();

            Logger.Log(LogEvent.Play, this);
            AL.SourcePlay(alSourceId);
            ALHelper.Check();

            Preparing = false;

            OggStreamer.Instance.AddStream(this);
        }

        public void Pause()
        {
            if (AL.GetSourceState(alSourceId) != ALSourceState.Playing)
                return;

            OggStreamer.Instance.RemoveStream(this);
            Logger.Log(LogEvent.Pause, this); 
            AL.SourcePause(alSourceId);
            ALHelper.Check();
        }

        public void Resume()
        {
            if (AL.GetSourceState(alSourceId) != ALSourceState.Paused)
                return;

            OggStreamer.Instance.AddStream(this);
            Logger.Log(LogEvent.Resume, this); 
            AL.SourcePlay(alSourceId);
            ALHelper.Check();
        }

        public void Stop()
        {
            var state = AL.GetSourceState(alSourceId);
            if (state == ALSourceState.Playing || state == ALSourceState.Paused)
            {
                Logger.Log(LogEvent.Stop, this); 
                StopPlayback();
            }

            lock (stopMutex)
            {
                NotifyFinished();
                OggStreamer.Instance.RemoveStream(this);
            }
        }

        float lowPassHfGain;
        public float LowPassHFGain
        {
            get { return lowPassHfGain; }
            set
            {
                if (ALHelper.Efx.IsInitialized)
                {
                    ALHelper.Efx.Filter(alFilterId, EfxFilterf.LowpassGainHF, lowPassHfGain = value);
                    ALHelper.Efx.BindFilterToSource(alSourceId, alFilterId);
                    ALHelper.Check();
                }
            }
        }

        float volume;
        public float Volume
        {
            get { return volume; }
            set
            {
                AL.Source(alSourceId, ALSourcef.Gain, volume = value);
                ALHelper.Check();
            }
        }

        public bool IsLooped { get; set; }

        public void Dispose()
        {
            var state = AL.GetSourceState(alSourceId);
            if (state == ALSourceState.Playing || state == ALSourceState.Paused)
                StopPlayback();

            lock (prepareMutex)
            {
                OggStreamer.Instance.RemoveStream(this);

                if (state != ALSourceState.Initial)
                    Empty();

                Close();

                underlyingStream.Dispose();
            }

            AL.DeleteSource(alSourceId);
            AL.DeleteBuffers(alBufferIds);

            if (ALHelper.Efx.IsInitialized)
                ALHelper.Efx.DeleteFilter(alFilterId);

            ALHelper.Check();

            Logger.Log(LogEventSingle.MemoryUsage, () => GC.GetTotalMemory(true));
        }

        void StopPlayback()
        {
            AL.SourceStop(alSourceId);
            ALHelper.Check();
        }

        internal void NotifyFinished()
        {
            var callback = Finished;
            if (callback != null)
            {
                callback(this, EventArgs.Empty);
                Finished = null;  // This is not typical...  Usually we count on whatever code added the event handler to also remove it
            }
        }

        void Empty()
        {
            int queued;
            AL.GetSource(alSourceId, ALGetSourcei.BuffersQueued, out queued);
            if (queued > 0)
            {
                try
                {
                    AL.SourceUnqueueBuffers(alSourceId, queued);
                    ALHelper.Check();
                }
                catch (InvalidOperationException)
                {
                    // This is a bug in the OpenAL implementation
                    // Salvage what we can
                    int processed;
                    AL.GetSource(alSourceId, ALGetSourcei.BuffersProcessed, out processed);
                    var salvaged = new int[processed];
                    if (processed > 0)
                    {
                        AL.SourceUnqueueBuffers(alSourceId, processed, salvaged);
                        ALHelper.Check();
                    }

                    // Try turning it off again?
                    AL.SourceStop(alSourceId);
                    ALHelper.Check();

                    Empty();
                }
            }

            Logger.Log(LogEvent.Empty, this);
        }

        internal void Open(bool precache = false)
        {
            underlyingStream.Seek(0, SeekOrigin.Begin);
            Reader = new VorbisReader(underlyingStream, false);

            if (precache)
            {
                // Fill first buffer synchronously
                OggStreamer.Instance.FillBuffer(this, alBufferIds[0]);
                AL.SourceQueueBuffer(alSourceId, alBufferIds[0]);
                ALHelper.Check();

                // Schedule the others asynchronously
                OggStreamer.Instance.AddStream(this);
            }

            Ready = true;
        }

        internal void Close()
        {
            if (Reader != null)
            {
                Reader.Dispose();
                Reader = null;
            }
            Ready = false;
        }
    }

    public class OggStreamer : IDisposable
    {
        const float DefaultUpdateRate = 10;
        const int DefaultBufferSize = 44100;

        static readonly object singletonMutex = new object();

        readonly object iterationMutex = new object();
        readonly object readMutex = new object();

        readonly float[] readSampleBuffer;
        readonly short[] castBuffer;

        readonly HashSet<OggStream> streams = new HashSet<OggStream>();
        readonly List<OggStream> threadLocalStreams = new List<OggStream>();

        Thread underlyingThread;
        volatile bool cancelled;

        public float UpdateRate { get; private set; }
        public int BufferSize { get; private set; }

        public ILogger Logger { private get; set; }

        static OggStreamer instance;
        public static OggStreamer Instance
        {
            get
            {
                lock (singletonMutex)
                {
                    if (instance == null)
                        throw new InvalidOperationException("No instance running");
                    return instance;
                }
            }
            private set { lock (singletonMutex) instance = value; }
        }

        /// <summary>
        /// Constructs an OggStreamer that plays ogg files in the background
        /// </summary>
        /// <param name="bufferSize">Buffer size</param>
        /// <param name="updateRate">Number of times per second to update</param>
        /// <param name="internalThread">True to use an internal thread, false to use your own thread, in which case use must call EnsureBuffersFilled periodically</param>
        public OggStreamer(int bufferSize = DefaultBufferSize, float updateRate = DefaultUpdateRate, bool internalThread = true)
        {
            lock (singletonMutex)
            {
                if (instance != null)
                    throw new InvalidOperationException("Already running");

                Instance = this;
                if (internalThread)
                {
                    underlyingThread = new Thread(EnsureBuffersFilled) { Priority = ThreadPriority.Lowest };
                    underlyingThread.Start();
                }
                else
                {
                    // no need for this, user is in charge
                    updateRate = 0;
                }
            }

            UpdateRate = updateRate;
            BufferSize = bufferSize;

            readSampleBuffer = new float[bufferSize];
            castBuffer = new short[bufferSize];

            Logger = NullLogger.Default;
        }

        public void Dispose()
        {
            lock (singletonMutex)
            {
                Debug.Assert(Instance == this, "Two instances running, somehow...?");

                cancelled = true;
                lock (iterationMutex)
                    streams.Clear();

                Instance = null;
                underlyingThread = null;
            }
        }

        internal bool AddStream(OggStream stream)
        {
            lock (iterationMutex)
                return streams.Add(stream);
        }
        internal bool RemoveStream(OggStream stream)
        {
            lock (iterationMutex) 
                return streams.Remove(stream);
        }

        public bool FillBuffer(OggStream stream, int bufferId)
        {
            int readSamples;
            lock (readMutex)
            {   
                readSamples = stream.Reader.ReadSamples(readSampleBuffer, 0, BufferSize);
                CastBuffer(readSampleBuffer, castBuffer, readSamples);
            }
            AL.BufferData(bufferId, stream.Reader.Channels == 1 ? ALFormat.Mono16 : ALFormat.Stereo16, castBuffer,
                          readSamples * sizeof (short), stream.Reader.SampleRate);
            ALHelper.Check();

            if (readSamples == BufferSize)  Logger.Log(LogEvent.NewPacket, stream);
            else                            Logger.Log(LogEvent.LastPacket, stream);
            Logger.Log(LogEventSingle.MemoryUsage, () => GC.GetTotalMemory(true));

            return readSamples != BufferSize;
        }
        public static void CastBuffer(float[] inBuffer, short[] outBuffer, int length)
        {
            for (int i = 0; i < length; i++)
            {
                var temp = (int)(32767f * inBuffer[i]);
                if (temp > short.MaxValue) temp = short.MaxValue;
                else if (temp < short.MinValue) temp = short.MinValue;
                outBuffer[i] = (short)temp;
            }
        }

        public void EnsureBuffersFilled()
        {
            do
            {
                threadLocalStreams.Clear();
                lock (iterationMutex) threadLocalStreams.AddRange(streams);

                foreach (var stream in threadLocalStreams)
                {
                    lock (stream.prepareMutex)
                    {
                        lock (iterationMutex)
                            if (!streams.Contains(stream))
                                continue;

                        bool finished = false;

                        int queued;
                        AL.GetSource(stream.alSourceId, ALGetSourcei.BuffersQueued, out queued);
                        ALHelper.Check();
                        int processed;
                        AL.GetSource(stream.alSourceId, ALGetSourcei.BuffersProcessed, out processed);
                        ALHelper.Check();

                        if (processed == 0 && queued == stream.BufferCount) continue;

                        int[] tempBuffers;
                        if (processed > 0)
                            tempBuffers = AL.SourceUnqueueBuffers(stream.alSourceId, processed);
                        else
                            tempBuffers = stream.alBufferIds.Skip(queued).ToArray();

                        int bufIdx = 0;
                        for (; bufIdx < tempBuffers.Length; bufIdx++)
                        {
                            finished |= FillBuffer(stream, tempBuffers[bufIdx]);

                            if (finished)
                            {
                                if (stream.IsLooped)
                                {
                                    stream.Reader.DecodedTime = TimeSpan.Zero;
                                    if (bufIdx == 0)
                                    {
                                        // we didn't have any buffers left over, so let's start from the beginning on the next cycle...
                                        continue;
                                    }
                                }
                                else
                                {
                                    lock (stream.stopMutex)
                                    {
                                        stream.NotifyFinished();
                                    }
                                    streams.Remove(stream);
                                    break;
                                }
                            }
                        }

                        AL.SourceQueueBuffers(stream.alSourceId, bufIdx, tempBuffers);
                        ALHelper.Check();

                        if (finished && !stream.IsLooped)
                            continue;
                    }

                    lock (stream.stopMutex)
                    {
                        if (stream.Preparing) continue;

                        lock (iterationMutex)
                            if (!streams.Contains(stream))
                                continue;

                        var state = AL.GetSourceState(stream.alSourceId);
                        if (state == ALSourceState.Stopped)
                        {
                            Logger.Log(LogEvent.BufferUnderrun, stream);
                            AL.SourcePlay(stream.alSourceId);
                            ALHelper.Check();
                        }
                    }
                }

                if (UpdateRate > 0)
                {
                    Thread.Sleep((int)(1000 / UpdateRate));
                }
            }
            while (underlyingThread != null && !cancelled);
        }
    }
}
