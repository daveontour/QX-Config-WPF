using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;

namespace QueueExchange
{
    class FilterXPathExists : MustInitialize<XElement>, IQueueFilter
    {

        // Filter that evaluates whether the specified XPath elements exists in the message

        private string nodePath;
        public bool Pass(string message)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(message);
                XmlNamespaceManager ns = new XmlNamespaceManager(doc.NameTable);

                foreach (KeyValuePair<string, string> item in Exchange.nsDict)
                {
                    ns.AddNamespace(item.Key, item.Value);
                }

                XmlNodeList nodes = doc.SelectNodes(nodePath, ns);
                bool result = nodes.Count > 0;
                return result;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public FilterXPathExists(XElement config) : base(config)
        {
            nodePath = config.Attribute("xpath").Value;
        }
    }
}
