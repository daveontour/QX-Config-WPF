using System;
using System.Xml.Linq;

namespace QueueExchange {
    class FilterLength : MustInitialize<XElement>, IQueueFilter {

        private int length = 0;

        public bool Pass(string message) {
            if (length == 0) {
                return true;
            }
            return message.Length >= length;
        }

        public FilterLength(XElement config) : base(config) {
            length = Int32.Parse(config.Attribute("value").Value);
        }
    }
}
