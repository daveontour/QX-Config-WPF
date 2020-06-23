using IBM.WMQ;
using System;
using System.Collections;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace QueueExchange
{
    class QEMQ : QueueAbstract
    {
        private string qMgr;
        private string qSvrChan;
        private string qHost;
        private string qPort;
        private string qUser = null;
        private string qPass = null;
        private int maxMessages = -1;
        private readonly Hashtable connectionParams = new Hashtable();
        public object sendLock = new object();

        public QEMQ(XElement defn, IProgress<QueueMonitorMessage> monitorMessageProgress) : base(defn, monitorMessageProgress) { }
        public override bool SetUp()
        {

            OK_TO_RUN = false;

            try
            {
                qMgr = definition.Attribute("queueMgr").Value;
            }
            catch (Exception)
            {
                // Can't do anything if the connection is not speicifed
                return false;
            }
            try
            {
                qSvrChan = definition.Attribute("channel").Value;
            }
            catch (Exception)
            {
                // Can't do anything if the connection is not speicifed
                return false;
            }

            try
            {
                qHost = definition.Attribute("host").Value;
            }
            catch (Exception)
            {
                // Can't do anything if the connection is not speicifed
                return false;
            }

            try
            {
                qPort = definition.Attribute("port").Value;
            }
            catch (Exception)
            {
                // Can't do anything if the connection is not speicifed
                return false;
            }

            try
            {
                qUser = definition.Attribute("username").Value;
            }
            catch (Exception)
            {
                qUser = null;
            }

            try
            {
                qPass = definition.Attribute("password").Value;
            }
            catch (Exception)
            {
                qPass = null;
            }

            try
            {
                // Set the connection parameter 
                connectionParams.Add(MQC.CHANNEL_PROPERTY, qSvrChan);
                connectionParams.Add(MQC.HOST_NAME_PROPERTY, qHost);
                connectionParams.Add(MQC.PORT_PROPERTY, qPort);
                connectionParams.Add(MQC.TRANSPORT_PROPERTY, MQC.TRANSPORT_MQSERIES_MANAGED);

                if (qUser != null)
                {
                    connectionParams.Add(MQC.USER_ID_PROPERTY, qUser);
                }
                if (qPass != null)
                {
                    connectionParams.Add(MQC.PASSWORD_PROPERTY, qPass);
                }
            }
            catch (Exception)
            {
                return false;
            }

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

        public override ExchangeMessage Listen(bool immediateReturn, int priorityWait)
        {

            if (immediateReturn)
            {
                getTimeout = priorityWait;
            }
            while (OK_TO_RUN)
            {
                try
                {
                    using (MQQueueManager queueManager = new MQQueueManager(qMgr, connectionParams))
                    {
                        var openOptions = MQC.MQOO_INPUT_AS_Q_DEF + MQC.MQOO_FAIL_IF_QUIESCING;
                        MQQueue queue = queueManager.AccessQueue(queueName, openOptions);

                        MQGetMessageOptions getOptions = new MQGetMessageOptions
                        {
                            WaitInterval = getTimeout,
                            Options = MQC.MQGMO_WAIT
                        };

                        MQMessage msg = new MQMessage
                        {
                            Format = MQC.MQFMT_STRING
                        };

                        queue.Get(msg, getOptions);
                        queue.Close();

                        return new ExchangeMessage(msg.ReadString(msg.MessageLength));
                    }
                }
                catch (Exception ex)
                {
                    // Exception occurs on read timeout or on failure to connect
                    logger.Trace($"No messages on: {queueName} { ex.Message }");
                    if (immediateReturn)
                    {
                        return null;
                    }
                    else
                    {
                        continue;
                    }
                }
            };

            return null;
        }
        public new async void Send(ExchangeMessage xm)
        {
            logger.Debug($"Sending to {xm.uuid} IBMMQ {queueName}");
            await SendToOutputAsync(xm);
        }
        public override async Task<ExchangeMessage> SendToOutputAsync(ExchangeMessage mess)
        {

            if (maxMessages != -1)
            {
                MaintainQueueInline();
            }

            try
            {

                mess = await SendMqStuff(mess);
                if (mess.sent)
                {
                    logger.Debug($"MQ Sent to {name}");
                    mess.sent = true;
                    mess.status = $"Sent to {queueName}";
                    return mess;
                }
                else
                {
                    logger.Error($"MQ Failed Sent to  {name}");
                    SendToUndeliverableQueue(mess);
                    mess.sent = false;
                    mess.status = $"Unable to Send to {queueName}";
                    return mess;
                }

            }
            catch (Exception ex)
            {
                if (!isLogger)
                {
                    logger.Error(ex, $"MQ Failed Sent to {name}");
                }
                SendToUndeliverableQueue(mess);
                mess.sent = false;
                mess.status = $"Unable to Send to {queueName}";
                return mess;
            }

        }

        public async Task<ExchangeMessage> SendMqStuff(ExchangeMessage xm)
        {

            await Task.Run(() => { });
            string messageXML = xm.payload;
            bool sent = false;
            int tries = 1;

            lock (sendLock)
            {

                do
                {

                    if (tries > maxRetry && maxRetry > 0)
                    {
                        xm.sent = false;
                        xm.status = $"Exceeded Connection retries on MQ {queueName}";
                        return xm;
                    }

                    try
                    {
                        using (MQQueueManager queueManager = new MQQueueManager(qMgr, connectionParams))
                        {
                            var openOptions = MQC.MQOO_OUTPUT + MQC.MQOO_FAIL_IF_QUIESCING;
                            MQQueue queue = queueManager.AccessQueue(queueName, openOptions);
                            var message = new MQMessage
                            {
                                CharacterSet = 1208 // UTF-8
                            };
                            message.WriteString(messageXML);
                            message.Format = MQC.MQFMT_STRING;
                            MQPutMessageOptions putOptions = new MQPutMessageOptions();
                            queue.Put(message, putOptions);
                            queue.Close();
                            sent = true;
                            logger.Trace($"===Message Sent {xm.uuid} to {queueName}");
                        }
                    }
                    catch (Exception ex)
                    {
                        if (!isLogger)
                        {
                            logger.Info("Unable to send message to : " + queueName + " : " + ex.Message);
                        }
                        tries++;
                    }
                    if (!sent)
                    {
                        logger.Trace($"===Message NOT Sent {xm.uuid} to  {queueName}");
                    }
                } while (!sent);
            }
            xm.sent = true;
            xm.status = $"Sent to  MQ {queueName}";

            return xm;
        }

        public void MaintainQueueInline()
        {


            try
            {
                using (MQQueueManager queueManager = new MQQueueManager(qMgr, connectionParams))
                {

                    int openOptions = MQC.MQOO_INPUT_AS_Q_DEF + MQC.MQOO_OUTPUT + MQC.MQOO_FAIL_IF_QUIESCING + MQC.MQOO_INQUIRE;
                    MQQueue queue = queueManager.AccessQueue(queueName, openOptions);
                    MQGetMessageOptions getOptions = new MQGetMessageOptions { WaitInterval = 5, Options = MQC.MQGMO_WAIT };
                    MQMessage msg = new MQMessage { Format = MQC.MQFMT_STRING };

                    while (queue.CurrentDepth > this.maxMessages)
                    {
                        GetMessage();
                    }
                    queue.Close();
                }
            }
            catch (Exception ex)
            {
                if (!isLogger)
                {
                    logger.Error($"Unable to maintain queue: {queueName}: {ex.Message}");
                }

            }
        }


        private void GetMessage()
        {
            try
            {
                using (MQQueueManager queueManager = new MQQueueManager(qMgr, connectionParams))
                {
                    var openOptions = MQC.MQOO_INPUT_AS_Q_DEF + MQC.MQOO_OUTPUT + MQC.MQOO_FAIL_IF_QUIESCING + MQC.MQOO_INQUIRE;
                    MQQueue queue = queueManager.AccessQueue(queueName, openOptions);
                    MQGetMessageOptions getOptions = new MQGetMessageOptions { WaitInterval = 5, Options = MQC.MQGMO_WAIT };
                    MQMessage msg = new MQMessage { Format = MQC.MQFMT_STRING };
                    queue.Get(msg, getOptions);
                    queue.Close();
                }
            }
            catch (Exception ex)
            {
                if (!isLogger)
                {
                    logger.Error($"Unable to maintain queue: {queueName}: {ex.Message}");
                }
            }
        }
    }
}


