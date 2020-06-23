using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Owin;
using QueueExchange;
using QueueExchange.Monitoring;

[assembly: OwinStartupAttribute(typeof(QXMonitor.Startup))]

namespace QXMonitor
{
    class Program
    {
        static void Main(string[] args)
        {
            SimpleHTTPServer httpListener = new SimpleHTTPServer("C:\\Users\\dave_\\Desktop\\", 5555);
            MQTTBroker mqttBroker = new MQTTBroker(GlobalHost.ConnectionManager.GetHubContext<QXMonitorHub>().Clients);
        }
    }
    class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseCors(CorsOptions.AllowAll);
            app.MapSignalR();
        }
    }
    public class QXMonitorHub : Hub
    {
        public void Send(string name, string message)
        {
            Clients.All.addMessage(name, message);
        }
    }
}
