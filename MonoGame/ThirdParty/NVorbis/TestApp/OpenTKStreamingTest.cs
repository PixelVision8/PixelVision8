using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NVorbis.OpenTKSupport;
using OpenTK.Audio;

namespace TestApp
{
    static class OpenTKStreamingTest
    {
        static readonly string[] StreamFiles = new[] { "2test.ogg", "2test.ogg", "2test.ogg", "2test.ogg", "2test.ogg", "3test.ogg" };

        static void Main()
        {
#if TRACE
            Trace.Listeners.Add(new ConsoleTraceListener());
#endif
            Console.WindowHeight = StreamFiles.Length + 12;

            Console.WriteLine("Pr[e]pare, [P]lay, [S]top, Pa[u]se, [R]esume, [L]oop toggle, [Q]uit");
            Console.WriteLine("Faders (in/out) : Low-pass filter [F]/[G], Volume [V]/[B]");
            Console.WriteLine("[Up], [Down] : Change current sample");
            Console.WriteLine("[Shift] + Action : Do for all " + StreamFiles.Length + " streams");

            var logger = new ConsoleLogger();
            logger.Write(" #  FX Buffering", 0, 8);

            using (new AudioContext())
            using (var streamer = new OggStreamer(65536))
            {
                streamer.Logger = logger;
                ALHelper.CheckCapabilities(logger);

                bool quit = false;

                var streams = new OggStream[StreamFiles.Length];

                for (int i = 0; i < StreamFiles.Length; i++)
                {
                    streams[i] = new OggStream(StreamFiles[i]) { Logger = logger };
                    logger.SetStreamIndex(streams[i], i);
                    logger.Write((i + 1).ToString(), 1, 10 + i);
                }
                logger.Write(">", 0, 10);
                foreach (var s in streams)
                    s.Prepare();

                int sIdx = 0;
                var activeSet = new List<OggStream>();

                while (!quit)
                {
                    var input = Console.ReadKey(true);

                    activeSet.Clear();
                    if ((input.Modifiers & ConsoleModifiers.Shift) == ConsoleModifiers.Shift)
                        activeSet.AddRange(streams);
                    else
                        activeSet.Add(streams[sIdx]);

                    var lower = char.ToLower(input.KeyChar);
                    if (input.Key == ConsoleKey.UpArrow) lower = '-';
                    if (input.Key == ConsoleKey.DownArrow) lower = '+';

                    switch (lower)
                    {
                        case 'e': activeSet.ForEach(x => x.Prepare()); break;
                        case 'p': activeSet.ForEach(x => x.Play()); break;
                        case 'u': activeSet.ForEach(x => x.Pause()); break;
                        case 's': activeSet.ForEach(x => x.Stop()); break;
                        case 'r': activeSet.ForEach(x => x.Resume()); break;

                        case 'l':
                            int index = 0;
                            activeSet.ForEach(s =>
                            {
                                s.IsLooped = !s.IsLooped;
                                logger.Write(s.IsLooped ? "L" : " ", 3, 10 + index++);
                            });
                            break;

                        case 'v': FadeVolume(activeSet, true, 1, logger); break;
                        case 'b': FadeVolume(activeSet, false, 1, logger); break;

                        case 'f': FadeFilter(activeSet, true, 1, logger); break;
                        case 'g': FadeFilter(activeSet, false, 1, logger); break;

                        case '+':
                            logger.Write(" ", 0, 10 + sIdx);
                            sIdx++;
                            if (sIdx > streams.Length - 1) sIdx = 0;
                            logger.Write(">", 0, 10 + sIdx);
                            break;

                        case '-':
                            logger.Write(" ", 0, 10 + sIdx);
                            sIdx--;
                            if (sIdx < 0) sIdx = streams.Length - 1;
                            logger.Write(">", 0, 10 + sIdx);
                            break;

                        case 'q':
                            quit = true;
                            foreach (var cts in filterFades.Values) cts.Cancel();
                            foreach (var cts in volumeFades.Values) cts.Cancel();
                            foreach (var s in streams) s.Stop(); // nicer and more effective
                            foreach (var s in streams) s.Dispose();
                            break;
                    }
                }
            }
        }

        static readonly Dictionary<OggStream, CancellationTokenSource> volumeFades = new Dictionary<OggStream, CancellationTokenSource>();
        static void FadeVolume(List<OggStream> streams, bool @in, float duration, ConsoleLogger logger)
        {
            int index = 0;
            foreach (var stream in streams)
            {
                var from = stream.Volume;
                var to = @in ? 1f : 0;
                var speed = @in ? 1 - @from : @from;

                lock (volumeFades)
                {
                    CancellationTokenSource token;
                    bool found = volumeFades.TryGetValue(stream, out token);
                    if (found)
                    {
                        token.Cancel();
                        volumeFades.Remove(stream);
                    }
                }
                var sIdx = index++;
                logger.Write(@in ? "V" : "v", 4, 10 + sIdx);

                var cts = new CancellationTokenSource();
                lock (volumeFades) volumeFades.Add(stream, cts);

                var sw = Stopwatch.StartNew();
                OggStream s = stream;
                Task.Factory.StartNew(() =>
                {
                    float step;
                    do
                    {
                        step = (float)Math.Min(sw.Elapsed.TotalSeconds / (duration * speed), 1);
                        s.Volume = (to - @from) * step + @from;
                        Thread.Sleep(1000 / 60);
                    } while (step < 1 && !cts.Token.IsCancellationRequested);
                    sw.Stop();

                    if (!cts.Token.IsCancellationRequested)
                    {
                        lock (volumeFades) volumeFades.Remove(s);
                        logger.Write(" ", 4, 10 + sIdx);
                    }
                }, cts.Token);
            }
        }

        static readonly Dictionary<OggStream, CancellationTokenSource> filterFades = new Dictionary<OggStream, CancellationTokenSource>();
        static void FadeFilter(List<OggStream> streams, bool @in, float duration, ConsoleLogger logger)
        {
            int index = 0;
            foreach (var stream in streams)
            {
                var from = stream.LowPassHFGain;
                var to = @in ? 1f : 0;
                var speed = @in ? 1 - @from : @from;

                lock (filterFades)
                {
                    CancellationTokenSource token;
                    bool found = filterFades.TryGetValue(stream, out token);
                    if (found)
                    {
                        token.Cancel();
                        filterFades.Remove(stream);
                    }
                }
                var sIdx = index++;
                logger.Write(@in ? "F" : "f", 5, 10 + sIdx);
                var cts = new CancellationTokenSource();
                lock (filterFades) filterFades.Add(stream, cts);

                var sw = Stopwatch.StartNew();
                OggStream s = stream;
                Task.Factory.StartNew(() =>
                {
                    float step;
                    do
                    {
                        step = (float)Math.Min(sw.Elapsed.TotalSeconds / (duration * speed), 1);
                        s.LowPassHFGain = (to - @from) * step + @from;
                        Thread.Sleep(1000 / 60);
                    } while (step < 1 && !cts.Token.IsCancellationRequested);
                    sw.Stop();

                    if (!cts.Token.IsCancellationRequested)
                    {
                        lock (filterFades) filterFades.Remove(s);
                        logger.Write(" ", 5, 10 + sIdx);
                    }
                }, cts.Token);
            }
        }
    }

    public class ConsoleLogger : LoggerBase
    {
        static readonly object ConsoleMutex = new object();

        int lastX, lastY;
        readonly Dictionary<OggStream, Point> streamOffset = new Dictionary<OggStream, Point>();

        public void SetStreamIndex(OggStream stream, int index)
        {
            streamOffset.Add(stream, new Point(6, index + 10));
        }
        void SetHOffset(OggStream stream, int offset)
        {
            var originalOffset = streamOffset[stream];
            streamOffset[stream] = new Point(offset, originalOffset.Y);
        }

        public void Write(string text, int? x = null, int? y = null)
        {
            lock (ConsoleMutex)
            {
                Console.SetCursorPosition(Math.Min(Console.BufferWidth - 1, x ?? lastX), y ?? lastY);
                Console.Write(text);
                lastX = Console.CursorLeft;
                lastY = Console.CursorTop;
                Console.SetCursorPosition(0, Console.WindowHeight - 1);
            }
        }

        public override void Log(LogEventBoolean eventType, bool context)
        {
            switch (eventType)
            {
                case LogEventBoolean.IsOpenAlSoft:
                    Write("OpenAL Soft [" + (context ? "X" : " ") + "], ", 0, 5);
                    break;
                case LogEventBoolean.XRamSupport:
                    Write("X-RAM [" + (context ? "X" : " ") + "], ");
                    break;
                case LogEventBoolean.EfxSupport:
                    Write("Effect Extensions [" + (context ? "X" : " ") + "]");
                    break;
            }
        }

        public override void Log(LogEventSingle eventType, float context)
        {
            switch (eventType)
            {
                case LogEventSingle.MemoryUsage:
                    var usedHeap = context;

                    string[] sizes = { "B", "KB", "MB", "GB" };
                    int order = 0;
                    while (usedHeap >= 1024 && order + 1 < sizes.Length)
                    {
                        order++;
                        usedHeap = usedHeap / 1024;
                    }

                    Write(string.Format("Total memory : {0:0.###} {1}          ", usedHeap, sizes[order]), 0, 6);
                    break;
            }
        }

        public override void Log(LogEvent eventType, OggStream stream)
        {
            var p = streamOffset[stream];
            switch (eventType)
            {
                case LogEvent.BeginPrepare:
                    Write("(*", 7, p.Y);
                    SetHOffset(stream, 9);
                    break;
                case LogEvent.EndPrepare:
                    Write(")", p.X, p.Y);
                    SetHOffset(stream, p.X + 1);
                    break;
                case LogEvent.Play:
                    Write("{", p.X, p.Y);
                    SetHOffset(stream, p.X + 1);
                    break;
                case LogEvent.Stop:
                    Write("}", p.X, p.Y);
                    SetHOffset(stream, p.X + 1);
                    break;
                case LogEvent.Pause:
                    Write("[", p.X, p.Y);
                    SetHOffset(stream, p.X + 1);
                    break;
                case LogEvent.Resume:
                    Write("]", p.X, p.Y);
                    SetHOffset(stream, p.X + 1);
                    break;
                case LogEvent.Empty:
                    Write(new string(Enumerable.Repeat(' ', Console.BufferWidth - 6).ToArray()), 6, p.Y);
                    SetHOffset(stream, 7);
                    break;
                case LogEvent.NewPacket:
                    Write(".", p.X, p.Y);
                    SetHOffset(stream, p.X + 1);
                    break;
                case LogEvent.LastPacket:
                    Write("|", p.X, p.Y);
                    SetHOffset(stream, p.X + 1);
                    break;
                case LogEvent.BufferUnderrun:
                    Write("!", p.X, p.Y);
                    SetHOffset(stream, p.X + 1);
                    break;
            }
        }
    }
}