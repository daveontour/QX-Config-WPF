using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace QueueExchange {
    class FilterDateRange : MustInitialize<XElement>, IQueueFilter {

        // Filters out messages that do not fall withing the specified window
        // xpath specifies the date under test in YYYY-MM-DD format
        // The fromOffset and toOffset are the number of days relative to NOW() (can be negative)

        private readonly string nodePath;
        private readonly int fromOffset;
        private readonly int toOffset;

        public bool Pass(string message) {
            try {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(message);
                XmlNamespaceManager ns = new XmlNamespaceManager(doc.NameTable);

                XDocument config = XDocument.Load(Exchange.configFileName);
                IEnumerable<XElement> nsDefn = from n in config.Descendants("namespace") select n;

                // Load any of the configured namespaces for possible use
                foreach (XElement n in nsDefn) {
                    ns.AddNamespace(n.Attribute("prefix").Value, n.Attribute("uri").Value);
                }
                XmlNode node = doc.SelectSingleNode(nodePath, ns);

                DateTime check = DateTime.Parse(node.InnerText);
                DateTime from = DateTime.Now.AddDays(fromOffset);
                from = new DateTime(from.Year, from.Month, from.Day, 0, 0, 0);
                DateTime to = DateTime.Now.AddDays(toOffset);
                to = new DateTime(to.Year, to.Month, to.Day, 23, 59, 59);

                return from <= check && check <= to;

            } catch (Exception) {
                // Could occur if the document is not XML or does not have the configured path and data in the correct format.
                return false;
            }
        }

        public FilterDateRange(XElement config) : base(config) {
            fromOffset = Int32.Parse(config.Attribute("fromOffset").Value);
            toOffset = Int32.Parse(config.Attribute("toOffset").Value);
            nodePath = config.Attribute("xpath").Value;
        }
    }
}
