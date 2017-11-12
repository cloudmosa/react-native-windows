using Newtonsoft.Json.Linq;
using ReactNative.UIManager.Events;
using System;

namespace ReactNative.Views.TextInput
{
    class ReactTextInputKeyPressEvent : Event
    {
        private readonly string _key;

        public ReactTextInputKeyPressEvent(int viewTag, string key)
            : base(viewTag)
        {
            _key = key;
        }

        public override string EventName
        {
            get
            {
                return "topKeyPress";
            }
        }

        public override bool CanCoalesce
        {
            get
            {
                return false;
            }
        }

        public override void Dispatch(RCTEventEmitter eventEmitter)
        {
            var eventData = new JObject
            {
                { "target", ViewTag },
                { "key", _key },
            };

            eventEmitter.receiveEvent(ViewTag, EventName, eventData);
        }
    }
}
