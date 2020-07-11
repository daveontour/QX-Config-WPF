using System;
using System.Xml.Linq;

namespace QueueExchange
{
    class FilterContains : MustInitialize<XElement>, IQueueFilter
    {

        private readonly string value;

        public bool Pass(string message)
        {
            if (value == null)
            {
                return true;
            }
            bool res = message.Contains(value);
            if (res)
            {
                Console.WriteLine(value);
            }
            return message.Contains(value);
        }

        public FilterContains(XElement config) : base(config)
        {
            value = config.Attribute("value").Value;
        }
    }
}
