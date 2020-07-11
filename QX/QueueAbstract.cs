using Saxon.Api;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Messaging;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;

namespace QueueExchange
{
    public abstract class QueueAbstract
    {

        public string name;
        protected string connection;
        private readonly IProgress<QueueMonitorMessage> monitorMessageProgress;
        protected XElement definition;
        protected bool bTransform = false;
        protected bool createQueue = false;
        public string queueName;
        protected string undeliverableQueue;
        protected static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        protected int sendTimeout;
        protected int sendRetry;
        protected int maxRetry = 10;
        protected string xslVersion = "1.0";
        private readonly List<string> styleSheets = new List<string>();
        private readonly Expression expression = null;
        private readonly QueueFactory fact = new QueueFactory();
        public int priority = 0;
        public bool lastUsed = false;

        protected string bufferQueueName;
        public bool isLogger = false;
        public bool OK_TO_RUN = true;
        private readonly QueueAbstract altQueue = null;
        private readonly IQueueFilter topLevelFilter = null;
        public bool isValid = false;
        protected static object undevlLock = new object();
        protected string xpathDestination = null;
        protected string xpathContentDestination;
        public string id;


        protected Monitor mon = Monitor.Instance;
        protected MessagePriority mqPriority;
        public string pipeInputQueueName;

        public abstract bool SetUp();
        public abstract Task<ExchangeMessage> SendToOutputAsync(ExchangeMessage message);

        virtual public async Task StartListener()
        {
            await Task.Run(() => { });
        }

        public void Stop()
        {

        }

        public QueueAbstract(string value)
        {

        }
        public QueueAbstract(XElement defn, IProgress<QueueMonitorMessage> monitorMessageProgress)
        {
            this.monitorMessageProgress = monitorMessageProgress;
            this.definition = defn;

            try
            {
                var stylesheet = definition.Attribute("stylesheet").Value;
                this.styleSheets = stylesheet.Split(',').ToList<string>();
                this.bTransform = true;
            }
            catch (Exception)
            {
                this.styleSheets = new List<string>();
                this.bTransform = false;
            }

            try
            {
                this.queueName = definition.Attribute("queue").Value;
            }
            catch (Exception)
            {
                this.queueName = "undefined";
            }

            try
            {
                this.id = definition.Attribute("id").Value;
            }
            catch (Exception)
            {
                this.id = Guid.NewGuid().ToString();
            }

            try
            {
                this.name = definition.Attribute("name").Value;
            }
            catch (Exception)
            {
                this.name = this.queueName;
            }

            try
            {
                this.createQueue = bool.Parse(definition.Attribute("createQueue").Value);
            }
            catch (Exception)
            {
                this.createQueue = false;
            }

            try
            {
                this.xslVersion = definition.Attribute("xslVersion").Value;
            }
            catch (Exception)
            {
                this.xslVersion = "1.0";
            }

            try
            {
                this.maxRetry = Convert.ToInt32(definition.Attribute("maxRetry").Value);
            }
            catch (Exception)
            {
                this.maxRetry = 10;
            }
            try
            {
                this.priority = Convert.ToInt32(definition.Attribute("priority").Value);

                switch (priority)
                {
                    case 0:
                        mqPriority = MessagePriority.Lowest;
                        break;
                    case 1:
                        mqPriority = MessagePriority.VeryLow;
                        break;
                    case 2:
                        mqPriority = MessagePriority.Low;
                        break;
                    case 3:
                        mqPriority = MessagePriority.Normal;
                        break;
                    case 4:
                        mqPriority = MessagePriority.AboveNormal;
                        break;
                    case 5:
                        mqPriority = MessagePriority.High;
                        break;
                    case 6:
                        mqPriority = MessagePriority.VeryHigh;
                        break;
                    case 7:
                        mqPriority = MessagePriority.Highest;
                        break;
                }
            }
            catch (Exception)
            {
                this.priority = 2;
                mqPriority = MessagePriority.Normal;
            }

            try
            {
                undeliverableQueue = defn.Attribute("undeliverableQueue").Value;
                CreateQueue(undeliverableQueue);
            }
            catch (Exception)
            {
                undeliverableQueue = null;
            }

            try
            {
                bufferQueueName = definition.Attribute("bufferQueueName").Value;
                CreateQueue(bufferQueueName, true);
            }
            catch (Exception)
            {
                bufferQueueName = null;
            }


            try
            {
                xpathDestination = definition.Attribute("xpathDestination").Value;
            }
            catch (Exception)
            {
                xpathDestination = null;
            }
            try
            {
                xpathContentDestination = definition.Attribute("xpathContentDestination").Value;
            }
            catch (Exception)
            {
                xpathContentDestination = null;
            }

            // Add any filter expresion
            XElement filtersDefn = defn.Element("filter");
            if (filtersDefn != null)
            {
                // Add the alternate queue to send to if the expression fails (optional)
                try
                {
                    altQueue = fact.GetQueue(filtersDefn.Element("altqueue"), this.monitorMessageProgress);
                }
                catch (Exception)
                {
                    altQueue = null;
                }

                /*
                 * At the top level, there is only one Expresssion, which it self can be a compound
                 * expression or a single data filter
                 * 
                 * When the Expression itself is constucted, it recurssive creates all the Expression o
                 * filters configured under it
                 */

                // Cycle through the expressions types (and, or, not, xor) to see if any exist
                foreach (string eType in Expression.expressionTypes)
                {
                    XElement exprDefn = filtersDefn.Element(eType);
                    if (exprDefn != null)
                    {
                        expression = new Expression(exprDefn, this);
                    }
                }

                // Cycle through the data filter types (and, or, not, xor) to see if any exist
                foreach (string fType in Expression.filterTypes)
                {
                    XElement filtDefn = filtersDefn.Element(fType);
                    if (filtDefn != null)
                    {
                        topLevelFilter = fact.GetFilter(filtDefn, this);
                    }
                }
            }
            // Call the instance specific setup
            this.isValid = this.SetUp();
        }
        protected void CreateQueue(string qName)
        {
            CreateQueue(qName, createQueue);
        }
        protected void CreateQueue(string qName, bool createOK)
        {

            // For local MSMQ queues only
            if (!queueName.StartsWith("FormatName"))
            {

                if (!qName.Contains("$"))
                {
                    qName = ".\\private$\\" + qName;
                }

                if (!MessageQueue.Exists(qName) && createOK)
                {
                    using (MessageQueue t = MessageQueue.Create(qName))
                    {
                        logger.Debug($"Queue Created for {qName}");
                    }
                }
            }
        }
        public async Task<ExchangeMessage> Send(ExchangeMessage xm)
        {
            //Do any filtering or transformation first.
            try
            {
                xm = PreAndPostProcess(xm);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }

            // Check the result of the filtering and transformations
            if (!xm.pass || xm.payload == null)
            {
                xm.status = $"Message blocked by filter to {queueName}";
                xm.pass = false;
                QXLog(xm?.uuid, "Output Node", "Messages did not pass filter", "PROGRESS");
                return xm;
            }
            else
            {
                QXLog(xm?.uuid, "Output Node", "Sending Message", "PROGRESS");
                // Send it to the destination. 
                // The state of the message should be updated by the particuar output
                xm = await SendToOutputAsync(xm);

                return xm;
            }
        }
        public void SendToPipe(string message)
        {
            //Check to see if it passes filters and transformations
            ExchangeMessage xm = new ExchangeMessage(message);
            xm = PreAndPostProcess(xm);
            if (xm == null || xm.payload == null)
            {
                logger.Info($"Message did not pass transformation");
                return;
            }
            if (!xm.pass)
            {
                logger.Info($"Message did not pass filter");
                return;
            }
            try
            {
                Message myMessage = new Message(message, new ActiveXMessageFormatter())
                {
                    Priority = mqPriority
                };
                MessageQueue pipeInputQueue = new MessageQueue(pipeInputQueueName);
                pipeInputQueue.Send(myMessage);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                logger.Error(ex.StackTrace);
            }

        }
        public ExchangeMessage PreAndPostProcess(ExchangeMessage xm)
        {

            QXLog(xm?.uuid, "Output Node: Message Recieved From Pipe", null, "PROGRESS");


            // payload is the actual content of the message to be send
            string message = xm.payload;

            // After the delivery or prior to sending a message, it can be filtered or 
            // transformed according to the configuration for each queue

            // The Expression object manages the evaluation of all the filters
            if (expression != null)
            {

                bool pass = expression.Pass(message);

                logger.Info($"Top Expression Evaluated {pass}");
                if (!pass)
                {
                    if (altQueue != null)
                    {
                        logger.Info($"Sending to Alt Queue {altQueue.name}");
                        QXLog(xm?.uuid, "Message did not pass filter", "Sending to Alt Queue", "PROGRESS");
                        Task.Run(async () => { _ = await altQueue.Send(xm); });
                    }
                    else
                    {
                        QXLog(xm?.uuid, "Message did not pass filter", "No Alt Queue Configured", "WARNING");
                    }
                    xm.pass = false;
                    return xm;
                }
                else
                {
                    xm.pass = true;
                }
            }

            if (topLevelFilter != null)
            {

                bool pass = topLevelFilter.Pass(message);

                logger.Info($"Top Filter Evaluated {pass}");
                if (!pass)
                {
                    if (altQueue != null)
                    {
                        logger.Info($"Sending to Alt Queue {altQueue.name}");
                        QXLog(xm?.uuid, "Message did not pass filter", "Sending to Alt Queue", "PROGRESS");
                        _ = altQueue.Send(xm);
                    }
                    else
                    {
                        QXLog(xm?.uuid, "Message did not pass filter", "No Alt Queue Configured", "WARNING");
                    }
                    xm.pass = false;
                    return xm;
                }
                else
                {
                    xm.pass = true;
                }
            }


            // If a XSLT transform has been specified
            if (bTransform)
            {
                QXLog(xm?.uuid, "Starting Message Transformation", null, "PROGRESS");
                message = Transform(message, xslVersion);
                QXLog(xm?.uuid, "Message Transformation Complete", null, "PROGRESS");

                xm.transformed = true;
            }
            else
            {
                xm.transformed = false;
            }

            if (message == null || message.Length == 0)
            {
                logger.Info("Message blocked by XSL Transform of Zero Length");
                xm.payload = null;
                xm.status = "Message blocked by XSL Transform. Null or Zero Length";

                QXLog(xm?.uuid, "Messaage Transformation", "Transformation resulted in a message of zero length", "WARNING");

                return xm;
            }

            xm.payload = message;
            QXLog(xm?.uuid, "Output Node: Post Processing Complete", null, "PROGRESS");
            return xm;
        }
        public string Transform(string message, string version)
        {

            // XSLT Transformation can be either performed by the internal XSLT 1.0 processor
            // or via the SAXON processor, which supports XSLT 3.0

            logger.Info("Performing XSLT transform before sending to " + this.queueName);

            if (version == "1.0")
            {
                //Use the in built XSL 1.0 MS Processor
                logger.Info("Performing XSLT transform using XSL 1.0 Processor");

                try
                {

                    foreach (string sheet in styleSheets)
                    {
                        FileInfo xslt = new FileInfo(sheet);
                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(message);
                        XDocument newTree = new XDocument();
                        using (XmlWriter writer = newTree.CreateWriter())
                        {
                            // Load the style sheet.  
                            XslCompiledTransform xs = new XslCompiledTransform();
                            xs.Load(sheet);

                            // Execute the transform and output the results to a writer.  
                            xs.Transform(doc, writer);
                        }
                        message = newTree.ToString();
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("XSL 1.0 Transformation Error");
                    logger.Error(ex.Message);
                    return null;
                }
                logger.Info("XSLT transform using 1.0 Processor Complete");
                return message;


            }
            else
            {
                //Use the SAXON 2.0/3.0 Processor (not as fast)

                logger.Info("Performing XSLT transform using Saxon XSL 3.0 Processor");
                try
                {
                    foreach (string sheet in styleSheets)
                    {

                        FileStream stream = File.Open(new FileInfo(sheet).ToString(), FileMode.Open);

                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(message);

                        Processor processor = new Processor();

                        XdmNode input = processor.NewDocumentBuilder().Build(doc);
                        Xslt30Transformer transformer = processor.NewXsltCompiler().Compile(stream).Load30();

                        Serializer serializer = processor.NewSerializer();
                        StringWriter strWriter = new StringWriter();
                        serializer.SetOutputWriter(strWriter);

                        // Transform the source XML and serialize the result document
                        transformer.ApplyTemplates(input, serializer);

                        message = strWriter.ToString();
                    }


                    logger.Info("XSLT transform using 3.0 Processor Complete");
                    return message;
                }
                catch (Exception ex)
                {
                    logger.Error("XSL 3.0 Transformation Error");
                    logger.Error(ex.Message);
                    return null;
                }
            }
        }
        protected ExchangeMessage SetDestinationFromMessage(ExchangeMessage mess)
        {

            if (xpathDestination != null)
            {
                queueName = GetDestinationFromMessage(mess.payload, true);
                if (queueName == null)
                {
                    mess.status = "Destination could not be found in the XPATH expression";
                    mess.destinationSet = false;
                    return mess;
                }
                else
                {
                    mess.destinationSet = true;
                    return mess;
                }
            }

            if (xpathContentDestination != null)
            {
                queueName = GetDestinationFromMessage(mess.payload, false);
                if (queueName == null)
                {
                    mess.status = "Destination Topic could not be found in the XPATH expression";
                    mess.destinationSet = false;
                    return mess;
                }
                else
                {
                    mess.destinationSet = true;
                    return mess;
                }
            }

            mess.destinationSet = false;
            return mess;
        }
        protected string GetDestinationFromMessage(string message, bool path)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(message);
            XmlNamespaceManager ns = new XmlNamespaceManager(doc.NameTable);

            foreach (KeyValuePair<string, string> item in Exchange.nsDict)
            {
                ns.AddNamespace(item.Key, item.Value);
            }


            string topic;

            try
            {
                if (path)
                {
                    XmlNode node = doc.SelectSingleNode(xpathDestination, ns);
                    topic = node.LocalName;
                }
                else
                {
                    XmlNode node = doc.SelectSingleNode(xpathContentDestination, ns);
                    topic = node.InnerText;
                }
            }
            catch (Exception ex)
            {
                logger.Error($"XPATH KEY ERROR {ex.Message}");
                return null;
            }
            return topic;
        }
        public void SendToUndeliverableQueue(ExchangeMessage xm)
        {

            // If messages cant be delivered, they can be sent to a local undeliverable queue
            // defined on a per queue basis. No reason why the one undeliverable queue cant be used
            // for all of the queue. 


            if (isLogger)
            {
                // If this is a logger, failure messages won't be sent to the undeliverable queue
                logger.Error("Log Message could not be sent to logger");
                return;
            }

            QXLog(xm?.uuid, "Output Node Send Failure", "Message Could Not Be Delivered to the Output Node", "PROGRESS");

            if (undeliverableQueue == null)
            {
                logger.Debug($"No Undeliverable Message Queue has been defined for {queueName}");
                QXLog(xm?.uuid, "Output Node Send Failure", "No Undeliverable Queue Defined", "WARNING");

                return;
            }

            lock (undevlLock)
            {
                try
                {
                    using (MessageQueue er = new MessageQueue(undeliverableQueue))
                    {
                        QXLog(xm?.uuid, "Output Node Send Failure", "Sending Message to Undeliverable Queue", "PROGRESS");
                        er.Send(xm);
                        QXLog(xm?.uuid, "Output Node Send Failure", "Message Sent to Undeliverable Queue", "PROGRESS");


                        logger.Info("Message Sent to Undeliverble Queue");
                    }
                }
                catch (Exception ex)
                {
                    QXLog(xm?.uuid, "Output Node Send Failure", "Message Could Not Be Sent To Undeliverable Queue", "ERROR");

                    logger.Error("Unable to Send to Undeliverble Queue");
                    logger.Error(ex.StackTrace);
                }
            }
        }
        public void QXLog(string uuid, string topic, string message, string type)
        {

            try
            {
                QueueMonitorMessage msg = new QueueMonitorMessage(id, name, uuid, topic, message, type);

                this.monitorMessageProgress?.Report(msg);
            }
            catch (Exception) { }

        }
    }
}
