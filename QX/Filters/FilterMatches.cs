using System;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace QueueExchange {
    class FilterMatches : MustInitialize<XElement>, IQueueFilter {

        private readonly string value;

        public bool Pass(string message) {
            try {
                Regex reg = new Regex(value, RegexOptions.Compiled);
                Match match = reg.Match(message);
                return match.Success;
            }
            catch (Exception) {
                return false;
            }
        }

        public FilterMatches(XElement config) : base(config) {
            value = config.Attribute("value").Value;
        }
    }
}
