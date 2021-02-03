using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Messaging;
using System.Runtime.Caching;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Xml;
using System.Xml.Linq;

namespace QueueExchange {

    public class ContextStats {
        public string key;
        public int recieved = 0;
        public int send = 0;

        override public String ToString() {
            return $"Key: {key}, Received: {recieved}, Sent:{send}";
        }
    }
    /*
     * The class that links inputs to outputs
     */
    public class Pipeline : IDisposable {
        private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly NLog.Logger statLogger = NLog.LogManager.GetLogger("ContextStats");

        private readonly List<QueueAbstract> output = new List<QueueAbstract>();
        private readonly List<QueueAbstract> input = new List<QueueAbstract>();
        //private readonly Dictionary<string, int> msgCount = new Dictionary<string, int>();
        private readonly Dictionary<string, ContextStats> statDict = new Dictionary<string, ContextStats>();
        private readonly QueueFactory queueFactory = new QueueFactory();  // Factory class that takes queue and filter defintions and returns the instantiated object

        private readonly MemoryCache _contextCache;
        private readonly MemoryCache _firstProcessedCleared = new MemoryCache("firstprocessed");
        private readonly MemoryCache _firstProcessedCompleted = new MemoryCache("firstprocessedcompleted");

        private readonly bool discardInCache = false;
        private readonly bool firstOnly;
        private readonly string contextCacheKeyXPath;
        private readonly double contextCacheExpiry = 10.0;
        private readonly bool mostRecentOnly;
        private readonly Dictionary<string, Queue<ExchangeMessage>> _bufferMemoryQueueDict = new Dictionary<string, Queue<ExchangeMessage>>();
        private readonly Dictionary<string, System.Timers.Timer> _bufferTimerDict = new Dictionary<string, System.Timers.Timer>();
        private readonly bool useMessageAsKey;
        private readonly bool contextAware = false;

        private int throttleInterval = 0;
        private readonly int maxMsgPerMinute;   // Maximum number of input messages that the pipe will process per minute
        private readonly string maxMsgPerMinuteProfile;   // Maximum number of input messages that the pipe will process per minute

        private readonly bool randomDistribution = false;   // Distribute to a single random output
        private readonly bool roundRobinDistribution = false;  // Distribute to output based ona round robin basis
        private int nextOutput = 0;  // Keep track of the next output to send to when configured for round robin distribution
        private readonly bool outputIsolation = false;  // When there are multiple outputs they can be run in isolation so the pipe doesn't wait for the message to be processed by one input before moving on to the next

        private int msgRecieved = 0;
        private bool OK_TO_RUN = false;  // Flag to indicate whether the pipe should be running
                                         //     protected bool enableLog = false;
        private bool _disposed = false;

        public string id;
        public string name;
        private readonly string inputQueueName;
        private readonly MessageQueue pipeInputQueue;

        private readonly IProgress<PipelineMonitorMessage> monitorMessageProgress;
        private readonly Progress<QueueMonitorMessage> monitorPipelineProgress;
        private readonly System.Timers.Timer resetTimer;
        private int totalReceived = 0;
        private int totalSent = 0;

        public Pipeline(XElement pipeConfig, IProgress<PipelineMonitorMessage> monitorMessageProgress) {
            this.monitorMessageProgress = monitorMessageProgress;
            monitorPipelineProgress = new Progress<QueueMonitorMessage>();
            monitorPipelineProgress.ProgressChanged += MonitorStatusMessage;

            try {
                string dist = pipeConfig.Attribute("distribution").Value;
                if (dist == "random") {
                    randomDistribution = true;
                } else if (dist == "roundRobin") {
                    roundRobinDistribution = true;
                }
            } catch {
                randomDistribution = false;
                roundRobinDistribution = false;
            }

            try {
                this.id = pipeConfig.Attribute("id").Value;
            } catch (Exception) {
                this.id = Guid.NewGuid().ToString();
            }

            try {
                outputIsolation = bool.Parse(pipeConfig.Attribute("outputIsolation").Value);
            } catch (Exception) {
                outputIsolation = false;
            }

            try {
                contextAware = bool.Parse(pipeConfig.Attribute("contextAware").Value);
            } catch (Exception) {
                contextAware = false;
            }
            try {
                mostRecentOnly = bool.Parse(pipeConfig.Attribute("mostRecentOnly").Value);
            } catch (Exception) {
                mostRecentOnly = false;
            }

            try {
                name = pipeConfig.Attribute("name").Value;
            } catch (Exception) {
                name = "Un Named PipeLine";
            }

            try {
                inputQueueName = pipeConfig.Attribute("pipeInputQueueName").Value;
            } catch (Exception) {
                inputQueueName = null;
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

            // Configure a throttling time to limit the throughput of the pipeline
            try {
                maxMsgPerMinuteProfile = pipeConfig.Attribute("maxMsgPerMinuteProfile").Value;
            } catch (Exception) {
                maxMsgPerMinuteProfile = null;
            }
            // QueueFactory takes the definition of each of the queues and creates the defined 
            // queue of the defined type. All the filters and transformations are built into the 
            // queue itself and configured in the constructor

            // The input queue. There may be multiple queue which have to be prioritised
            IEnumerable<XElement> InEndPoints = from ep in pipeConfig.Descendants("input") select ep;
            foreach (XElement ep in InEndPoints) {
                QueueAbstract queue = queueFactory.GetQueue(ep, monitorPipelineProgress, inputQueueName);
                //queue.SetParentPipe(this);
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


            // The output queues. There may be multiple output queues for each input queue
            IEnumerable<XElement> OutEndPoints = from ep in pipeConfig.Descendants("output") select ep;

            foreach (XElement ep in OutEndPoints) {

                try {
                    QueueAbstract queue = queueFactory.GetQueue(ep, monitorPipelineProgress);

                    if (queue != null) {
                        if (!queue.isValid) {
                            logger.Warn($"Could not add Output Queue {queue.queueName} to {name}. Invalid Configuration");
                            continue;
                        }
                        output.Add(queue);
                    } else {
                        logger.Error($"Could not proccess Output Queue {queue.queueName}");
                    }
                } catch (Exception ex) {
                    logger.Error(ex.Message);
                }
            }


            // Create a queue that all the inputs will sent messages to
            try {

                if (!MessageQueue.Exists(inputQueueName)) {
                    using (MessageQueue t = MessageQueue.Create(inputQueueName)) {
                        this.pipeInputQueue = t;
                        logger.Info($"Created Input Queue {inputQueueName} for pipeline");
                    }
                } else {
                    this.pipeInputQueue = new MessageQueue(inputQueueName);
                }
            } catch (Exception ex) {
                logger.Error(ex.Message);
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
                this.firstOnly = bool.Parse(pipeConfig.Attribute("firstOnly").Value);
            } catch (Exception) {
                this.firstOnly = false;
            }

            try {
                this.useMessageAsKey = bool.Parse(pipeConfig.Attribute("useMessageAsKey").Value);
            } catch (Exception) {
                this.useMessageAsKey = false;
            }



            //Output and reset context
            if (contextCacheKeyXPath != null) {
                int contextStatsInterval;
                try {
                    contextStatsInterval = int.Parse(pipeConfig.Attribute("contextStatsInterval").Value);
                } catch (Exception) {
                    contextStatsInterval = 30000;
                }

                resetTimer = new System.Timers.Timer() {
                    AutoReset = true,
                    Interval = contextStatsInterval
                };
                logger.Info($"Context Cache Stats Interval set to {contextStatsInterval}");
                resetTimer.Elapsed += (source, eventArgs) =>
                {
                    statLogger.Info($"Total received: {this.totalReceived}, Total sent: {this.totalSent}");
                    statLogger.Info($">>>> Context Cache Stats for Previous {resetTimer.Interval}ms");

                    try {
                        foreach (var v in statDict.Values) {
                            statLogger.Info(v);
                        }
                    } catch (Exception ex) {
                        logger.Error($"Stats dictionary problem {ex.Message}");
                    } finally {
                        statDict.Clear();
                    }

                    statLogger.Info($"<<<<< End of Context Cache Stats");

                };
                resetTimer.Start();
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

        protected ExchangeMessage GetMessage() {
            return new ExchangeMessage(GetAsyncMessageFromInputQueue());
        }

        private string GetAsyncMessageFromInputQueue() {
            using (pipeInputQueue) {
                while (OK_TO_RUN) {
                    try {
                        using (Message msg = pipeInputQueue.Receive()) {
                            ActiveXMessageFormatter mft = new ActiveXMessageFormatter();
                            string mess = mft.Read(msg) as string;
                            return mess;
                        }
                    } catch (MessageQueueException e) {
                        // Handle no message arriving in the queue.
                        if (e.MessageQueueErrorCode == MessageQueueErrorCode.IOTimeout) {

                        } else {
                            logger.Info($"Queue Error: {this.pipeInputQueue.Path} {e.StackTrace}");
                        }

                    } catch (Exception ex) {
                        logger.Trace(ex.Message);
                        logger.Info(ex, "Unhandled MSMQ listen Error");
                    }
                }
            }

            return null;

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

        // Enables the changing of MaxMessagesPerMinute to change over the course of running.  
        private void PrepareProfileThreads() {
            string[] pairs = maxMsgPerMinuteProfile.Split(',');
            try {
                foreach (string pair in pairs) {
                    string[] pairString = pair.Split(':');
                    int minFromStart = int.Parse(pairString[0]);
                    int maxMess = int.Parse(pairString[1]);
                    int intervalThrottleInterval = 60000 / maxMess;
                    int waitBeforeStart = Math.Max(minFromStart * 60000, 5);

                    System.Timers.Timer resetTimer = new System.Timers.Timer {
                        AutoReset = false,
                        Interval = waitBeforeStart,
                        Enabled = true
                    };
                    resetTimer.Elapsed += (sender, e) => MyElapsedMethod(sender, e, intervalThrottleInterval, maxMess);
                    resetTimer.Start();
                }
            } catch (Exception e) {
                logger.Error("**********************************");
                logger.Error("* Message Throttle Profile Error *");
                logger.Error("**********************************");
                logger.Error(e.Message);
            }
        }

        private void MyElapsedMethod(object sender, ElapsedEventArgs e, int intervalThrottleInterval, int maxMess) {
            logger.Info($"Message Rate Profile. Setting Throttle interval = {throttleInterval}. Message Rate = {maxMess}");
            this.throttleInterval = intervalThrottleInterval;
        }

        public async Task StartPipeLine() {

            // At the pipeline level, the pipe can be configured to use or bypass the
            // filtering and transformation on each of the queues. 

            // Pipeline processing is simple, wait for something from the input queue and
            // if is not null, then distribute to the output according to the 
            // selected distribution pattern and then repeat 

            // The distribution to each of the output queues is done in a seperate async
            // Task so that they do not interfere with each other. 

            if (maxMsgPerMinuteProfile != null) {
                PrepareProfileThreads();
            }

            foreach (QueueAbstract inQ in input) {
                logger.Info($"Starting Async Listener for {inQ.name}");
                var t = Task.Run(() => inQ.StartListener());
            }

            logger.Info($"Pipe {name} running. Input Queues = {input.Count()}, Output Queues = {output.Count()}");
            logger.Info($"Throttle interval = {throttleInterval}");

            while (OK_TO_RUN) {
                try {

                    //Get the message from the input
                    ExchangeMessage xm = GetMessage();
                    QXLog("Recieved Message", null, "PROGRESS");

                    if (throttleInterval > 0 || contextAware == true) {
                        QXLog("Sending Message to Context Proceesor", null, "PROGRESS");
                        await ContextProcessorAsync(xm);
                    } else {
                        QXLog("Sending Message to Injector", null, "PROGRESS");
                        await InjectMessage(xm);
                        Thread.Sleep(throttleInterval);
                    }
                } catch (Exception ex) {
                    Console.WriteLine(ex.Message);
                }
            }
            logger.Info($"Stopping Pipeline {name}");
        }

        public async Task ContextProcessorAsync(ExchangeMessage xm) {

            if (!xm.pass) {
                return;
            }

            // If no contextKey has been provided, just inject the message straight away
            if (contextCacheKeyXPath == null && !useMessageAsKey) {
                await InjectMessage(xm);
                return;
            }

            string contextKeyValue = GetContextKey(xm);

            if (contextKeyValue == null) {
                await InjectMessage(xm);
                return;
            }

            try {

                Queue<ExchangeMessage> bufferMemoryQueue = null;
                System.Timers.Timer bufferPopperTimer = null;


                if (_bufferMemoryQueueDict.ContainsKey(contextKeyValue) && _bufferTimerDict.ContainsKey(contextKeyValue)) {
                    bufferMemoryQueue = _bufferMemoryQueueDict[contextKeyValue];
                    bufferPopperTimer = _bufferTimerDict[contextKeyValue];
                } else {

                    // They don't exist, so they need to be created.
                    logger.Trace($"Creating queue and timer for {contextKeyValue}");

                    //Create the queue
                    bufferMemoryQueue = new Queue<ExchangeMessage>();
                    _bufferMemoryQueueDict.Add(contextKeyValue, bufferMemoryQueue);

                    //Create the timer
                    bufferPopperTimer = CreatePopperTask(xm, bufferMemoryQueue, contextKeyValue);
                    _bufferTimerDict.Add(contextKeyValue, bufferPopperTimer);
                }

                ContextStats stats = null;
                if (statDict.ContainsKey(contextKeyValue)) {
                    stats = statDict[contextKeyValue];
                } else {
                    ContextStats v = new ContextStats {
                        key = contextKeyValue
                    };
                    statDict.Add(contextKeyValue, v);
                    stats = v;
                }

                stats.recieved++;

                this.totalReceived++;

                if (_contextCache.Contains(contextKeyValue) && this.discardInCache) {
                    logger.Info($"Message found in Cache - Discarding. Message Hash {contextKeyValue}");
                    return;
                }

                if (mostRecentOnly) {
                    bufferMemoryQueue.Clear();
                }

                bufferMemoryQueue.Enqueue(xm);

                // So the message is enqueued, now just work out how long how to set the popper

                //Let's deal with it if it is not a firstOnly

                if (!firstOnly) {

                    if (!_contextCache.Contains(contextKeyValue)) {
                        //It's not in the cachce

                        if (!bufferPopperTimer.Enabled) {
                            //If the popper isn't already enabled, enable it to pop straight away (5ms) 
                            bufferPopperTimer.Interval = 5.0;
                            bufferPopperTimer.Enabled = true;
                            bufferPopperTimer.Start();
                        }

                    } else {

                        //It's in tha cache, 
                        if (!bufferPopperTimer.Enabled) {
                            //If the popper isn't already enabled, then set the popper. (If it is in the cache, then really, the popper should already be set) 
                            bufferPopperTimer.Interval = contextCacheExpiry * 1000;
                            bufferPopperTimer.Enabled = true;
                            bufferPopperTimer.Start();
                        }
                    }
                    return;
                }

                if (firstOnly) {
                    //It's not in the cache

                    bool inCache = _contextCache.Contains(contextKeyValue);
                    bool firstDone = _firstProcessedCompleted.Contains(contextKeyValue);
                    if (!inCache || firstDone) {
                        _contextCache.AddOrGetExisting(contextKeyValue, DateTime.Now.AddSeconds(this.contextCacheExpiry), DateTime.Now.AddSeconds(this.contextCacheExpiry));
                        if (!bufferPopperTimer.Enabled) {
                            bufferPopperTimer.Interval = 10.0;
                            bufferPopperTimer.Enabled = true;
                            bufferPopperTimer.Start();
                        }
                    } else {
                        if (!bufferPopperTimer.Enabled) {
                            bufferPopperTimer.Interval = contextCacheExpiry * 1000;
                            bufferPopperTimer.Enabled = true;
                            bufferPopperTimer.Start();
                        }
                    }
                    return;
                }

            } catch (Exception ex) {
                logger.Error(ex);
            }
        }
        private string GetContextKey(ExchangeMessage xm) {

            string contextKeyValue = null;

            if (useMessageAsKey) {
                QXLog("Context Proceesor", "Entire Message Being Used As Key", "PROGRESS");
                using (SHA256 mySHA256 = SHA256.Create()) {

                    byte[] byteArray = Encoding.UTF8.GetBytes(xm.payload);


                    using (MemoryStream stream = new MemoryStream(byteArray)) {
                        byte[] hashValue = mySHA256.ComputeHash(stream);
                        contextKeyValue = BitConverter.ToString(hashValue);
                    }
                    return contextKeyValue;
                }
            } else if (contextCacheKeyXPath == "*") {
                return "ALL_MESSAGES";
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
                    contextKeyValue = node.InnerText;
                    logger.Info($"Context Key for Message = {contextKeyValue}");
                } catch {
                    return null;
                }
                QXLog("Context Proceesor", $"Context Key Retrieves Context Key = {contextKeyValue}", "PROGRESS");

                return contextKeyValue;
            }
        }
        private System.Timers.Timer CreatePopperTask(ExchangeMessage xm, Queue<ExchangeMessage> bufferMemoryQueue, string contextKeyValue) {

            double interval = contextCacheExpiry * 1000 + 100.0;

            logger.Info($"Creating Popper Task. Message Hash: {contextKeyValue}");

            System.Timers.Timer bufferPopperTimer = new System.Timers.Timer {
                Interval = interval,
                Enabled = false
            };
            bufferPopperTimer.Elapsed += async (source, eventArgs) =>
            {
                bufferPopperTimer.AutoReset = false;
                bufferPopperTimer.Enabled = false;

                if (_firstProcessedCompleted.Contains(contextKeyValue)) {
                    //  logger.Info("Pooper Expired - Marking it cleared <<<<<<<<<<<<<<<<<<");
                    _firstProcessedCleared.AddOrGetExisting(contextKeyValue, DateTime.Now.AddSeconds(this.contextCacheExpiry), DateTime.Now.AddSeconds(this.contextCacheExpiry));
                }

                if (bufferMemoryQueue.Count != 0) {
                    logger.Trace($"Injection Popper Timer Went Off for key = {contextKeyValue} - {bufferMemoryQueue.Count} Messages on queue");

                    _firstProcessedCompleted.AddOrGetExisting(contextKeyValue, contextKeyValue, DateTime.Now.AddHours(18));
                    _contextCache.AddOrGetExisting(contextKeyValue, DateTime.Now.AddSeconds(this.contextCacheExpiry), DateTime.Now.AddSeconds(this.contextCacheExpiry));

                    if (bufferMemoryQueue.Count > 0) {
                        await InjectMessage(bufferMemoryQueue.Dequeue());
                    } else {
                        return;
                    }
                    if (firstOnly && _firstProcessedCleared.Contains(contextKeyValue)) {
                        bufferPopperTimer.Interval = 10.0;
                    } else {
                        bufferPopperTimer.Interval = contextCacheExpiry * 1000;
                    }
                    bufferPopperTimer.AutoReset = true;
                    bufferPopperTimer.Enabled = true;

                    try {
                        statDict[contextKeyValue].send++;
                        statLogger.Trace(statDict[contextKeyValue]);
                    } catch (Exception ex) {
                        logger.Error($"Stats dictionary problem {ex.Message}");
                    }
                    this.totalSent++;
                } else {
                    logger.Trace($"Reinjection Popper Timer Went Off for key = {contextKeyValue} - No Message to Process");
                    if (_firstProcessedCleared.Contains(contextKeyValue)) {
                        bufferPopperTimer.Enabled = false;
                        bufferPopperTimer.AutoReset = false;
                    } else {
                        bufferPopperTimer.Interval = contextCacheExpiry * 1000;
                        bufferPopperTimer.Enabled = true;
                        bufferPopperTimer.AutoReset = true;
                    }
                }
            };

            return bufferPopperTimer;
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
                QXLog("Starting Output Meesage Processing", null, "PROGRESS");

                if (randomDistribution) {
                    // Distributes the message to a random output

                    Random rnd = new Random();
                    int index = rnd.Next(0, output.Count);

                    logger.Trace($"Random Distributor. Sending message {xm.uuid } to {output[index].name}");
                    QXLog("Output Meesage Processing", "Distributing to Random Output", "PROGRESS");

                    if (outputIsolation) {
                        QXLog("Async Message Start", "Distribution to Random Output", "PROGRESS");
                        _ = Task.Run(() => SendAndLog(output[index], xm));
                        QXLog("Async Message End", "Distribution to Random Output", "PROGRESS");
                    } else {
                        QXLog("Sync Message Start", "Distribution to Random Output", "PROGRESS");
                        _ = await SendAndLog(output[index], xm);
                        QXLog("Sync Message End", "Distribution to Random Output", "PROGRESS");
                    }

                } else if (roundRobinDistribution) {
                    // Distributes the message only once to each queue in turn
                    int index = nextOutput % output.Count;

                    logger.Trace($"Round Robin Distributor. Sending message {xm.uuid } to {output[index].name}");
                    QXLog("Output Message Processing", "Distribution to Round Robin Output", "PROGRESS");

                    if (outputIsolation) {
                        QXLog("Async Message Start", "Distribution to Round Robin Output", "PROGRESS");
                        _ = Task.Run(() => SendAndLog(output[index], xm));
                        QXLog("Async Message End", "Distribution to Round Robin Output", "PROGRESS");
                    } else {
                        QXLog("Sync Message Start", "Distribution to Round Robin Output", "PROGRESS");
                        _ = await SendAndLog(output[index], xm);
                        QXLog("Sync Message End", "Distribution to Round Robin Output", "PROGRESS");
                    }

                    nextOutput++;

                } else {
                    // Distribute the message to all the outputs
                    if (outputIsolation) {
                        // Wont wait for the Send to complete before completing
                        foreach (QueueAbstract q in output) {
                            QXLog("Async Message Start", "Normal Distribution", "PROGRESS");
                            _ = Task.Run(() => SendAndLog(q, xm));
                            QXLog("Async Message End", "Normal Distribution", "PROGRESS");
                        }
                    } else {

                        // Will wait for all Sends to complete before proceeding
                        QXLog("Sync Message Start", "Normal Distribution", "PROGRESS");

                        foreach (QueueAbstract q in output) {
                            logger.Info($"-->{name} --> sending message {xm?.uuid } to {q.name}");
                            _ = await SendAndLog(q, xm);

                            //if (xm.sent)
                            //{
                            //    if (q.queueName != null)
                            //    {
                            //        msgCount[q.queueName] = msgCount[q.queueName] + 1;
                            //        logger.Trace($"{q.queueName} = {msgCount[q.queueName]}");
                            //    }
                            //    else
                            //    {
                            //        msgCount[q.name] = msgCount[q.name] + 1;
                            //        logger.Trace($"{q.name} = {msgCount[q.name]}");
                            //    }
                            //}

                        }
                        QXLog("Sync Message End", "Normal Distribution", "PROGRESS");
                    }
                }
            }

        }
        private async Task<ExchangeMessage> SendAndLog(QueueAbstract queue, ExchangeMessage message) {
            message = await queue.Send(message);
            return message;
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
        private void MonitorStatusMessage(object sender, QueueMonitorMessage e) {
            PipelineMonitorMessage msg = new PipelineMonitorMessage(e) {
                pipeID = this.id,
                pipeName = this.name,
                pipemessageType = "QUEUEMESSAGE"
            };
            monitorMessageProgress?.Report(msg);
        }
        public void QXLog(string topic, string message, string messageType) {
            PipelineMonitorMessage msg = new PipelineMonitorMessage(id, name, topic, message, messageType);
            monitorMessageProgress?.Report(msg);
        }

    }
}
