using System;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace QueueExchange {
    class QXMonitor {

        /*
         * Static classfor logging details
         * 
         * The <monitor> element is the same as an output queue. If it is defined, then
         * the log messages are sent there. The monitor can be any valid Output destination.
         * 
         */

        private readonly static QueueAbstract monitorQueue = null;
        private readonly static bool monitorEnabled = false;
        private readonly static object logLock = new object();
        private readonly static bool json = false;
        static QXMonitor() {


            XDocument doc = XDocument.Load(Exchange.configFileName);
            XElement loggerQueueDefn = doc.Descendants("monitor").FirstOrDefault();

            try {
                if (loggerQueueDefn != null) {
                    monitorQueue = new QueueFactory().GetQueue(loggerQueueDefn);
                    monitorQueue.isLogger = true;
                    monitorEnabled = true;
                } else {
                    monitorEnabled = false;
                }
            } catch (Exception) {
                monitorEnabled = false;
            }

            try {
                json = bool.Parse(loggerQueueDefn.Attribute("json").Value);
            } catch (Exception) {
                json = false;
            }
        }
        public static void Log(ExchangeMonitorMessage monMess) {
            if (!monitorEnabled) {
                return;
            }
            lock (logLock) {
                if (json) {
                    _ = Task.Run(() => monitorQueue.SendToOutputAsync(new ExchangeMessage(monMess.ToJSONString())));
                } else {
                    _ = Task.Run(() => monitorQueue.SendToOutputAsync(new ExchangeMessage(monMess.ToString())));
                }
            }
        }
    }
}
