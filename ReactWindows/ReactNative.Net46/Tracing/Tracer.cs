using System;
using System.Diagnostics.Tracing;

namespace ReactNative.Tracing
{
    /// <summary>
    /// Temporary NullTracing helpers for the application.
    /// </summary>
    public static class Tracer
    {
        /// <summary>
        /// Trace ID for bridge events.
        /// </summary>
        public const int TRACE_TAG_REACT_BRIDGE = 0;

        /// <summary>
        /// Trace ID for application events.
        /// </summary>
        public const int TRACE_TAG_REACT_APPS = 1;

        /// <summary>
        /// Trace ID for view events.
        /// </summary>
        public const int TRACE_TAG_REACT_VIEW = 2;

        /// <summary>
        /// Create a null logging activity builder.
        /// </summary>
        /// <param name="tag">The trace tag.</param>
        /// <param name="name">The event name.</param>
        /// <returns>The null logging activity builder with a fake Start method.</returns>
        public static LoggingActivityBuilder Trace(int tag, string name)
        {
            try
            {
                return new LoggingActivityBuilder(ReactNativeWindowsEventSource.INSTANCE, name, tag);
            }
            catch (Exception) { };
        }

        /// <summary>
        /// Write an event.
        /// </summary>
        /// <param name="tag">The trace tag.</param>
        /// <param name="eventName">The event name.</param>
        public static void Write(int tag, string eventName)
        {
            try
            {
                ReactNativeWindowsEventSource.INSTANCE.Write(tag, eventName);
            }
            catch (Exception) { };
        }

        /// <summary>
        /// Write an error event.
        /// </summary>
        /// <param name="tag">The trace tag.</param>
        /// <param name="eventName">The event name.</param>
        /// <param name="ex">The exception.</param>
        public static void Error(int tag, string eventName, Exception ex)
        {
            try
            {
                ReactNativeWindowsEventSource.INSTANCE.Error(tag, eventName, ex != null ? ex.ToString() : "");
            }
            catch (Exception) { };
        }
    }

    [EventSource(Name = "ReactNativeWindows")]
    public sealed class ReactNativeWindowsEventSource : EventSource
    {
        public static ReactNativeWindowsEventSource INSTANCE = new ReactNativeWindowsEventSource();

        [Event(1, Level = EventLevel.Informational)]
        public void Write(int tag, string eventName)
        {
            if (IsEnabled())
                WriteEvent(1, tag, eventName);
        }

        [Event(2, Level = EventLevel.Error)]
        public void Error(int tag, string eventName, string exception)
        {
            if (IsEnabled())
                WriteEvent(2, tag, eventName, exception);
        }

        [Event(3, Level = EventLevel.Informational)]
        public void Trace(int tag, string eventName, long elapsedTicks)
        {
            if (IsEnabled())
                WriteEvent(3, tag, eventName, elapsedTicks);
        }
    }
}
