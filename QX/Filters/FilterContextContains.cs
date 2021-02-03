using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Caching;
using System.Security.Cryptography;
using System.Xml;
using System.Xml.Linq;

namespace QueueExchange
{
    class FilterContextContains : MustInitialize<XElement>, IQueueFilter, IDisposable
    {

        // private QueueAbstract queue;
        private readonly string contextCacheKeyXPath;
        private readonly double contextCacheExpiry = 10.0;
        private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly bool useMessageAsKey = false;
        public MemoryCache _contextCache;

        public bool Pass(string message)
        {

            if (useMessageAsKey)
            {
                using (SHA256 mySHA256 = SHA256.Create())
                {
                    using (var stream = GenerateStreamFromString(message))
                    {
                        byte[] hashValue = mySHA256.ComputeHash(stream);
                        string nodeValue = BitConverter.ToString(hashValue);

                        bool inCache = _contextCache.Contains(nodeValue);

                        if (!inCache)
                        {
                            logger.Info($"Adding Key:{nodeValue}  to Context Cache");
                            _contextCache.AddOrGetExisting(nodeValue, nodeValue, DateTime.Now.AddSeconds(this.contextCacheExpiry));
                        }
                        else
                        {
                            logger.Info($"NOT Adding Key:{nodeValue}  to Context Cache");
                        }

                        return !inCache;
                    }
                }
            }

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(message);
                XmlNamespaceManager ns = new XmlNamespaceManager(doc.NameTable);

                foreach (KeyValuePair<string, string> item in Exchange.nsDict)
                {
                    ns.AddNamespace(item.Key, item.Value);
                }
                XmlNode node = doc.SelectSingleNode(contextCacheKeyXPath, ns);
                string nodeValue = node.InnerText;

                bool inCache = _contextCache.Contains(nodeValue);

                if (!inCache)
                {
                    logger.Info($"Adding Key:{nodeValue}  to Context Cache");
                    _contextCache.AddOrGetExisting(nodeValue, nodeValue, DateTime.Now.AddSeconds(this.contextCacheExpiry));
                }
                else
                {
                    logger.Info($"NOT Adding Key:{nodeValue}  to Context Cache");
                }

                return !inCache;

            }
            catch (Exception ex)
            {
                logger.Info(ex.Message);
                return false;
            }
        }

        public FilterContextContains(XElement config, QueueAbstract queue) : base(config)
        {

            _contextCache = new MemoryCache(queue.queueName);

            try
            {
                contextCacheKeyXPath = config.Attribute("contextCacheKeyXPath").Value;
            }
            catch (Exception)
            {
                contextCacheKeyXPath = null;
            }

            try
            {
                this.contextCacheExpiry = double.Parse(config.Attribute("contextCacheExpiry").Value);
            }
            catch (Exception)
            {
                this.contextCacheExpiry = 10.0;
            }

            try
            {
                this.useMessageAsKey = bool.Parse(config.Attribute("useMessageAsKey").Value);
            }
            catch (Exception)
            {
                this.useMessageAsKey = false;
            }

            //          this.queue = queue;
        }

        public static Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            using (var writer = new StreamWriter(stream))
            {
                writer.Write(s);
                writer.Flush();
            }
            stream.Position = 0;
            return stream;
        }

        public void Dispose()
        {
            try
            {
                _contextCache.Dispose();
            }
            catch { }
        }
    }
}
