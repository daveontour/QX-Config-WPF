using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Client.Receiving;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MQTT_TEST {
    class Program {

        static IMqttClient mqttClient;
        static void Main(string[] args) {
            SendToOutput("Dave WAS HERE");
        }

        public static void SendToOutput(string message) {

            // Create a new MQTT client.
            var factory = new MqttFactory();
            mqttClient = factory.CreateMqttClient();
            var msg = new MqttApplicationMessageBuilder()
                        .WithTopic("testtopic/1")
                        .WithPayload(message)
                        .WithExactlyOnceQoS()
                        .WithRetainFlag()
                        .Build();

            var options = new MqttClientOptionsBuilder().WithTcpServer("broker.hivemq.com", 1883).Build();
            mqttClient.UseDisconnectedHandler(new MqttClientDisconnectedHandlerDelegate(e => MqttClient_Disconnected(e)));
            mqttClient.UseConnectedHandler(new MqttClientConnectedHandlerDelegate(e => MqttClient_Connected(e)));
            mqttClient.UseApplicationMessageReceivedHandler(new MqttApplicationMessageReceivedHandlerDelegate(e => MqttClient_ApplicationMessageReceived(e)));
            mqttClient.ConnectAsync(options).Wait();

            //try {
            //    var options = new MqttClientOptionsBuilder().WithTcpServer("broker.hivemq.com", 1883).Build();
            //    MQTTnet.Client.Connecting.MqttClientAuthenticateResult conn = mqttClient.ConnectAsync(options, CancellationToken.None).Result;
            //    mqttClient.PublishAsync(msg, CancellationToken.None);

            //    mqttClient.UseConnectedHandler(async e =>
            //    {
            //        Console.WriteLine("### CONNECTED WITH SERVER ###");

            //        // Subscribe to a topic
            //        await mqttClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic("testtopic/1").Build());

            //        Console.WriteLine("### SUBSCRIBED ###");
            //    });
            //} catch (Exception ex) {
            //    Console.WriteLine(ex.Message);
            //}

            Console.ReadLine();
        }

        private static void MqttClient_ApplicationMessageReceived(MqttApplicationMessageReceivedEventArgs e) {
            Console.WriteLine($"ClientId:{e.ClientId} Topic:{e.ApplicationMessage.Topic} Payload:{e.ApplicationMessage.ConvertPayloadToString()}");
        }

        private static async void MqttClient_Disconnected(MqttClientDisconnectedEventArgs e) {
            Debug.WriteLine("Disconnected");
            await Task.Delay(TimeSpan.FromSeconds(5));

            //try {
            //    await mqttClient.ConnectAsync(mqttOptions);
            //} catch (Exception ex) {
            //    Debug.WriteLine("Reconnect failed {0}", ex.Message);
            //}
        }

        private static async void MqttClient_Connected(MqttClientConnectedEventArgs e) {
            Debug.WriteLine("Cconnected");
            await mqttClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic("testtopic/1").Build());
        }
    }
}
