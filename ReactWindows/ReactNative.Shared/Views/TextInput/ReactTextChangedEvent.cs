using Newtonsoft.Json.Linq;
using ReactNative.UIManager.Events;
using System;

namespace ReactNative.Views.TextInput
{
    /// <summary>
    /// Event emitted by <see cref="ReactTextInputManager"/> native view when
    /// text changes.
    /// </summary>
    class ReactTextChangedEvent : Event
    {
        private readonly string _text;
        private readonly string _compositionText;
        private readonly int _eventCount;

        public enum Reason
        {
            TextChanged,
            SizeChanged,
        }
        private readonly Reason _reason;

        /// <summary>
        /// Instantiates a <see cref="ReactTextChangedEvent"/>.
        /// </summary>
        /// <param name="viewTag">The view tag.</param>
        /// <param name="text">The text.</param>
        /// <param name="eventCount">The event count.</param>
        public ReactTextChangedEvent(int viewTag, string text, int eventCount)
            : this(viewTag, text, "", eventCount, Reason.TextChanged)
        {
        }

        public ReactTextChangedEvent(int viewTag, string text, int eventCount, Reason reason)
            : this(viewTag, text, "", eventCount, reason)
        {
        }

        public ReactTextChangedEvent(int viewTag, string text, string compositionText, int eventCount, Reason reason)
            : base(viewTag)
        {
            _text = text;
            _compositionText = compositionText;
            _eventCount = eventCount;
            _reason = reason;
        }

        /// <summary>
        /// The name of the event.
        /// </summary>
        public override string EventName
        {
            get
            {
                return "topChange";
            }
        }

        /// <summary>
        /// Text change events cannot be coalesced.
        /// </summary>
        /// <remarks>
        /// Return <code>false</code> if the event can never be coalesced.
        /// </remarks>
        public override bool CanCoalesce
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Push the event up to the event emitter.
        /// </summary>
        /// <param name="rctEventEmitter">The event emitter.</param>
        public override void Dispatch(RCTEventEmitter rctEventEmitter)
        {
            var eventData = new JObject
            {
                { "text", _text },
                { "compositionText", _compositionText },
                { "eventCount", _eventCount },
                { "target", ViewTag },
            };
            switch (_reason)
            {
                case Reason.SizeChanged:
                    eventData["_reason"] = "sizeChanged";
                    break;
                case Reason.TextChanged:
                    eventData["_reason"] = "textChanged";
                    break;
            }

            rctEventEmitter.receiveEvent(ViewTag, EventName, eventData);
        }
    }
}
