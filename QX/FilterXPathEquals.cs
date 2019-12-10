﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace QueueExchange {
    class FilterXPathEquals : MustInitialize<XElement>, IQueueFilter {

        private string nodePath;
        private bool equals = false;
        private bool matches = false;
        private string value;

        public bool Pass(string message) {
            try {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(message);
                XmlNamespaceManager ns = new XmlNamespaceManager(doc.NameTable);

                foreach (KeyValuePair<string, string> item in Exchange.nsDict) {
                    ns.AddNamespace(item.Key, item.Value);
                }
                XmlNode node = doc.SelectSingleNode(nodePath, ns);
                string nodeValue = node.InnerText;

                if (equals) {
                    return (nodeValue == value);
                }

                if (matches) {
                    Regex reg = new Regex(value, RegexOptions.Compiled);
                    Match match = reg.Match(nodeValue);
                    return match.Success;
                }
            } catch (Exception) {
                return false;
            }

            return false;
        }

        public FilterXPathEquals(XElement config) : base(config) {
            equals = config.Attribute("type").Value == "equals";
            matches = config.Attribute("type").Value == "matches";

            nodePath = config.Attribute("xpath").Value;
            value = config.Attribute("value").Value;
        }
    }
}
