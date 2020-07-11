using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace QueueExchange
{
    class QETCPClient : QueueAbstract
    {

        private readonly string tcpServerIP;
        private readonly int tcpServerPort;
        private readonly bool closeConnection;
        //private AsynchronousSocketListener sockListner;

        public QETCPClient(XElement defn, IProgress<QueueMonitorMessage> monitorMessageProgress) : base(defn, monitorMessageProgress)
        {

            try
            {
                tcpServerIP = defn.Attribute("tcpServerIP").Value;
            }
            catch (Exception)
            {
                tcpServerIP = "127.0.0.1";
            }
            try
            {
                tcpServerPort = int.Parse(defn.Attribute("tcpServerPort").Value);
            }
            catch (Exception)
            {

            }

            try
            {
                closeConnection = bool.Parse(defn.Attribute("closeConnection").Value);
            }
            catch (Exception)
            {
                closeConnection = false;
            }
        }

        public override async Task StartListener()
        {
            await Task.Run(() =>
            {

            });
        }

        public override async Task<ExchangeMessage> SendToOutputAsync(ExchangeMessage message)
        {

            await Task.Run(() => { });

            try
            {


                TcpClient client = new TcpClient(tcpServerIP, tcpServerPort);

                // Translate the passed message into ASCII and store it as a Byte array.
                Byte[] data = System.Text.Encoding.UTF8.GetBytes(message.payload);

                // Get a client stream for reading and writing.

                NetworkStream stream = client.GetStream();

                // Send the message to the connected TcpServer. 
                stream.Write(data, 0, data.Length);

                // Close everything.
                stream.Close();
                client.Close();
            }
            catch (ArgumentNullException e)
            {
                logger.Error($"ArgumentNullException: {e.Message}");
                message.sent = false;
                return message;
            }
            catch (SocketException e)
            {
                logger.Error($"SocketException: {e.Message}");
                message.sent = false;
                return message;
            }

            message.sent = true;
            return message;


        }

        public override bool SetUp()
        {
            return true;
        }
    }
}
