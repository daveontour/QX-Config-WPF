using System.Xml.Linq;

namespace QueueExchange {
    class FilterContains : MustInitialize<XElement>, IQueueFilter {

        private string value;

        public bool Pass(string message) {
            if (value == null) {
                return true;
            }
            return message.Contains(value);
        }

        public FilterContains(XElement config) : base(config) {
            value = config.Attribute("value").Value;
        }
    }
}
