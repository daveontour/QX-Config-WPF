﻿using System;
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

namespace QueueExchange
{

    public class ContextStats
    {
        public string key;
        public int recieved = 0;
        public int send = 0;

        override
        public String ToString()
        {
            return $"Key: {key}, Received: {recieved}, Sent:{send}";
        }
    }
    /*
     * The class that links inputs to outputs
     */
    public class Pipeline : IDisposable
    {

        protected readonly List<QueueAbstract> output = new List<QueueAbstract>();
        protected readonly List<QueueAbstract> input = new List<QueueAbstract>();
        protected readonly Dictionary<string, int> msgCount = new Dictionary<string, int>();
        protected readonly Dictionary<string, ContextStats> statDict = new Dictionary<string, ContextStats>();
        protected readonly QueueFactory queueFactory = new QueueFactory();  // Factory class that takes queue and filter defintions and returns the instantiated object
        protected int throttleInterval = 0;
        protected int priorityWait = 200; // If there is more than on input queue, this is the time in ms that polling of each input will wait
        protected bool preWait = true;
        protected int contextStatsInterval;
        protected int msgRecieved = 0;
        protected int maxMsgPerMinute;   // Maximum number of input messages that the pipe will process per minute
        protected string maxMsgPerMinuteProfile;   // Maximum number of input messages that the pipe will process per minute
        protected bool randomDistribution = false;   // Distribute to a single random output
        protected bool roundRobinDistribution = false;  // Distribute to output based ona round robin basis
        protected int nextOutput = 0;  // Keep track of the next output to send to when configured for round robin distribution
        protected bool OK_TO_RUN = false;  // Flag to indicate whether the pipe should be running
        protected bool outputIsolation = false;  // When there are multiple outputs they can be run in isolation so the pipe doesn't wait for the message to be processed by one input before moving on to the next
                                                 //     protected bool enableLog = false;
        protected readonly string contextCacheKeyXPath;
        protected readonly double contextCacheExpiry = 10.0;
        protected readonly MemoryCache _contextCache;

        protected readonly MemoryCache _OKToPass = new MemoryCache("OKToPass");
        protected readonly MemoryCache _firstProcessedCleared = new MemoryCache("firstprocessed");
        protected readonly MemoryCache _firstProcessedCompleted = new MemoryCache("firstprocessedcompleted");
        protected readonly bool discardInCache = false;
        protected readonly Dictionary<String, Queue<ExchangeMessage>> _bufferMemoryQueueDict = new Dictionary<String, Queue<ExchangeMessage>>();
        protected readonly Dictionary<String, System.Timers.Timer> _bufferTimerDict = new Dictionary<String, System.Timers.Timer>();
        protected readonly bool useMessageAsKey;
        protected bool _disposed = false;
        protected bool allAsyncInputs = true;
        //public QXMonitor qMon = new QXMonitor();

        public string id;

        public string name;
        protected static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        protected static NLog.Logger statLogger = NLog.LogManager.GetLogger("ContextStats");
        protected bool firstOnly;
        private IProgress<PipelineMonitorMessage> monitorMessageProgress;
        private Progress<QueueMonitorMessage> monitorPipelineProgress;
        private bool mostRecentOnly;
        private System.Timers.Timer resetTimer;
        private int totalReceived = 0;
        private int totalSent = 0;
        private string inputQueueName;
        private MessageQueue pipeInputQueue;

        public Pipeline(XElement pipeConfig, IProgress<PipelineMonitorMessage> monitorMessageProgress)
        {
            this.monitorMessageProgress = monitorMessageProgress;

            monitorPipelineProgress = new Progress<QueueMonitorMessage>();
            monitorPipelineProgress.ProgressChanged += MonitorStatusMessage;
            try
            {
                string dist = pipeConfig.Attribute("distribution").Value;
                if (dist == "random")
                {
                    randomDistribution = true;
                }
                else if (dist == "roundRobin")
                {
                    roundRobinDistribution = true;
                }

            }
            catch
            {

            }

            try
            {
                this.id = pipeConfig.Attribute("id").Value;
            }
            catch (Exception)
            {
                this.id = Guid.NewGuid().ToString();
            }

            try
            {
                outputIsolation = bool.Parse(pipeConfig.Attribute("outputIsolation").Value);
            }
            catch (Exception)
            {
                outputIsolation = false;
            }

            try
            {
                priorityWait = int.Parse(pipeConfig.Attribute("priorityWait").Value);
            }
            catch (Exception)
            {
                priorityWait = 1;
            }

            try
            {
                this.contextStatsInterval = int.Parse(pipeConfig.Attribute("contextStatsInterval").Value);
            }
            catch (Exception)
            {
                contextStatsInterval = 30000;
            }


            try
            {
                preWait = bool.Parse(pipeConfig.Attribute("preWait").Value);
            }
            catch (Exception)
            {
                preWait = true;
            }


            try
            {
                mostRecentOnly = bool.Parse(pipeConfig.Attribute("mostRecentOnly").Value);
            }
            catch (Exception)
            {
                mostRecentOnly = false;
            }

            try
            {
                name = pipeConfig.Attribute("name").Value;
            }
            catch (Exception)
            {
                name = "Un Named PipeLine";
            }

            try
            {
                inputQueueName = pipeConfig.Attribute("pipeInputQueueName").Value;
            }
            catch (Exception)
            {
                inputQueueName = null;
            }


            // Configure a throttling time to limit the throughput of the pipeline
            try
            {
                maxMsgPerMinute = int.Parse(pipeConfig.Attribute("maxMsgPerMinute").Value);
                if (maxMsgPerMinute == -1)
                {
                    throttleInterval = 0;
                }
                else
                {
                    throttleInterval = 60000 / maxMsgPerMinute;
                }
            }
            catch (Exception)
            {
                throttleInterval = 0;
            }

            // Configure a throttling time to limit the throughput of the pipeline
            try
            {
                maxMsgPerMinuteProfile = pipeConfig.Attribute("maxMsgPerMinuteProfile").Value;
            }
            catch (Exception)
            {
                maxMsgPerMinuteProfile = null;
            }
            // QueueFactory takes the definition of each of the queues and creates the defined 
            // queue of the defined type. All the filters and transformations are built into the 
            // queue itself and configured in the constructor

            // The input queue. There may be multiple queue which have to be prioritised
            IEnumerable<XElement> InEndPoints = from ep in pipeConfig.Descendants("input") select ep;
            foreach (XElement ep in InEndPoints)
            {
                QueueAbstract queue = queueFactory.GetQueue(ep, monitorPipelineProgress);
                queue.SetParentPipe(this);
                if (queue != null)
                {
                    if (!queue.isValid)
                    {
                        logger.Warn($"Could not add Input Queue {queue.queueName} to {name}. Invalid Configuration");
                        continue;
                    }
                    input.Add(queue);
                }
                else
                {
                    logger.Error($"Could not proccess Input Queue {queue.queueName}");
                }
            }
            // Sort the input into priority order
            input.Sort((x, y) => x.priority.CompareTo(y.priority));


            // The output queues. There may be multiple output queues for each input queue
            IEnumerable<XElement> OutEndPoints = from ep in pipeConfig.Descendants("output") select ep;
            int disambiguate = 0;
            foreach (XElement ep in OutEndPoints)
            {

                try
                {
                    QueueAbstract queue = queueFactory.GetQueue(ep, monitorPipelineProgress);
                    if (!queue.SupportsAsync())
                    {
                        this.allAsyncInputs = false;
                    }
                    // queue.SetMonitor(qMon, this);
                    if (msgCount.ContainsKey(queue.queueName))
                    {
                        queue.queueName = $"{queue.queueName}-{disambiguate}";
                        disambiguate++;
                    }
                    if (queue.queueName != null)
                    {
                        msgCount.Add(queue.queueName, 0);
                    }
                    else
                    {
                        msgCount.Add(queue.name, 0);
                    }
                    if (queue != null)
                    {
                        if (!queue.isValid)
                        {
                            logger.Warn($"Could not add Output Queue {queue.queueName} to {name}. Invalid Configuration");
                            continue;
                        }
                        output.Add(queue);
                    }
                    else
                    {
                        logger.Error($"Could not proccess Output Queue {queue.queueName}");
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message);
                }
            }

            if (this.allAsyncInputs)
            // Create a queue that all the inputs will sent messages to
            {
                try
                {

                    if (!MessageQueue.Exists(inputQueueName))
                    {
                        using (MessageQueue t = MessageQueue.Create(inputQueueName))
                        {
                            this.pipeInputQueue = t;
                            Console.WriteLine($"Created Input Queue {inputQueueName} for pipeline");
                        }
                    }
                    else
                    {
                        this.pipeInputQueue = new MessageQueue(inputQueueName);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            _contextCache = new MemoryCache(name);

            try
            {
                contextCacheKeyXPath = pipeConfig.Attribute("contextCacheKeyXPath").Value;
            }
            catch (Exception)
            {
                contextCacheKeyXPath = null;
            }

            try
            {
                this.contextCacheExpiry = double.Parse(pipeConfig.Attribute("contextCacheExpiry").Value);
            }
            catch (Exception)
            {
                this.contextCacheExpiry = 10.0;
            }

            try
            {
                this.discardInCache = bool.Parse(pipeConfig.Attribute("discardInCache").Value);
            }
            catch (Exception)
            {
                this.discardInCache = false;
            }

            try
            {
                this.firstOnly = bool.Parse(pipeConfig.Attribute("firstOnly").Value);
            }
            catch (Exception)
            {
                this.firstOnly = false;
            }

            try
            {
                this.useMessageAsKey = bool.Parse(pipeConfig.Attribute("useMessageAsKey").Value);
            }
            catch (Exception)
            {
                this.useMessageAsKey = false;
            }

            //Output and reset context
            if (contextCacheKeyXPath != null)
            {
                resetTimer = new System.Timers.Timer()
                {
                    AutoReset = true,
                    Interval = contextStatsInterval
                };
                logger.Info($"Context Cache Stats Interval set to {this.contextStatsInterval}");
                resetTimer.Elapsed += (source, eventArgs) =>
                {
                    statLogger.Info($"Total received: {this.totalReceived}, Total sent: {this.totalSent}");
                    statLogger.Info($">>>> Context Cache Stats for Previous {resetTimer.Interval}ms");

                    try
                    {
                        foreach (var v in statDict.Values)
                        {
                            statLogger.Info(v);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error($"Stats dictionary problem {ex.Message}");
                    }
                    finally
                    {
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

            if (output.Count() > 0)
            {
                OK_TO_RUN = true;
            }
            else
            {
                OK_TO_RUN = false;
                logger.Warn($"No inputs or outputs defined for Pipeline {name}");
            }
        }



        protected async Task<ExchangeMessage> GetMessage()
        {

            // All the inputs support async mode, so go wait for it
            if (this.allAsyncInputs)
            {
                return new ExchangeMessage(GetAsyncMessageFromInputQueue());
            }

            // Goes through the input queues and return the message.
            // After each message is returned, it always checks the queues 
            // in priority order, so the highest priority will always be cleared 
            // before messages are taken from the next highest queue

            bool immediateReturn = false;
            if (input.Count > 1)
            {
                // Flag to signal Input Listeners not to enter a permanaent wait cycle 
                // for the message, but to return null on the first time out. 
                // This is required for the prioritisation of input queues
                immediateReturn = true;
            }


            while (true)
            {
                foreach (QueueAbstract inQ in input)
                {
                    ExchangeMessage xm = await Task.Run(() => inQ.ListenToQueue(immediateReturn, priorityWait));
                    inQ.lastUsed = true;

                    if (xm != null)
                    {
                        if (xm.payload != null)
                        {
                            QXLog("Message Received", xm.uuid, "PROGRESS");
                            ReOrderInputs(inQ);
                            return xm;
                        }
                        else
                        {
                            if (immediateReturn)
                            {
                                // Return null so the pipeline can check other higher priority inputs 
                                ReOrderInputs(inQ);
                                return null;
                            }
                            else
                            {
                                QXLog("NULL Message", "Message was null after filtering and transformation", "WARNING");
                            }

                            // If we get here, then the input isn't part of a prioity set, so 
                            // Don't return anything so the loop continues to listen for messages to process
                        }
                    }
                }
            }
        }

        private string GetAsyncMessageFromInputQueue()
        {
            using (pipeInputQueue)
            {
                while (OK_TO_RUN)
                {
                    try
                    {
                        using (Message msg = pipeInputQueue.Receive())
                        {
                            // Wait for the next message on the input queue

                            msg.Formatter = new ActiveXMessageFormatter();
                            using (var reader = new StreamReader(msg.BodyStream))
                            {
                                return reader.ReadToEnd();
                            }
                        }
                    }
                    catch (MessageQueueException e)
                    {
                        // Handle no message arriving in the queue.
                        if (e.MessageQueueErrorCode == MessageQueueErrorCode.IOTimeout)
                        {

                        }
                        else
                        {
                            logger.Info($"Queue Error: {this.pipeInputQueue.Path} {e.StackTrace}");
                        }

                    }
                    catch (Exception ex)
                    {
                        logger.Trace(ex.Message);
                        logger.Info(ex, "Unhandled MSMQ listen Error");
                    }
                }
            }

            return null;

        }

        private void ReOrderInputs(QueueAbstract inQ)
        {
            //Rearranges the input list so the most recently used queue is put behind other
            // queue of the same priority. 

            int priority = inQ.priority;
            input.Remove(inQ);
            for (int i = 0; i < input.Count; i++)
            {
                if (input[i].priority > priority)
                {
                    input.Insert(i, inQ);
                    return;
                }
            }
            input.Add(inQ);
        }

        public void StopPipeLine()
        {
            foreach (QueueAbstract q in input)
            {
                try
                {
                    q.Stop();
                }
                catch (Exception)
                {
                    //
                }
            }
            foreach (QueueAbstract q in output)
            {
                try
                {
                    q.Stop();
                }
                catch (Exception)
                {
                    //
                }
            }
            OK_TO_RUN = false;
        }

        // Enables the changing of MaxMessagesPerMinute to change over the course of running.  
        private void PrepareProfileThreads()
        {
            string[] pairs = maxMsgPerMinuteProfile.Split(',');
            try
            {
                foreach (string pair in pairs)
                {
                    string[] pairString = pair.Split(':');
                    int minFromStart = int.Parse(pairString[0]);
                    int maxMess = int.Parse(pairString[1]);
                    int intervalThrottleInterval = 60000 / maxMess;
                    int waitBeforeStart = Math.Max(minFromStart * 60000, 5);

                    System.Timers.Timer resetTimer = new System.Timers.Timer
                    {
                        AutoReset = false,
                        Interval = waitBeforeStart,
                        Enabled = true
                    };
                    resetTimer.Elapsed += (sender, e) => MyElapsedMethod(sender, e, intervalThrottleInterval, maxMess);
                    resetTimer.Start();
                }
            }
            catch (Exception e)
            {
                logger.Error("**********************************");
                logger.Error("* Message Throttle Profile Error *");
                logger.Error("**********************************");
                Console.WriteLine(e.Message);
            }
        }

        private void MyElapsedMethod(object sender, ElapsedEventArgs e, int intervalThrottleInterval, int maxMess)
        {
            logger.Info($"Message Rate Profile. Setting Throttle interval = {throttleInterval}. Message Rate = {maxMess}");
            this.throttleInterval = intervalThrottleInterval;
        }

        public async Task StartPipeLine()
        {

            // At the pipeline level, the pipe can be configured to use or bypass the
            // filtering and transformation on each of the queues. 

            // Pipeline processing is simple, wait for something from the input queue and
            // if is not null, then distribute to the output according to the 
            // selected distribution pattern and then repeat 

            // The distribution to each of the output queues is done in a seperate async
            // Task so that they do not interfere with each other. 

            if (maxMsgPerMinuteProfile != null)
            {
                PrepareProfileThreads();
            }

            if (this.allAsyncInputs)
            {
                foreach (QueueAbstract inQ in input)
                {
                    logger.Info($"Starting Async Listener for {inQ.name}");
                    Task.Run(() => inQ.StartListener(this.inputQueueName));
                }
            }

            logger.Info($"Pipe {name} running. Input Queues = {input.Count()}, Output Queues = {output.Count()}");
            logger.Info($"Throttle interval = {throttleInterval}");

            while (OK_TO_RUN)
            {
                try
                {

                    //Get the message from the input
                    ExchangeMessage xm = await GetMessage();
                    QXLog("Recieved Message", null, "PROGRESS");

                    if (throttleInterval <= 0)
                    {
                        QXLog("Sending Message to Context Proceesor", null, "PROGRESS");
                        await ContextProcessorAsync(xm);
                    }
                    else
                    {
                        QXLog("Sending Message to Injector", null, "PROGRESS");
                        await InjectMessage(xm);
                        Thread.Sleep(throttleInterval);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            logger.Info($"Stopping Pipeline {name}");
        }

        public async Task ContextProcessorAsync(ExchangeMessage xm)
        {

            if (!xm.pass)
            {
                return;
            }

            // If no contextKey has been provided, just inject the message straight away
            if (contextCacheKeyXPath == null && !useMessageAsKey)
            {
                await InjectMessage(xm);
                return;
            }

            string contextKeyValue = getContextKey(xm);

            if (contextKeyValue == null)
            {
                await InjectMessage(xm);
                return;
            }

            try
            {

                Queue<ExchangeMessage> bufferMemoryQueue = null;
                System.Timers.Timer bufferPopperTimer = null;

                try
                {
                    // Get the buffer queue and popperTimer for this key
                    bufferMemoryQueue = _bufferMemoryQueueDict[contextKeyValue];
                    bufferPopperTimer = _bufferTimerDict[contextKeyValue];
                }
                catch (Exception)
                {

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
                if (statDict.ContainsKey(contextKeyValue))
                {
                    stats = statDict[contextKeyValue];
                }
                else
                {
                    ContextStats v = new ContextStats();
                    v.key = contextKeyValue;
                    statDict.Add(contextKeyValue, v);
                    stats = v;
                }

                stats.recieved++;

                this.totalReceived++;

                if (_contextCache.Contains(contextKeyValue) && this.discardInCache)
                {
                    logger.Info("Message found in Cache, but discard configured");
                    return;
                }

                if (mostRecentOnly)
                {
                    bufferMemoryQueue.Clear();
                }

                bufferMemoryQueue.Enqueue(xm);

                // So the message is enqueued, now just work out how long how to set the popper

                //Let's deal with it if it is not a firstOnly

                if (!firstOnly)
                {

                    if (!_contextCache.Contains(contextKeyValue))
                    {
                        //It's not in the cachce

                        if (!bufferPopperTimer.Enabled)
                        {
                            //If the popper isn't already enabled, enable it to pop straight away (5ms) 
                            bufferPopperTimer.Interval = 5.0;
                            bufferPopperTimer.Enabled = true;
                            bufferPopperTimer.Start();
                        }

                    }
                    else
                    {

                        //It's in tha cache, 
                        if (!bufferPopperTimer.Enabled)
                        {
                            //If the popper isn't already enabled, then set the popper. (If it is in the cache, then really, the popper should already be set) 
                            bufferPopperTimer.Interval = contextCacheExpiry * 1000;
                            bufferPopperTimer.Enabled = true;
                            bufferPopperTimer.Start();
                        }
                    }
                    return;
                }

                if (firstOnly)
                {
                    //It's not in the cache

                    bool inCache = _contextCache.Contains(contextKeyValue);
                    bool firstDone = _firstProcessedCompleted.Contains(contextKeyValue);
                    if (!inCache || firstDone)
                    {
                        _contextCache.AddOrGetExisting(contextKeyValue, DateTime.Now.AddSeconds(this.contextCacheExpiry), DateTime.Now.AddSeconds(this.contextCacheExpiry));
                        if (!bufferPopperTimer.Enabled)
                        {
                            bufferPopperTimer.Interval = 10.0;
                            bufferPopperTimer.Enabled = true;
                            bufferPopperTimer.Start();
                        }
                    }
                    else
                    {
                        if (!bufferPopperTimer.Enabled)
                        {
                            bufferPopperTimer.Interval = contextCacheExpiry * 1000;
                            bufferPopperTimer.Enabled = true;
                            bufferPopperTimer.Start();
                        }
                    }
                    return;
                }

            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }
        private String getContextKey(ExchangeMessage xm)
        {

            string contextKeyValue = null;

            if (useMessageAsKey)
            {
                QXLog("Context Proceesor", "Entire Message Being Used As Key", "PROGRESS");
                using (SHA256 mySHA256 = SHA256.Create())
                {

                    byte[] byteArray = Encoding.UTF8.GetBytes(xm.payload);


                    using (MemoryStream stream = new MemoryStream(byteArray))
                    {
                        byte[] hashValue = mySHA256.ComputeHash(stream);
                        contextKeyValue = BitConverter.ToString(hashValue);
                        logger.Trace($"Message Hash {contextKeyValue}");
                    }
                    return contextKeyValue;
                }
            }
            else if (contextCacheKeyXPath == "*")
            {
                return "ALL_MESSAGES";
            }
            else
            {

                // Look at the XML to get the contextKey
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xm.payload);
                XmlNamespaceManager ns = new XmlNamespaceManager(doc.NameTable);

                foreach (KeyValuePair<string, string> item in Exchange.nsDict)
                {
                    ns.AddNamespace(item.Key, item.Value);
                }


                try
                {
                    XmlNode node = doc.SelectSingleNode(contextCacheKeyXPath, ns);
                    contextKeyValue = node.InnerText;

                    logger.Info($"Context Key for Message = {contextKeyValue}");
                }
                catch
                {
                    return null;
                }
                QXLog("Context Proceesor", $"Context Key Retrieves Context Key = {contextKeyValue}", "PROGRESS");

                return contextKeyValue;
            }
        }
        private System.Timers.Timer CreatePopperTask(ExchangeMessage xm, Queue<ExchangeMessage> bufferMemoryQueue, string contextKeyValue)
        {

            double interval = contextCacheExpiry * 1000 + 100.0;

            logger.Info("Creating Popper Task");
            logger.Info($"firstOnly= {firstOnly}, First Completed = {_firstProcessedCompleted.Contains(contextKeyValue)} ");

            System.Timers.Timer bufferPopperTimer = new System.Timers.Timer
            {
                Interval = interval,
                Enabled = false
            };
            bufferPopperTimer.Elapsed += async (source, eventArgs) =>
            {
                bufferPopperTimer.AutoReset = false;
                bufferPopperTimer.Enabled = false;

                if (_firstProcessedCompleted.Contains(contextKeyValue))
                {
                    logger.Info("Pooper Expired - Marking it cleared <<<<<<<<<<<<<<<<<<");
                    _firstProcessedCleared.AddOrGetExisting(contextKeyValue, DateTime.Now.AddSeconds(this.contextCacheExpiry), DateTime.Now.AddSeconds(this.contextCacheExpiry));
                }

                if (bufferMemoryQueue.Count != 0)
                {
                    logger.Trace($"Injection Popper Timer Went Off for key = {contextKeyValue} - {bufferMemoryQueue.Count} Messages on queue");


                    _firstProcessedCompleted.AddOrGetExisting(contextKeyValue, contextKeyValue, DateTime.Now.AddHours(18));
                    _contextCache.AddOrGetExisting(contextKeyValue, DateTime.Now.AddSeconds(this.contextCacheExpiry), DateTime.Now.AddSeconds(this.contextCacheExpiry));

                    if (bufferMemoryQueue.Count > 0)
                    {
                        await InjectMessage(bufferMemoryQueue.Dequeue());
                    }
                    else
                    {
                        return;
                    }
                    if (firstOnly && _firstProcessedCleared.Contains(contextKeyValue))
                    {
                        bufferPopperTimer.Interval = 10.0;
                    }
                    else
                    {
                        bufferPopperTimer.Interval = contextCacheExpiry * 1000;
                    }
                    bufferPopperTimer.AutoReset = true;
                    bufferPopperTimer.Enabled = true;

                    try
                    {
                        statDict[contextKeyValue].send++;
                        statLogger.Trace(statDict[contextKeyValue]);
                    }
                    catch (Exception ex)
                    {
                        logger.Error($"Stats dictionary problem {ex.Message}");
                    }
                    this.totalSent++;
                }
                else
                {
                    logger.Trace($"Reinjection Popper Timer Went Off for key = {contextKeyValue} - No Message to Process");
                    if (_firstProcessedCleared.Contains(contextKeyValue))
                    {
                        bufferPopperTimer.Enabled = false;
                        bufferPopperTimer.AutoReset = false;
                    }
                    else
                    {
                        bufferPopperTimer.Interval = contextCacheExpiry * 1000;
                        bufferPopperTimer.Enabled = true;
                        bufferPopperTimer.AutoReset = true;
                    }
                }
            };

            return bufferPopperTimer;
        }
        protected async Task InjectMessage(ExchangeMessage xm)
        {

            if (xm != null)
            {

                if (!xm.pass)
                {
                    logger.Trace($"Blocked by incoming filter. Message {xm.uuid }");
                    return;
                }

                msgRecieved++;
                logger.Trace($"Processing message {xm.uuid }  [{msgRecieved}]");
                xm.status = "Message Recieved";

                // Pipeline specific logging
                QXLog("Starting Output Meesage Processing", null, "PROGRESS");

                if (randomDistribution)
                {
                    // Distributes the message to a random output

                    Random rnd = new Random();
                    int index = rnd.Next(0, output.Count);

                    logger.Trace($"Random Distributor. Sending message {xm.uuid } to {output[index].name}");
                    QXLog("Output Meesage Processing", "Distributing to Random Output", "PROGRESS");

                    if (outputIsolation)
                    {
                        QXLog("Async Message Start", "Distribution to Random Output", "PROGRESS");
                        _ = Task.Run(() => SendAndLog(output[index], xm));
                        QXLog("Async Message End", "Distribution to Random Output", "PROGRESS");
                    }
                    else
                    {
                        QXLog("Sync Message Start", "Distribution to Random Output", "PROGRESS");
                        _ = await SendAndLog(output[index], xm);
                        QXLog("Sync Message End", "Distribution to Random Output", "PROGRESS");
                    }

                }
                else if (roundRobinDistribution)
                {
                    // Distributes the message only once to each queue in turn
                    int index = nextOutput % output.Count;

                    logger.Trace($"Round Robin Distributor. Sending message {xm.uuid } to {output[index].name}");
                    QXLog("Output Message Processing", "Distribution to Round Robin Output", "PROGRESS");

                    if (outputIsolation)
                    {
                        QXLog("Async Message Start", "Distribution to Round Robin Output", "PROGRESS");
                        _ = Task.Run(() => SendAndLog(output[index], xm));
                        QXLog("Async Message End", "Distribution to Round Robin Output", "PROGRESS");
                    }
                    else
                    {
                        QXLog("Sync Message Start", "Distribution to Round Robin Output", "PROGRESS");
                        _ = await SendAndLog(output[index], xm);
                        QXLog("Sync Message End", "Distribution to Round Robin Output", "PROGRESS");
                    }

                    nextOutput++;

                }
                else
                {
                    // Distribute the message to all the outputs
                    if (outputIsolation)
                    {
                        // Wont wait for the Send to complete before completing
                        foreach (QueueAbstract q in output)
                        {
                            QXLog("Async Message Start", "Normal Distribution", "PROGRESS");
                            _ = Task.Run(() => SendAndLog(q, xm));
                            QXLog("Async Message End", "Normal Distribution", "PROGRESS");
                        }
                    }
                    else
                    {

                        // Will wait for all Sends to complete before proceeding
                        QXLog("Sync Message Start", "Normal Distribution", "PROGRESS");

                        foreach (QueueAbstract q in output)
                        {
                            logger.Info($"Sending message {xm?.uuid } to {q.name}");
                            _ = await SendAndLog(q, xm);

                            if (xm.sent)
                            {
                                if (q.queueName != null)
                                {
                                    msgCount[q.queueName] = msgCount[q.queueName] + 1;
                                    logger.Trace($"{q.queueName} = {msgCount[q.queueName]}");
                                }
                                else
                                {
                                    msgCount[q.name] = msgCount[q.name] + 1;
                                    logger.Trace($"{q.name} = {msgCount[q.name]}");
                                }
                            }

                        }
                        QXLog("Sync Message End", "Normal Distribution", "PROGRESS");
                    }
                }
            }

        }
        private async Task<ExchangeMessage> SendAndLog(QueueAbstract queue, ExchangeMessage message)
        {
            message = await queue.Send(message);
            return message;
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
                Dispose(true);
                GC.SuppressFinalize(this);
            }
            catch { }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Clear all property values that maybe have been set
                    // when the class was instantiated
                    _contextCache.Dispose();
                }

                // Indicate that the instance has been disposed.
                _disposed = true;
            }
        }
        private void MonitorStatusMessage(object sender, QueueMonitorMessage e)
        {
            PipelineMonitorMessage msg = new PipelineMonitorMessage(e);
            msg.pipeID = this.id;
            msg.pipeName = this.name;
            msg.pipemessageType = "QUEUEMESSAGE";
            monitorMessageProgress?.Report(msg);
        }
        public void QXLog(string topic, string message, string messageType)
        {
            PipelineMonitorMessage msg = new PipelineMonitorMessage(id, name, topic, message, messageType);
            monitorMessageProgress?.Report(msg);
        }

    }
}
