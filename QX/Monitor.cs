using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace QueueExchange
{
    public sealed class Monitor
    {
        private static readonly Monitor instance = new Monitor();
        private string tcpServerIP;
        private int tcpServerPort;
        private IMqttClientOptions options;
        private IMqttClient mqttClient;

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static Monitor()
        {
        }

        private Monitor()
        {
        }

        public static Monitor Instance {
            get {
                return instance;
            }
        }

        internal void setConfig(XElement monitorDefn)
        {
            try
            {
                tcpServerIP = monitorDefn.Attribute("host").Value;
            }
            catch (Exception)
            {
                tcpServerIP = null;
            }
            try
            {
                tcpServerPort = int.Parse(monitorDefn.Attribute("port").Value);
            }
            catch (Exception)
            {
                tcpServerIP = null;
            }

            if (tcpServerIP != null)
            {
                options = new MqttClientOptionsBuilder()
                    .WithTcpServer(tcpServerIP, tcpServerPort) // Port is optional
                    .Build();

                var factory = new MqttFactory();
                mqttClient = factory.CreateMqttClient();
                mqttClient.UseDisconnectedHandler(async e =>
                {
                    Console.WriteLine("### DISCONNECTED FROM SERVER ###");
                    await Task.Delay(TimeSpan.FromSeconds(5));

                    try
                    {
                        await mqttClient.ConnectAsync(options, CancellationToken.None); // Since 3.0.5 with CancellationToken
                    }
                    catch
                    {
                        Console.WriteLine("### RECONNECTING FAILED ###");
                    }
                });

                mqttClient.ConnectAsync(options, CancellationToken.None);
            }
        }

        public void Send(String message)
        {
            if (tcpServerIP == null)
            {
                return;
            }

            Task.Run(() => SendAsync(message));
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void SendAsync(String message)
        {
            var mqqtmessage = new MqttApplicationMessageBuilder()
                .WithTopic("QX Monitor")
                .WithPayload(message)
                .WithExactlyOnceQoS()
                .Build();

            if (mqttClient.IsConnected)
            {
                mqttClient.PublishAsync(mqqtmessage, CancellationToken.None);
            }
        }
    }
}
