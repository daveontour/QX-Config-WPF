using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace QueueExchange {
    /*
     * The class that links inputs to outputs
     */
    public class Pipeline: IDisposable {

        protected readonly List<QueueAbstract> output = new List<QueueAbstract>();
        protected readonly List<QueueAbstract> input = new List<QueueAbstract>();
        protected readonly Dictionary<string, int> msgCount = new Dictionary<string, int>();
        protected readonly QueueFactory queueFactory = new QueueFactory();  // Factory class that takes queue and filter defintions and returns the instantiated object
        protected int throttleInterval = 0;
        protected int priorityWait = 200; // If there is more than on input queue, this is the time in ms that polling of each input will wait
        protected bool preWait = true;
        protected int msgRecieved = 0;
        protected int maxMsgPerMinute;   // Maximum number of input messages that the pipe will process per minute
        protected bool randomDistribution = false;   // Distribute to a single random output
        protected bool roundRobinDistribution = false;  // Distribute to output based ona round robin basis
        protected int nextOutput = 0;  // Keep track of the next output to send to when configured for round robin distribution
        protected bool OK_TO_RUN = false;  // Flag to indicate whether the pipe should be running
        protected bool outputIsolation = false;  // When there are multiple outputs they can be run in isolation so the pipe doesn't wait for the message to be processed by one input before moving on to the next
                                                 //     protected bool enableLog = false;
        protected readonly string contextCacheKeyXPath;
        protected readonly double contextCacheExpiry = 10.0;
        protected readonly MemoryCache _contextCache;
        protected readonly bool discardInCache = false;
        protected readonly Dictionary<String, Queue<ExchangeMessage>> _bufferMemoryQueueDict = new Dictionary<String, Queue<ExchangeMessage>>();
        protected readonly Dictionary<String, System.Timers.Timer> _bufferTimerDict = new Dictionary<String, System.Timers.Timer>();
        protected readonly bool useMessageAsKey;
        protected bool _disposed = false;

        protected string name;
        protected static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public Pipeline(XElement pipeConfig) {

            try {
                string dist = pipeConfig.Attribute("distribution").Value;
                if (dist == "random") {
                    randomDistribution = true;
                } else if (dist == "roundRobin") {
                    roundRobinDistribution = true;
                }

            } catch {

            }

            try {
                outputIsolation = bool.Parse(pipeConfig.Attribute("outputIsolation").Value);
            } catch (Exception) {
                outputIsolation = false;
            }

            try {
                priorityWait = int.Parse(pipeConfig.Attribute("priorityWait").Value);
            } catch (Exception) {
                priorityWait = 200;
            }

            try {
                preWait = bool.Parse(pipeConfig.Attribute("preWait").Value);
            } catch (Exception) {
                preWait = true;
            }
            try {
                name = pipeConfig.Attribute("name").Value;
            } catch (Exception) {
                name = "Un Named PipeLine";
            }

            // Configure a throttling time to limit the throughput of the pipeline

            try {
                maxMsgPerMinute = int.Parse(pipeConfig.Attribute("maxMsgPerMinute").Value);
                if (maxMsgPerMinute == -1) {
                    throttleInterval = 0;
                } else {
                    throttleInterval = 60000 / maxMsgPerMinute;
                }
            } catch (Exception) {
                throttleInterval = 0;
            }
            // QueueFactory takes the definition of each of the queues and creates the defined 
            // queue of the defined type. All the filters and transformations are built into the 
            // queue itself and configured in the constructor

            // The input queue. There may be multiple queue which have to be prioritised
            IEnumerable<XElement> InEndPoints = from ep in pipeConfig.Descendants("input") select ep;
            foreach (XElement ep in InEndPoints) {
                QueueAbstract queue = queueFactory.GetQueue(ep);
                if (queue != null) {
                    if (!queue.isValid) {
                        logger.Warn($"Could not add Input Queue {queue.queueName} to {name}. Invalid Configuration");
                        continue;
                    }
                    input.Add(queue);
                } else {
                    logger.Error($"Could not proccess Input Queue {queue.queueName}");
                }
            }
            // Sort the input into priority order
            input.Sort((x, y) => x.priority.CompareTo(y.priority));


            // The output queues. There may be multiple output queues for each input queue
            IEnumerable<XElement> OutEndPoints = from ep in pipeConfig.Descendants("output") select ep;
            foreach (XElement ep in OutEndPoints) {
                QueueAbstract queue = queueFactory.GetQueue(ep);
                if (queue.queueName != null) {
                    msgCount.Add(queue.queueName, 0);
                } else {
                    msgCount.Add(queue.name, 0);
                }
                if (queue != null) {
                    if (!queue.isValid) {
                        logger.Warn($"Could not add Output Queue {queue.queueName} to {name}. Invalid Configuration");
                        continue;
                    }
                    output.Add(queue);
                } else {
                    logger.Error($"Could not proccess Output Queue {queue.queueName}");
                }
            }

            _contextCache = new MemoryCache(name);

            try {
                contextCacheKeyXPath = pipeConfig.Attribute("contextCacheKeyXPath").Value;
            } catch (Exception) {
                contextCacheKeyXPath = null;
            }

            try {
                this.contextCacheExpiry = double.Parse(pipeConfig.Attribute("contextCacheExpiry").Value);
            } catch (Exception) {
                this.contextCacheExpiry = 10.0;
            }

            try {
                this.discardInCache = bool.Parse(pipeConfig.Attribute("discardInCache").Value);
            } catch (Exception) {
                this.discardInCache = false;
            }

            try {
                this.useMessageAsKey = bool.Parse(pipeConfig.Attribute("useMessageAsKey").Value);
            } catch (Exception) {
                this.useMessageAsKey = false;
            }

            /* Only allow if there are output queues configured
             * It is OK if there are no inputs defined. An output only queue may be
             * defined just to specifiy a maxMessages parameter on the queue for size maintenance
             */

            if (output.Count() > 0) {
                OK_TO_RUN = true;
            } else {
                OK_TO_RUN = false;
                logger.Warn($"No inputs or outputs defined for Pipeline {name}");
            }
        }
        protected async Task<ExchangeMessage> GetMessage() {

            // Goes through the input queues and return the message.
            // After each message is returned, it always checks the queues 
            // in priority order, so the highest priority will always be cleared 
            // before messages are taken from the next highest queue

            bool immediateReturn = false;
            if (input.Count > 1) {
                // Flag to signal Input Listeners not to enter a permanaent wait cycle 
                // for the message, but to return null on the first time out. 
                // This is required for the prioritisation of input queues
                immediateReturn = true;
            }


            while (true) {
                foreach (QueueAbstract inQ in input) {
                    ExchangeMessage xm = await Task.Run(() => inQ.ListenToQueue(immediateReturn, priorityWait));
                    if (xm != null) {
                        if (xm.payload != null) {
                            QXMonitor.Log(new ExchangeMonitorMessage(xm.uuid, inQ.name, this.name, "MESSAGE RECEIVED", null));
                            return xm;
                        } else {
                            if (immediateReturn) {
                                // Return null so the pipeline can check other higher priority inputs 
                                return null;
                            } else {
                                QXMonitor.Log(new ExchangeMonitorMessage(xm.uuid, inQ.name, this.name, "NULL RECEIVED", "Message was NULL after filtering and transformation"));
                            }

                            // If we get here, then the input isn't part of a prioity set, so 
                            // Don't return anything so the loop continues to listen for messages to process
                        }
                    }
                }
            }
        }

        public void StopPipeLine() {
            foreach (QueueAbstract q in input) {
                try {
                    q.Stop();
                } catch (Exception) {
                    //
                }
            }
            foreach (QueueAbstract q in output) {
                try {
                    q.Stop();
                } catch (Exception) {
                    //
                }
            }
            OK_TO_RUN = false;
        }

        public async Task StartPipeLine() {

            // At the pipeline level, the pipe can be configured to use or bypass the
            // filtering and transformation on each of the queues. 

            // Pipeline processing is simple, wait for something from the input queue and
            // if is not null, then distribute to the output according to the 
            // selected distribution pattern and then repeat 

            // The distribution to each of the output queues is done in a seperate async
            // Task so that they do not interfere with each other. 

            logger.Info($"Pipe {name} running. Input Queues = {input.Count()}, Output Queues = {output.Count()}");
            logger.Info($"Throttle interval = {throttleInterval}");

            while (OK_TO_RUN) {

                //Get the message from the input
                ExchangeMessage xm = await GetMessage();

                if (throttleInterval <= 0) {
                    await ContextProcessorAsync(xm);
                } else {
                    await InjectMessage(xm);
                    Thread.Sleep(throttleInterval);
                }
            }
            logger.Info($"Stopping Pipeline {name}");
        }

        public async Task ContextProcessorAsync(ExchangeMessage xm) {

            // If no contextKey has been provided, just inject the message straight away
            if (contextCacheKeyXPath == null && !useMessageAsKey) {
                await InjectMessage(xm);
                return;
            }

            try {

                string nodeValue = null;

                if (useMessageAsKey) {
                    using (SHA256 mySHA256 = SHA256.Create()) {
                        using (var stream = GenerateStreamFromString(xm.payload)) {
                            byte[] hashValue = mySHA256.ComputeHash(stream);
                            nodeValue = BitConverter.ToString(hashValue);
                            logger.Trace($"\n\nMessage Hash {nodeValue}\n\n");
                        }
                    }
                } else {

                    // Look at the XML to get the contextKey
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(xm.payload);
                    XmlNamespaceManager ns = new XmlNamespaceManager(doc.NameTable);

                    foreach (KeyValuePair<string, string> item in Exchange.nsDict) {
                        ns.AddNamespace(item.Key, item.Value);
                    }


                    try {
                        XmlNode node = doc.SelectSingleNode(contextCacheKeyXPath, ns);
                        nodeValue = node.InnerText;

                        logger.Trace($"\n\n KEY = {nodeValue}\n\n");
                    } catch {
                        _ = InjectMessage(xm);
                        return;
                    }
                }

                Queue<ExchangeMessage> _bufferMemoryQueue = null;
                System.Timers.Timer _bufferPopperTimer = null;

                try {
                    _bufferMemoryQueue = _bufferMemoryQueueDict[nodeValue];
                    _bufferPopperTimer = _bufferTimerDict[nodeValue];
                } catch (Exception) {

                    logger.Trace($"Creating queue and timer for {nodeValue}");

                    _bufferMemoryQueue = new Queue<ExchangeMessage>();
                    _bufferMemoryQueueDict.Add(nodeValue, _bufferMemoryQueue);

                    _bufferPopperTimer = new System.Timers.Timer {
                        AutoReset = false,
                        Interval = contextCacheExpiry * 1000 + 100.0,
                        Enabled = false
                    };
                    _bufferPopperTimer.Elapsed += async (source, eventArgs) =>
                    {
                        logger.Trace("Popper Timer Went Off");
                        if (_bufferMemoryQueue.Count != 0) {
                            logger.Trace("Popper Timer Went Off - Message to Process");
                            await ContextProcessorAsync(_bufferMemoryQueue.Dequeue());
                        } else {
                            logger.Trace("Popper Timer Went Off - No Message to Process");
                            _bufferPopperTimer.Enabled = false;
                        }
                    };

                    _bufferTimerDict.Add(nodeValue, _bufferPopperTimer);
                }

                // See if the key is currently in the context cache
                if (!_contextCache.Contains(nodeValue)) {
                    //It's not in the context cache, so add it and immediately send the inject the message to the output nodes
                    logger.Info($"No existing key in cache, so injecting it and adding to cache");

                    _contextCache.AddOrGetExisting(nodeValue, DateTime.Now.AddSeconds(this.contextCacheExpiry), DateTime.Now.AddSeconds(this.contextCacheExpiry));
                    await InjectMessage(xm);


                    //Start the task to pop the next message if it appears in the meantime
                    _bufferPopperTimer.Enabled = true;
                    _bufferPopperTimer.Start();


                } else {
                    logger.Trace("The key was found in the Context Cache");

                    if (this.discardInCache) {
                        return;
                    }
                    // Schedule the injection of the message back to the output
                    DateTimeOffset t = DateTimeOffset.Parse(_contextCache.Get(nodeValue).ToString());
                    TimeSpan ts = t - DateTime.Now;
                    double interval = ts.TotalMilliseconds + 5.0;

                    logger.Trace($"Found in cache {t.ToString()},  Now =  {DateTime.Now.ToString()}, Interval =  {interval}  {xm.uuid}\n\n");
                    //Create a Timer Task to inject the message after the buffer time since the last message was sent expired

                    if (interval < 0 && _bufferMemoryQueue.Count == 0) {

                        logger.Trace("The interval is less than zero => immediate injection");

                        _bufferPopperTimer.Enabled = true;
                        _bufferPopperTimer.Stop();
                        _bufferPopperTimer.Start();

                        _contextCache.Remove(nodeValue);
                        _contextCache.AddOrGetExisting(nodeValue, DateTime.Now.AddSeconds(this.contextCacheExpiry), DateTime.Now.AddSeconds(this.contextCacheExpiry));
                        _ = InjectMessage(xm);

                        return;
                    } else {

                        _bufferMemoryQueue.Enqueue(xm);

                        if (!_bufferPopperTimer.Enabled) {
                            _bufferPopperTimer.Enabled = true;
                            _bufferPopperTimer.Start();
                        }

                    }
                }

                return;

            } catch (Exception ex) {
                logger.Error(ex);
            }
        }
        protected async Task InjectMessage(ExchangeMessage xm) {

            if (xm != null) {

                if (!xm.pass) {
                    logger.Trace($"Blocked by incoming filter. Message {xm.uuid }");
                    return;
                }

                msgRecieved++;
                logger.Trace($"Processing message {xm.uuid }  [{msgRecieved}]");
                xm.status = "Message Recieved";

                // Pipeline specific logging
                QXMonitor.Log(new ExchangeMonitorMessage(xm.uuid, null, this.name, "PIPE MESSAGE OUTPUT PROCESSING START", null));

                if (randomDistribution) {
                    // Distributes the message to a random output

                    Random rnd = new Random();
                    int index = rnd.Next(0, output.Count);

                    logger.Trace($"Random Distributor. Sending message {xm.uuid } to {output[index].name}");

                    QXMonitor.Log(new ExchangeMonitorMessage(xm.uuid, output[index].name, this.name, "PIPE MESSAGE OUTPUT INJECTION", "Distribution to Random output"));

                    if (outputIsolation) {
                        QXMonitor.Log(new ExchangeMonitorMessage(xm.uuid, output[index].name, this.name, "ASYNC OUTPUT NODE START", "Distribution to Random output"));
                        _ = Task.Run(() => SendAndLog(output[index], xm));
                        QXMonitor.Log(new ExchangeMonitorMessage(xm.uuid, output[index].name, this.name, "ASYNC OUTPUT NODE END", "Distribution to Random output"));
                    } else {
                        QXMonitor.Log(new ExchangeMonitorMessage(xm.uuid, output[index].name, this.name, "SYNCHRONOUS OUTPUT NODE START", "Distribution to Random output"));
                        _ = await SendAndLog(output[index], xm);
                        QXMonitor.Log(new ExchangeMonitorMessage(xm.uuid, output[index].name, this.name, "SYNCHRONOUS OUTPUT NODE END", "Distribution to Random output"));
                    }

                } else if (roundRobinDistribution) {
                    // Distributes the message only once to each queue in turn

                    int index = nextOutput % output.Count;

                    logger.Trace($"Round Robin Distributor. Sending message {xm.uuid } to {output[index].name}");

                    QXMonitor.Log(new ExchangeMonitorMessage(xm.uuid, output[index].name, this.name, "PIPE MESSAGE OUTPUT INJECTION", "Distribution to Round  Robin Output"));

                    if (outputIsolation) {
                        QXMonitor.Log(new ExchangeMonitorMessage(xm.uuid, output[index].name, this.name, "ASYNC OUTPUT NODE START", "Distribution to Round  Robin Output"));
                        _ = Task.Run(() => SendAndLog(output[index], xm));
                        QXMonitor.Log(new ExchangeMonitorMessage(xm.uuid, output[index].name, this.name, "ASYNC OUTPUT NODE END", "Distribution to Round  Robin Output"));
                    } else {
                        QXMonitor.Log(new ExchangeMonitorMessage(xm.uuid, output[index].name, this.name, "SYNCHRONOUS OUTPUT NODE START", "Distribution toRound  Robin Output"));
                        _ = await SendAndLog(output[index], xm);
                        QXMonitor.Log(new ExchangeMonitorMessage(xm.uuid, output[index].name, this.name, "SYNCHRONOUS OUTPUT NODE END", "Distribution to Round  Robin Output"));
                    }

                    nextOutput++;

                } else {
                    // Distribute the message to all the outputs
                    if (outputIsolation) {

                        // Wont wait for the Send to complete before completing
                        foreach (QueueAbstract q in output) {
                            QXMonitor.Log(new ExchangeMonitorMessage(xm.uuid, q.name, this.name, "ASYNCHRONOUS OUTPUT NODE START", "Normal Distribution"));
                            _ = Task.Run(() => SendAndLog(q, xm));
                            QXMonitor.Log(new ExchangeMonitorMessage(xm.uuid, q.name, this.name, "ASYNCHRONOUS OUTPUT NODE END", "Normal Distribution"));
                        }
                    } else {

                        // Will wait for all Sends to complete before proceeding

                        QXMonitor.Log(new ExchangeMonitorMessage(xm.uuid, null, this.name, "SYNCHRONOUS OUTPUT NODE START", "Normal Distribution"));

                        foreach (QueueAbstract q in output) {
                            logger.Info($"  Sending message {xm.uuid } to {q.name}");
                             _ = await SendAndLog(q, xm);

                            if (xm.sent) {
                                if (q.queueName != null) {
                                    msgCount[q.queueName] = msgCount[q.queueName] + 1;
                                    logger.Trace($"{q.queueName} = {msgCount[q.queueName]}");
                                } else {
                                    msgCount[q.name] = msgCount[q.name] + 1;
                                    logger.Trace($"{q.name} = {msgCount[q.name]}");
                                }
                            }
                           
                        }
 
                        QXMonitor.Log(new ExchangeMonitorMessage(xm.uuid, null, this.name, "SYNCHRONOUS OUTPUT NODE END", "Normal Distribution"));
                    }
                }
            }

        }
        private async Task<ExchangeMessage> SendAndLog(QueueAbstract queue, ExchangeMessage message) {
            message = await queue.Send(message);
            return message;
        }


        public static Stream GenerateStreamFromString(string s) {
            var stream = new MemoryStream();

            using (var writer = new StreamWriter(stream)) {
                writer.Write(s);
                writer.Flush();
            }
            stream.Position = 0;
            return stream;
        }

        public void Dispose() {
            try {
                Dispose(true);
                GC.SuppressFinalize(this);
            } catch { }
        }

        protected virtual void Dispose(bool disposing) {
            if (!_disposed) {
                if (disposing) {
                    // Clear all property values that maybe have been set
                    // when the class was instantiated
                    _contextCache.Dispose();
                }

                // Indicate that the instance has been disposed.
                _disposed = true;
            }
        }

    }
}
