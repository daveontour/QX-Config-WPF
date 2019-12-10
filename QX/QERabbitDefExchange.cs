using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace QueueExchange {
    class QERabbitDefExchange : QueueAbstract, IDisposable {

        public QEMSMQ serviceQueue;
        public QERabbitDefExchange(XElement defn) : base(defn) { }

        public override bool SetUp() {
            OK_TO_RUN = false;

            try {
                connection = definition.Attribute("connection").Value;
            } catch (Exception) {
                logger.Error("Connection not defined for RabbitMQ");
                return false;
            }

            if (definition.Name == "input") {
                serviceQueue = new QEMSMQ(bufferQueueName);
                OK_TO_RUN = true;
                _ = Task.Run(() => StartListen());
            }

            return true;
        }

        public override ExchangeMessage Listen(bool immediateReturn, int priorityWait) {
            // The serviceQueue is monitored and messages are returned as the appear on the queue
            logger.Debug("Listening to Rabbit QUEUE");
            return (serviceQueue.Listen(immediateReturn, priorityWait));
        }

        public void StartListen() {

            if (!OK_TO_RUN) {
                logger.Error("Rabbit Input not conffgured correctly to run");
                return;
            }

            try {
                var factory = new ConnectionFactory() { HostName = connection };
                var conn = factory.CreateConnection();
                var channel = conn.CreateModel();

                channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += async (model, ea) =>
                {
                    _ = await serviceQueue.SendToOutputAsync(new ExchangeMessage(Encoding.UTF8.GetString(ea.Body)));
                };

                // This returns straight away, so don't dispose of the channel
                channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);

            } catch (Exception ex) {
                logger.Error(ex.StackTrace);
            }
        }

        public override async Task<ExchangeMessage> SendToOutputAsync(ExchangeMessage message) {

            await Task.Run(() => { });


            // Set the queueName bassed on the content of the message if configured
            message = SetDestinationFromMessage(message);
            if (!message.destinationSet && queueName == null) {
                message.sent = false;
                return message;
            }

            try {
                var factory = new ConnectionFactory() { HostName = connection };
                using (var conn = factory.CreateConnection()) {
                    using (var channel = conn.CreateModel()) {

                        try {
                            QueueDeclareOk ch = channel.QueueDeclare(queue: queueName,
                                                                     durable: true,
                                                                     exclusive: false,
                                                                     autoDelete: false,
                                                                     arguments: null);


                        } catch (Exception ex) {
                            logger.Error(ex.Message);
                            logger.Error(ex.StackTrace);
                            message.sent = false;
                            message.status = $"Error  Sending to Rabbit Topic {queueName}";
                            return message;
                        }
                        var body = Encoding.UTF8.GetBytes(message.payload);

                        try {
                            channel.BasicPublish(exchange: "",
                                                 routingKey: queueName,
                                                 basicProperties: null,
                                                 body: body);
                        } catch (Exception ex) {
                            logger.Error(ex.Message);
                            logger.Error(ex.StackTrace);
                            SendToUndeliverableQueue(message);
                            message.sent = false;
                            message.status = $"Error Sending to Rabbit Queue {queueName}";
                            return message;

                        }
                        message.sent = true;
                        message.status = $"Sent to Rabbit Queue {queueName}";
                        return message;

                    }
                }
            } catch (Exception e) {
                logger.Error(e.Message);
                message.sent = false;
                return message;
            }
        }

        public void Dispose() {
            try {
                serviceQueue.Dispose();
            } catch { }
        }
    }
}

