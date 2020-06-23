using Confluent.Kafka;
using System;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace QueueExchange
{
    class QEKafka : QueueAbstract, IDisposable
    {

        private string topic;
        private string key;
        private string bootStrapServers = "localhost:9092";
        public QEMSMQ serviceQueue;
        private string consumerGroup;


        public QEKafka(XElement defn, IProgress<QueueMonitorMessage> monitorMessageProgress) : base(defn, monitorMessageProgress)
        {

        }

        public override bool SetUp()
        {

            OK_TO_RUN = false;

            try
            {
                bootStrapServers = definition.Attribute("connection").Value;
            }
            catch (Exception)
            {
                return false;
            }
            try
            {
                key = definition.Attribute("key").Value;
            }
            catch (Exception)
            {
                key = null;
            }


            try
            {
                consumerGroup = definition.Attribute("consumerGroup").Value;
            }
            catch (Exception)
            {
                consumerGroup = "QueueExchange";
            }
            try
            {
                topic = definition.Attribute("topic").Value;
            }
            catch (Exception)
            {
                topic = "my_topic";
            }

            if (key == null)
            {
                key = topic;
            }

            try
            {
                // Create a service queue manager to write to and read from the buffer queue
                serviceQueue = new QEMSMQ(bufferQueueName);
            }
            catch (Exception)
            {
                bufferQueueName = null;
                if (definition.Name == "input")
                {
                    logger.Error($"A bufferQueueName must be correctly specified for a KAFKA interface");
                    return false;
                }
            }

            OK_TO_RUN = true;

            if (definition.Name == "input")
            {
                _ = Task.Run(() => Run_Consume());
            }

            return true;

        }

        public new void Stop()
        {
            OK_TO_RUN = false;
            if (serviceQueue != null)
            {
                serviceQueue.Stop();
            }
        }
        public override ExchangeMessage Listen(bool immediateReturn, int priorityWait)
        {
            // The serviceQueue is monitored and messages are returned as the appear on the queue
            logger.Debug("Listening to KAFKA QUEUE");
            return (serviceQueue.Listen(immediateReturn, priorityWait));
        }

        public new async void Send(ExchangeMessage xm)
        {
            logger.Debug($"Sending to {xm.uuid} Kafka Topic {topic}");
            await SendToOutputAsync(xm);
        }
        public override async Task<ExchangeMessage> SendToOutputAsync(ExchangeMessage mess)
        {

            var config = new ProducerConfig { BootstrapServers = this.bootStrapServers };

            // Set the topic bassed on the content of the message if configured
            mess = SetDestinationFromMessage(mess);
            if (mess.destinationSet)
            {
                topic = queueName;
            }
            else
            {
                if (topic == null)
                {
                    mess.sent = false;
                    return mess;
                }
            }

            using (var p = new ProducerBuilder<string, string>(config).Build())
            {
                try
                {
                    var dr = await p.ProduceAsync(topic, new Message<string, string> { Key = key, Value = mess.payload });
                }
                catch (Exception e)
                {
                    logger.Error(e.Message);
                    logger.Error(e);
                    logger.Error($"Unable to deliver to Kafka Server on topic {topic}");
                    SendToUndeliverableQueue(mess);
                    mess.sent = false;
                    mess.status = $"Unable to Send to Kafka Topic = {topic}";
                    return mess;

                }
                logger.Info($"Kafka sent message to {topic}");
            }

            mess.sent = true;
            mess.status = $"Sent to Kafka Topic = {topic}";
            return mess;
        }



        public void Run_Consume()
        {
            logger.Info("Starting the KAFKA Consumer");

            var config = new ConsumerConfig
            {
                BootstrapServers = bootStrapServers,
                GroupId = consumerGroup,
                EnableAutoCommit = true,
                StatisticsIntervalMs = 5000,
                SessionTimeoutMs = 6000,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnablePartitionEof = true
            };

            using (var consumer = new ConsumerBuilder<Ignore, string>(config).Build())
            {

                consumer.Subscribe(topic);

                try
                {
                    while (OK_TO_RUN)
                    {
                        try
                        {
                            var consumeResult = consumer.Consume();

                            if (consumeResult.IsPartitionEOF)
                            {
                                logger.Info($"Reached end of topic {consumeResult.Topic}, partition {consumeResult.Partition}, offset {consumeResult.Offset}.");
                                continue;
                            }

                            logger.Info("=============Recieved Message From Kafka============");
                            _ = serviceQueue.ServiceSend(new ExchangeMessage(consumeResult.Value));

                            //if (consumeResult.Offset % commitPeriod == 0) {
                            //    try {
                            //        consumer.Commit(consumeResult);
                            //    } catch (KafkaException e) {
                            //        logger.Info($"Commit error: {e.Error.Reason}");
                            //    }
                            //}

                        }
                        catch (ConsumeException e)
                        {
                            logger.Info($"Consume error: {e.Error.Reason}");
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    // consumer.Close();
                }
            }
        }

        public void Dispose()
        {
            try
            {
                serviceQueue.Dispose();
            }
            catch { }
        }
    }
}
