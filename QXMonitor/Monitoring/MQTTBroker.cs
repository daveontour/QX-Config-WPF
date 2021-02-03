using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using MQTTnet;
using MQTTnet.Server;
using QXMonitor;
using System;
using System.Text;
using System.Threading;

namespace QueueExchange.Monitoring {
    class MQTTBroker {
        private readonly Thread _serverThread;

        public MQTTBroker() : this(GlobalHost.ConnectionManager.GetHubContext<QXMonitorHub>().Clients) {

        }
        private IHubConnectionContext<dynamic> Clients {
            get;
            set;
        }

        public MQTTBroker(IHubConnectionContext<dynamic> clients) {
            Clients = clients;
            _serverThread = new Thread(this.StartListening);
            _serverThread.Start();
        }

        public void StartListening() {
            MqttServerOptionsBuilder optionsBuilder = new MqttServerOptionsBuilder()
                    .WithConnectionBacklog(100)
                    .WithDefaultEndpointPort(5656)
                    .WithApplicationMessageInterceptor(context =>
                    {
                        Console.WriteLine(Encoding.UTF8.GetString(context.ApplicationMessage.Payload, 0, context.ApplicationMessage.Payload.Length));
                        Clients.All.notifyMessage(context.ApplicationMessage.Payload, 0, context.ApplicationMessage.Payload.Length);
                        context.ApplicationMessage.Payload = null;
                    });
            IMqttServer mqttServer = new MqttFactory().CreateMqttServer();
            mqttServer.StartAsync(optionsBuilder.Build());
            Console.WriteLine("Press any key to exit.");
            Console.ReadLine();


        }
    }
}
