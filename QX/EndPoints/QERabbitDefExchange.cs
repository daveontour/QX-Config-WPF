using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace QueueExchange {
    class QERabbitDefExchange : QueueAbstract, IDisposable {
        private int port;
        private string vhost;
        private string pass;
        private string user;

        public QERabbitDefExchange(XElement defn, IProgress<QueueMonitorMessage> monitorMessageProgress) : base(defn, monitorMessageProgress) { }

        public override bool SetUp() {
            OK_TO_RUN = false;

            try {
                connection = definition.Attribute("connection").Value;
            } catch (Exception) {
                logger.Error("Connection not defined for RabbitMQ");
                return false;
            }


            try {
                connection = definition.Attribute("connection").Value;
            } catch (Exception) {
                return false;
            }
            try {
                queueName = definition.Attribute("queueName").Value;
            } catch (Exception) {
                queueName = null;
            }

            try {
                user = definition.Attribute("rabbitUser").Value;
            } catch (Exception) {
                user = "guest";
            }
            try {
                pass = definition.Attribute("rabbitPass").Value;
            } catch (Exception) {
                pass = "guest";
            }
            try {
                vhost = definition.Attribute("rabbitVHost").Value;
            } catch (Exception) {
                vhost = "/";
            }
            try {
                port = int.Parse(definition.Attribute("rabbitPort").Value);
            } catch (Exception) {
                port = 5672;
            }


            OK_TO_RUN = true;

            return true;
        }

        override async public Task StartListener() {

            await Task.Run(() =>
            {
                try {
                    var factory = new ConnectionFactory() { HostName = connection };
                    factory.UserName = user;
                    factory.Password = pass;
                    factory.VirtualHost = vhost;
                    factory.HostName = connection;
                    factory.Port = port;

                    var conn = factory.CreateConnection();
                    var channel = conn.CreateModel();

                    channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += (model, ea) =>
                    {
                        SendToPipe(Encoding.UTF8.GetString(ea.Body));
                    };

                    // This returns straight away, so don't dispose of the channel
                    channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);

                } catch (Exception ex) {
                    logger.Error(ex.StackTrace);
                }
            });
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
                factory.UserName = user;
                factory.Password = pass;
                factory.VirtualHost = vhost;
                factory.HostName = connection;
                factory.Port = port;
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

        }
    }
}

