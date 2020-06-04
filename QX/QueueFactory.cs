using System.Xml.Linq;

namespace QueueExchange
{
    public class QueueFactory
    {
        public QueueFactory() { }

        public QueueAbstract GetQueue(XElement ep, System.IProgress<MonitorMessage> monitorMessageProgress)
        {

            string queueType = ep.Attribute("type").Value;

            QueueAbstract queue;

            switch (queueType)
            {
                case "MSMQ":
                    queue = new QEMSMQ(ep, monitorMessageProgress);
                    break;
                case "MQ":
                    queue = new QEMQ(ep, monitorMessageProgress);
                    break;
                case "HTTP":
                    queue = new QEHTTP(ep, monitorMessageProgress);
                    break;
                case "REST":
                    queue = new QERest(ep, monitorMessageProgress);
                    break;
                case "KAFKA":
                    queue = new QEKafka(ep, monitorMessageProgress);
                    break;
                case "RABBITDEFEX":
                    queue = new QERabbitDefExchange(ep, monitorMessageProgress);
                    break;
                case "FILE":
                    queue = new QEFile(ep, monitorMessageProgress);
                    break;
                case "SINK":
                    queue = new QESink(ep, monitorMessageProgress);
                    break;
                case "TESTSOURCE":
                    queue = new QESink(ep, monitorMessageProgress);
                    break;
                default:
                    queue = null;
                    break;
            }

            return queue;
        }

        public IQueueFilter GetFilter(XElement ep, QueueAbstract queue)
        {

            string type = ep.Name.ToString();

            IQueueFilter filter = null;

            switch (type)
            {
                case "xpexists":
                    filter = new FilterXPathExists(ep);
                    break;
                case "xpequals":
                case "xpmatches":
                    filter = new FilterXPathEquals(ep);
                    break;
                case "dateRange":
                    filter = new FilterDateRange(ep);
                    break;
                case "bool":
                    filter = new FilterBool(ep);
                    break;
                case "contains":
                    filter = new FilterContains(ep);
                    break;
                case "equals":
                    filter = new FilterEquals(ep);
                    break;
                case "matches":
                    filter = new FilterMatches(ep);
                    break;
                case "length":
                    filter = new FilterLength(ep);
                    break;
                case "contextContains":
                    filter = new FilterContextContains(ep, queue);
                    break;
            }

            return filter;
        }
    }
}
