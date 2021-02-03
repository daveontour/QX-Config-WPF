using System.Xml.Linq;

namespace QueueExchange {
    class FilterEquals : MustInitialize<XElement>, IQueueFilter {

        private readonly string value;

        public bool Pass(string message) {
            if (value == null) {
                return true;
            }
            return message.Equals(value);
        }

        public FilterEquals(XElement config) : base(config) {
            value = config.Attribute("value").Value;
        }
    }
}
