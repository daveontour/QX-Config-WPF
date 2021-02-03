using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Client.Publishing;
using MQTTnet.Client.Receiving;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace QueueExchange {
    public class QEMQTT : QueueAbstract, IDisposable {

        private readonly string topic;
        private readonly string mqttServer;
        private readonly string mqttServerURL;
        private readonly string serverType;
        private readonly int mqttServerPort;

        private IMqttClientOptions options;
        private readonly IMqttClient mqttClient;

        public QEMQTT(XElement defn, IProgress<QueueMonitorMessage> monitorMessageProgress) : base(defn, monitorMessageProgress) {
            var factory = new MqttFactory();
            mqttClient = factory.CreateMqttClient();
            try {
                mqttServer = definition.Attribute("mqttServer").Value;
            } catch (Exception) {
                mqttServer = null;
            }
            try {
                mqttServerURL = definition.Attribute("mqttServerURL").Value;
            } catch (Exception) {
                mqttServerURL = null;
            }
            try {
                mqttServerPort = int.Parse(definition.Attribute("mqttPort").Value);
            } catch (Exception) {
                mqttServerPort = -1;
            }
            try {
                topic = definition.Attribute("mqttTopic").Value;
            } catch (Exception) {
                topic = null;
            }
            try {
                serverType = definition.Attribute("mqttServerType").Value;
            } catch (Exception) {
                topic = null;
            }
        }


        public override bool SetUp() {


            if (serverType == "ws" && mqttServerURL == null) {
                return false;
            }
            if (serverType == "tcp" && (mqttServer == null || mqttServerPort == -1)) {
                return false;
            }

            return true;
        }

        public void Dispose() {
            throw new NotImplementedException();
        }

        public override async Task<ExchangeMessage> SendToOutputAsync(ExchangeMessage message) {

            // Create a new MQTT client.

            var msg = new MqttApplicationMessageBuilder()
                        .WithTopic(topic)
                        .WithPayload(message.payload)
                        .WithExactlyOnceQoS()
                        .WithRetainFlag()
                        .Build();

            //"broker.hivemq.com", 1883)
            if (serverType == "tcp") {
                options = new MqttClientOptionsBuilder().WithTcpServer(mqttServer, mqttServerPort).Build();
            }

            //"broker.hivemq.com:8000/mqtt"
            if (serverType == "ws") {
                options = new MqttClientOptionsBuilder().WithWebSocketServer(mqttServerURL).Build();
            }
            if (!mqttClient.IsConnected) {
                mqttClient.ConnectAsync(options).Wait();
            }

            MqttClientPublishResult result = await mqttClient.PublishAsync(msg, CancellationToken.None);

            if (result.ReasonCode == MqttClientPublishReasonCode.Success) {
                message.sent = true;
                message.status = $"Sent to MQTT topic {queueName}";
                return message;
            } else {
                message.sent = true;
                message.status = $"MQTT Send Failure {result.ReasonString}";
                logger.Error($"MQTT Send Failure {result.ReasonString}");
                return message;
            }
        }
        override async public Task StartListener() {

            logger.Info("Starting MQTT Listener");
            await Task.Run(() => { });

            if (serverType == "tcp") {
                options = new MqttClientOptionsBuilder().WithTcpServer(mqttServer, mqttServerPort).Build();
            }

            if (serverType == "ws") {
                options = new MqttClientOptionsBuilder().WithWebSocketServer(mqttServerURL).Build();
            }
            mqttClient.UseDisconnectedHandler(new MqttClientDisconnectedHandlerDelegate(e => MqttClient_Disconnected(e)));
            mqttClient.UseConnectedHandler(new MqttClientConnectedHandlerDelegate(e => MqttClient_Connected(e)));
            mqttClient.UseApplicationMessageReceivedHandler(new MqttApplicationMessageReceivedHandlerDelegate(e => MqttClient_ApplicationMessageReceived(e)));
            mqttClient.ConnectAsync(options).Wait();

        }
        private void MqttClient_ApplicationMessageReceived(MqttApplicationMessageReceivedEventArgs e) {

            string message = e.ApplicationMessage.ConvertPayloadToString();
            string topic = e.ApplicationMessage.Topic;

            logger.Trace($"MQTT Message Received on {name}. Topic:{topic}");
            SendToPipe(message);
        }

        private async void MqttClient_Disconnected(MqttClientDisconnectedEventArgs e) {
            logger.Warn("MQTT Disconnected");
            await Task.Delay(TimeSpan.FromSeconds(5));

            try {
                await mqttClient.ConnectAsync(options);
            } catch (Exception ex) {
                logger.Warn("Reconnect failed {0}", ex.Message);
            }
        }

        private async void MqttClient_Connected(MqttClientConnectedEventArgs e) {
            logger.Info("MQTT Connected");
            await mqttClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic(topic).Build());
        }
    }
}
