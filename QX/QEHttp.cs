using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace QueueExchange {
    class QEHTTP : QueueAbstract, IDisposable {


        private string postURL;
        private readonly Dictionary<string, string> headers = new Dictionary<string, string>();
        private HttpListener listener;
        private QEMSMQ serviceQueue;
        private bool isXML;

        public QEHTTP(XElement defn) : base(defn) { }

        public override bool SetUp() {

            OK_TO_RUN = false;

            //try {
            //    requestURL = definition.Attribute("requestURL").Value;
            //} catch (Exception) {
            //    if (definition.Name == "input") {
            //        return false;
            //    }
            //}
            try {
                postURL = definition.Attribute("postURL").Value;
            } catch (Exception) {
                if (definition.Name == "output") {
                    return false;
                }
            }

            try {
                this.isXML = bool.Parse(definition.Attribute("isXML").Value);
            } catch (Exception) {
                this.isXML = true;
            }
            try {
                // Create a service queue manager to write to and read from the buffer queue                
                if (definition.Name == "input") {
                    serviceQueue = new QEMSMQ(bufferQueueName, isXML);
                    _ = Task.Run(() => StartSimpleListener());
                }
            } catch (Exception ex) {
                logger.Error($"HTTP Listener Error {ex.Message}");
                bufferQueueName = null;
                return false;
            }

            OK_TO_RUN = true;

            return true;

        }

        public new void Stop() {
            OK_TO_RUN = false;

            try {
                if (serviceQueue != null) {
                    serviceQueue.Stop();
                }
            } catch (Exception) {
                //
            }
        }

        public override ExchangeMessage Listen(bool immediateReturn, int priorityWait) {

            // The HTTP listener placesany received messages on the service queue,
            // So they are ready for the picking, just need to use the Listen method
            // on the servic queue to get the message

            ExchangeMessage xm = serviceQueue.Listen(immediateReturn, priorityWait);

            logger.Trace(xm.payload);
            return xm;
        }

        public override async Task<ExchangeMessage> SendToOutputAsync(ExchangeMessage mess) {

            try {

                bool status = await SendHTTPStuff(mess);

                if (status) {
                    logger.Trace($"Sent to {name}");
                } else {
                    logger.Error($"Failed Sent to {name}");
                    SendToUndeliverableQueue(mess);
                    mess.sent = false;
                    return mess;
                }

            } catch (Exception ex) {
                logger.Error(ex, $"Failed Sent to {name}");
                this.SendToUndeliverableQueue(mess);
                mess.sent = false;
                return mess;

            }
            mess.sent = true;
            return mess;
        }

        public async Task<bool> SendHTTPStuff(ExchangeMessage message) {

            string messageXML = message.payload;

            bool sent = false;
            int tries = 1;

            do {

                if (tries > maxRetry && maxRetry > 0) {
                    return sent;
                }

                try {
                    using (var client = new HttpClient()) {

                        HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, postURL) {
                            Content = new StringContent(messageXML, Encoding.UTF8, "application/xml")
                        };

                        if (headers != null) {
                            foreach (var item in headers) {
                                requestMessage.Headers.Add(item.Key, item.Value);
                            }
                        }

                        using (HttpResponseMessage response = await client.SendAsync(requestMessage)) {
                            if (response.StatusCode == HttpStatusCode.OK) {
                                sent = true;
                                return sent;
                            } else {
                                throw new Exception();
                            }
                        }
                    }
                } catch (Exception ex) {
                    logger.Info("Unable to send message to : " + queueName + " : " + ex.Message);
                    tries++;
                }
            } while (!sent && OK_TO_RUN);

            return sent;
        }

        public void StartSimpleListener() {

            if (!HttpListener.IsSupported) {
                logger.Error("Windows XP SP2 or Server 2003 is required to use the HttpListener class.");
                return;
            }

            try {
                listener = new HttpListener();
                listener.Prefixes.Add(postURL);
                listener.Start();
                logger.Trace("Listening on " + postURL + "...");
            } catch (Exception ex) {
                logger.Error($"Could not start HTTP Listener {ex.Message}");
            }

            // Recieved messages are written to a buffer queue using a service MSMQ object
            // The written records are read in the main Listen method and returned
            while (true) {
                // Note: The GetContext method blocks while waiting for a request. 
                HttpListenerContext context = listener.GetContext();
                HttpListenerRequest request = context.Request;

                string message = null;
                if (!request.HasEntityBody) {
                    continue;
                }

                using (StreamReader reader = new StreamReader(request.InputStream, request.ContentEncoding)) {
                    message = reader.ReadToEnd();
                }


                // Obtain a response object.
                HttpListenerResponse response = context.Response;
                response.StatusCode = 200;

                byte[] buffer = System.Text.Encoding.UTF8.GetBytes("OK");
                // Get a response stream and write the response to it.

                response.ContentLength64 = buffer.Length;
                Stream output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
                output.Close();

                // serviceQueue.Send(new ExchangeMessage(message));
                _ = Task.Run(() => serviceQueue.ServiceSend(new ExchangeMessage(message)));
            }
        }

        public void Dispose() {
            try {
                listener.Close();
                serviceQueue.Dispose();
            } catch { }
        }
    }
}


