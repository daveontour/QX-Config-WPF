using System.Xml.Linq;

namespace QueueExchange
{
    class FilterBool : MustInitialize<XElement>, IQueueFilter
    {

        // Simple Filter that return the boolean value set, regardless of message content
        // (used for testing Expressions)

        private readonly bool value = true;

        public bool Pass(string message)
        {
            return value;
        }

        public FilterBool(XElement config) : base(config)
        {
            value = bool.Parse(config.Element("value").Value);
        }
    }
}
