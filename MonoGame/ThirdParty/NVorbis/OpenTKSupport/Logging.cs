using System;

namespace NVorbis.OpenTKSupport
{
    public class NullLogger : ILogger
    {
        public static readonly ILogger Default = new NullLogger();

        public void Log(LogEventBoolean eventType, Func<bool> context) { }
        public void Log(LogEventBoolean eventType, bool context) { }
        public void Log(LogEventSingle eventType, Func<float> context) { }
        public void Log(LogEventSingle eventType, float context) { }
        public void Log(LogEvent eventType, OggStream stream) { }
    }

    public abstract class LoggerBase : ILogger
    {
        public void Log(LogEventBoolean eventType, Func<bool> context) { Log(eventType, context()); }
        public abstract void Log(LogEventBoolean eventType, bool context);

        public void Log(LogEventSingle eventType, Func<float> context) { Log(eventType, context()); }
        public abstract void Log(LogEventSingle eventType, float context);

        public abstract void Log(LogEvent eventType, OggStream stream);
    }

    public interface ILogger
    {
        void Log(LogEventBoolean eventType, Func<bool> context);
        void Log(LogEventBoolean eventType, bool context);
        void Log(LogEventSingle eventType, Func<float> context);
        void Log(LogEventSingle eventType, float context);
        void Log(LogEvent eventType, OggStream stream);
    }

    public enum LogEventBoolean
    {
        IsOpenAlSoft,
        XRamSupport,
        EfxSupport,
    }
    public enum LogEventSingle
    {
        MemoryUsage,
    }
    public enum LogEvent
    {
        BeginPrepare,
        EndPrepare,
        Play,
        Stop,
        Pause,
        Resume,
        Empty,
        NewPacket,
        LastPacket,
        BufferUnderrun
    }
}
