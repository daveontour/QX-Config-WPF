using System;
using System.IO;
using System.Management;
using System.Messaging;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace QueueExchange {
    class QERest : QueueAbstract, IDisposable {

        private string requestURL;
        private HttpListener listener;
        private QEMSMQ serviceQueue;
        public object sendLock = new object();
        public int maxMessages = -1;

        public QERest(XElement defn) : base(defn) { }

        public override bool SetUp() {

            OK_TO_RUN = false;




            try {
                requestURL = definition.Attribute("requestURL").Value;
            } catch (Exception) {
                return false;
            }
            if (definition.Name == "output") {
                try {
                    serviceQueue = new QEMSMQ(bufferQueueName);
                    OK_TO_RUN = true;
                    _ = Task.Run(() => StartSimpleGetListener());

                    // Maintain the buffer queue to a limited length if configure
                    try {
                        maxMessages = Int32.Parse(definition.Attribute("maxMessages").Value);
                        if (maxMessages != -1) {
                            _ = Task.Run(() => MaintainQueue());
                        }
                    } catch (Exception) {
                        maxMessages = -1;
                    }
                    return true;

                } catch (Exception ex) {
                    logger.Error(ex);
                    return false;
                }
            } else {
                return false;
            }


        }

        public new void Stop() {
            OK_TO_RUN = false;

            try {
                serviceQueue.Stop();
            } catch (Exception) {
                //
            }
        }

        public override ExchangeMessage Listen(bool immediateReturn, int priorityWait) {
            // this type can only be used as an output
            return null;
        }

        public override async Task<ExchangeMessage> SendToOutputAsync(ExchangeMessage mess) {

            // Send the message to the bufferQueue, so it is available when a HTTP GET request comes in
            mess = await serviceQueue.SendToOutputAsync(mess);
            mess.status += "Added to HTTPGET Queue";
            return mess;
        }

        public string StartSimpleGetListener() {

            // Start a HTTP listener that returns the first message on the bufffer queue when a GET request come in.

            if (!HttpListener.IsSupported) {
                logger.Error("Windows XP SP2 or Server 2003 is required to use the HttpListener class.");
                return null;
            }

            try {
                listener = new HttpListener();
                listener.Prefixes.Add(requestURL);
                listener.Start();
                logger.Trace("Listening on " + requestURL + "...");
            } catch (Exception e) {
                logger.Error(e.Message);
            }
            while (OK_TO_RUN) {
                // Note: The GetContext method blocks while waiting for a request. 
                HttpListenerContext context = listener.GetContext();
                HttpListenerRequest request = context.Request;

                string reply;

                if (request.HttpMethod == "GET") {
                    logger.Trace("Get Request Recieved");
                    try {
                        serviceQueue.ResetReadConnect();
                        using (Message msg = serviceQueue.msgQueue.Receive(new TimeSpan(0, 0, 0))) {

                            msg.Formatter = new ActiveXMessageFormatter();

                            using (var reader = new StreamReader(msg.BodyStream)) {
                                reply = reader.ReadToEnd();
                            }
                        }
                    } catch (Exception e) {
                        logger.Error(e.Message);
                        reply = "NULL";
                    }
                } else {
                    reply = "NULL";
                }

                // Obtain a response object.
                HttpListenerResponse response = context.Response;
                response.StatusCode = 200;

                logger.Trace(reply);
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(reply);
                // Get a response stream and write the response to it.

                response.ContentLength64 = buffer.Length;
                Stream output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
                output.Close();
            }

            return null;
        }

        public void MaintainQueue() {

            // Keeps the queue to a maximum size by reading of the oldest messages.
            using (MessageQueue msgQueue = new MessageQueue(bufferQueueName)) {
                while (true) {
                    serviceQueue.ResetReadConnect();

                    try {
                        lock (sendLock) {
                            long count = 0;
                            try {
                                count = GetMessaegCount(bufferQueueName);
                            } catch (Exception) {
                                count = msgQueue.GetAllMessages().Length;
                            }

                            logger.Trace($"Maintenance for Queue {bufferQueueName}. Length = {count}");
                            for (int i = 0; i < count - this.maxMessages; i++) {
                                msgQueue.Receive();
                            }
                        }

                        //Sleep for 10 seconds and then do it again
                        Thread.Sleep(10000);

                    } catch (Exception ex) {
                        logger.Error(ex, $"Error Maininting the Queue Size {bufferQueueName}");
                        //Sleep for 10 seconds and then do it again
                        Thread.Sleep(10000);
                    }
                }
            }
        }

        private static long GetMessaegCount(String msmqName) {

            msmqName = msmqName.Split('\\')[2].ToLower();
            ManagementObjectCollection wmiCollection = null;
            using (ManagementObjectSearcher wmiSearch = new ManagementObjectSearcher(String.Format("SELECT Name,MessagesinQueue FROM Win32_PerfRawdata_MSMQ_MSMQQueue"))) {
                try {
                    wmiCollection = wmiSearch.Get();
                } catch (Exception ex) {
                    logger.Error(ex.Message);
                }
            }


            foreach (ManagementObject wmiObject in wmiCollection) {
                if (!wmiObject.Path.Path.Contains(msmqName)) {
                    continue;
                }
                foreach (PropertyData wmiProperty in wmiObject.Properties) {
                    if (wmiProperty.Name.Equals("MessagesinQueue", StringComparison.InvariantCultureIgnoreCase)) {
                        return long.Parse(wmiProperty.Value.ToString());
                    }
                }
            }
            throw new ArgumentException(String.Format("MSMQ with name {0} not found", msmqName));
        }

        public void Dispose() {
            try {
                listener.Close();
                serviceQueue.Dispose();
            } catch { }
        }
    }
}