using System;
using System.Diagnostics;
using System.Reactive.Disposables;

namespace ReactNative.Tracing
{
    /// <summary>
    /// Temporary LoggingActivityBuilder.
    /// </summary>
    public class LoggingActivityBuilder : IDisposable
    {
        private readonly ReactNativeWindowsEventSource _target;
        private readonly string _name;
        private readonly int _tag;

        private readonly Stopwatch _timer;

        /// <summary>
        /// Constructor of LoggingActivityBuilder
        /// </summary>
        /// <param name="target">ReactNativeWindowsEventSource instance</param>
        /// <param name="name">event name</param>
        /// <param name="tag">Tracer tag</param>
        public LoggingActivityBuilder(ReactNativeWindowsEventSource target, string name, int tag)
        {
            _target = target;
            _name = name;
            _tag = tag;

            _timer = new Stopwatch();
            _timer.Start();
        }

        /// <summary>
        /// Cleanup logging activity and will calculate elapsed ticks and send to EventSource
        /// </summary>
        public void Dispose()
        {
            _timer.Stop();
            _target.Trace(_tag, _name, _timer.ElapsedTicks);
        }

        /// <summary>
        /// Dummy method to satisfy interface requirements.
        /// </summary>
        /// <returns>An empty disposable object.</returns>
        public IDisposable Create()
        {
            return this;
        }
    }
}
