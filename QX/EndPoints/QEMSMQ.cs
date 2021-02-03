using System;
using System.IO;
using System.Management;
using System.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace QueueExchange
{
    class QEMSMQ : QueueAbstract, IDisposable
    {

        public MessageQueue msgQueue;
        private int maxMessages = -1;
        private readonly object sendLock = new object();


        public QEMSMQ(XElement defn, IProgress<QueueMonitorMessage> monitorMessageProgress) : base(defn, monitorMessageProgress)
        {
            CreateQueue(queueName);
        }

        public QEMSMQ(string qDef) : base(qDef)
        {

            if (qDef != null)
            {

                if (!qDef.Contains("$"))
                {
                    qDef = ".\\private$\\" + qDef;
                }
                logger.Trace($"QUEUE NAME = {qDef}");
                this.queueName = qDef;
                CreateQueue(queueName, true);
            }
        }

        public override bool SetUp()
        {
            logger.Info(queueName);

            // On output queues, if the maxMessages parameter is set, then set up a task to maitain the queue size at or below the value set
            try
            {
                if (definition.Name == "output")
                {
                    maxMessages = Int32.Parse(definition.Attribute("maxMessages").Value);
                }
            }
            catch (Exception)
            {
                maxMessages = -1;
            }

            OK_TO_RUN = true;

            return true;
        }

        public new void Stop()
        {
            OK_TO_RUN = false;
        }

        override public async Task StartListener()
        {
            await Task.Run(() =>
             {
                 // Wait until we can connect
                 ResetReadConnect();

                 using (msgQueue)
                 {
                     while (OK_TO_RUN)
                     {
                         try
                         {
                             // Wait for the next message on the input queue
                             using (Message msg = msgQueue.Receive(new TimeSpan(0, 0, 0, 30)))
                             {
                                 msg.Formatter = new ActiveXMessageFormatter();
                                 using (var reader = new StreamReader(msg.BodyStream))
                                 {
                                     string message = reader.ReadToEnd();
                                     logger.Trace($"Messages recieved on {queueName}");

                                     SendToPipe(message);
                                 }
                             }
                         }
                         catch (MessageQueueException e)
                         {
                             // Handle no message arriving in the queue.
                             if (e.MessageQueueErrorCode == MessageQueueErrorCode.IOTimeout)
                             {
                                 logger.Info($"No messages on {queueName}");
                             }
                             else
                             {
                                 logger.Info($"Queue Error: {queueName} {e.StackTrace}");
                                 ResetReadConnect();
                             }
                         }
                         catch (Exception ex)
                         {
                             logger.Trace(ex.Message);
                             logger.Info(ex, "Unhandled MSMQ listen Error");
                         }
                     }
                 }
             });
        }

        public void ResetReadConnect()
        {

            //Connect to the queue that is to be read

            msgQueue = new MessageQueue(queueName);
            while (!this.TestReadConnect())
            {
                logger.Error("Cannot Read from Queue: " + this.queueName);
                Thread.Sleep(2000);
            }
        }
        private int ResetWriteConnect()
        {

            //Connect to the queue that is to be written to

            msgQueue = new MessageQueue(queueName);
            int counter = 0;
            while (!this.TestWriteConnect() && (counter < maxRetry || maxRetry <= 0))
            {

                logger.Error($"Cannot Write to Queue: {queueName}  {counter}/ {maxRetry}");
                counter++;
                Thread.Sleep(2000);
            }

            return counter;
        }
        public override async Task<ExchangeMessage> SendToOutputAsync(ExchangeMessage mess)
        {

            await Task.Run(() => { });

            if (maxMessages != -1)
            {
                MaintainQueueInline();
            }

            //// Connection to the queue
            if (ResetWriteConnect() == maxRetry)
            {
                logger.Error($"Cannot Write to Queue: {queueName}");
                this.SendToUndeliverableQueue(mess);
                mess.sent = false;
                mess.status = $"Unable to deliver to {queueName}. Sent to Undeliverable";
                return mess;
            }

            // Locking on send is neccessary because writing to a MSMQ queue is not thread safe
            // and this method may be concurrently called.
            // The lock only protects against write clasehes by this onbject - other objects may 
            // indepentantly try to write to the queue. 

            lock (sendLock)
            {
                try
                {
                    using (msgQueue)
                    {
                        string payload = mess.payload;

                        try
                        {
                            Message myMessage = new Message(Encoding.ASCII.GetBytes(payload), new ActiveXMessageFormatter());
                            msgQueue.Send(myMessage);
                            mess.sent = true;
                            mess.status = $"Sent to {queueName}";
                            return mess;
                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex.Message);
                            logger.Error(ex.StackTrace);
                            mess.sent = false;
                            mess.status = $"Unable to deliver to {queueName}. Sent to Undeliverable";
                            return mess;
                        }
                    }
                }
                catch (Exception ex)
                {

                    logger.Error(ex, $"MSMQ Error Sending to {queueName}");
                    SendToUndeliverableQueue(mess);
                    mess.sent = false;
                    mess.status = $"Unable to deliver to {queueName}. Sent to Undeliverable";
                    return mess;
                }
            }
        }
        private bool TestReadConnect()
        {
            try
            {
                return msgQueue.CanRead;
            }
            catch (MessageQueueException)
            {
                return false;
            }
        }
        private bool TestWriteConnect()
        {
            try
            {
                return msgQueue.CanWrite;
            }
            catch (MessageQueueException)
            {
                return false;
            }
        }
        public void MaintainQueueInline()
        {

            // Keeps the queue to a maximum size by reading of the oldest messages.
            ResetWriteConnect();
            try
            {
                lock (sendLock)
                {
                    long count = 0;
                    try
                    {
                        count = GetMessaegCount(queueName);
                    }
                    catch (Exception)
                    {
                        count = msgQueue.GetAllMessages().Length;
                    }

                    logger.Trace($"Maintenance for Queue {queueName}. Length = {count}");
                    for (int i = 0; i <= count - this.maxMessages; i++)
                    {
                        msgQueue.Receive();
                    }
                }

            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error Maininting the Queue Size {queueName}");
            }

        }
        private static long GetMessaegCount(String msmqName)
        {

            msmqName = msmqName.Split('\\')[2].ToLower();
            ManagementObjectCollection wmiCollection = null;
            using (ManagementObjectSearcher wmiSearch = new ManagementObjectSearcher(String.Format("SELECT Name,MessagesinQueue FROM Win32_PerfRawdata_MSMQ_MSMQQueue")))
            {
                try
                {
                    wmiCollection = wmiSearch.Get();
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message);
                }
            }


            foreach (ManagementObject wmiObject in wmiCollection)
            {
                if (!wmiObject.Path.Path.Contains(msmqName))
                {
                    continue;
                }
                foreach (PropertyData wmiProperty in wmiObject.Properties)
                {
                    if (wmiProperty.Name.Equals("MessagesinQueue", StringComparison.InvariantCultureIgnoreCase))
                    {
                        return long.Parse(wmiProperty.Value.ToString());
                    }
                }
            }
            throw new ArgumentException(String.Format("MSMQ with name {0} not found", msmqName));
        }
        public void Dispose()
        {
            try
            {
                msgQueue.Dispose();
            }
            catch { }
        }
    }
}


