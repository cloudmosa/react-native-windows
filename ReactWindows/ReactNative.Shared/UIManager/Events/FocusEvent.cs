using Newtonsoft.Json.Linq;
using System;

namespace ReactNative.UIManager.Events
{
#if CUSTOMIZATION_CLOUDMOSA
    public class FocusEvent : Event
#else
    class FocusEvent : Event
#endif
    {
        public FocusEvent(int viewTag)
            : base(viewTag)
        {
        }

        public override string EventName
        {
            get
            {
                return "topFocus";
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
                };

            eventEmitter.receiveEvent(ViewTag, EventName, eventData);
        }
    }
}
