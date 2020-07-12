using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace QueueExchange {

    public class StateObject {
        // Client  socket.  
        public Socket workSocket = null;
        // Size of receive buffer.  
        public const int BufferSize = 102400;
        // Receive buffer.  
        public byte[] buffer = new byte[BufferSize];
        // Received data string.  
        public StringBuilder sb = new StringBuilder();
    }
    class QETCP : QueueAbstract {

        private readonly string tcpServerIP;
        private readonly int tcpServerPort;
        private readonly string eof;
        public ManualResetEvent allDone = new ManualResetEvent(false);
        private readonly bool eofRemove = true;
        private bool continueToListen = true;

        public QETCP(XElement defn, IProgress<QueueMonitorMessage> monitorMessageProgress) : base(defn, monitorMessageProgress) {

            try {
                tcpServerIP = defn.Attribute("tcpServerIP").Value;
            }
            catch (Exception) {
                tcpServerIP = "127.0.0.1";
            }
            try {
                tcpServerPort = int.Parse(defn.Attribute("tcpServerPort").Value);
            }
            catch (Exception) {

            }

            try {
                eof = defn.Attribute("eof").Value;
            }
            catch (Exception) {
                eof = "<EOF>";
            }

            try {
                eofRemove = bool.Parse(defn.Attribute("eofRemove").Value);
            }
            catch (Exception) {
                eofRemove = true;
            }
        }

        public override async Task StartListener() {
            await Task.Run(() =>
            {

                try {

                    IPAddress ipAddress = IPAddress.Parse(tcpServerIP);
                    logger.Info("\n*****************************************");
                    logger.Info($"*   TCP Server Listen on {ipAddress}:{tcpServerPort}");
                    logger.Info("*****************************************\n");


                    IPEndPoint localEndPoint = new IPEndPoint(ipAddress, tcpServerPort);

                    // Create a TCP/IP socket.  
                    using (Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp)) {

                        // Bind the socket to the local endpoint and listen for incoming connections.  
                        try {
                            listener.Bind(localEndPoint);
                            listener.Listen(100);

                            while (continueToListen) {

                                // Set the event to nonsignaled state.  
                                allDone.Reset();

                                // Start an asynchronous socket to listen for connections.  
                                logger.Info("TCP Server Waiting for a connection");
                                listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);

                                // Wait until a connection is made before continuing.  
                                allDone.WaitOne();
                            }

                        }
                        catch (Exception e) {
                            logger.Error(e.Message);
                        }
                    }
                }
                catch (Exception ex) {
                    logger.Error(ex);
                }

            });
        }

        public void AcceptCallback(IAsyncResult ar) {
            // Signal the main thread to continue.  
            allDone.Set();

            logger.Info("TCP Server Accepted a connection");
            // Get the socket that handles the client request.  

            try {
                Socket listener = (Socket)ar.AsyncState;
                Socket handler = listener.EndAccept(ar);

                // Create the state object.  
                StateObject state = new StateObject {
                    workSocket = handler
                };
                handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReadCallback), state);
            }
            catch (Exception) { }

        }

        public void ReadCallback(IAsyncResult ar) {
            String content = String.Empty;

            // Retrieve the state object and the handler socket  
            // from the asynchronous state object.  
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;

            // Read data from the client socket.
            int bytesRead = handler.EndReceive(ar);

            if (bytesRead > 0) {
                // There  might be more data, so store the data received so far.  
                state.sb.Append(Encoding.ASCII.GetString(
                    state.buffer, 0, bytesRead));

                // Check for end-of-file tag. If it is not there, read
                // more data.  
                content = state.sb.ToString();
                if (content.IndexOf(eof) > -1) {
                    // All the data has been read from the
                    // client. Display it on the console.  
                    logger.Trace("Read {0} bytes from socket. \n Data : {1}", content.Length, content);
                    if (eofRemove) {
                        content = content.Substring(0, content.IndexOf(eof));
                    }
                    SendToPipe(content);
                }
                else {
                    // Not all data received. Get more.  
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReadCallback), state);
                }
            }
        }


        public override async Task<ExchangeMessage> SendToOutputAsync(ExchangeMessage message) {

            await Task.Run(() => { });

            try {


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
            catch (ArgumentNullException e) {
                logger.Error($"ArgumentNullException: {e.Message}");
                message.sent = false;
                return message;
            }
            catch (SocketException e) {
                logger.Error($"SocketException: {e.Message}");
                message.sent = false;
                return message;
            }

            message.sent = true;
            return message;
        }

        public override bool SetUp() {
            return true;
        }

        override public void Stop() {
            continueToListen = false;
            allDone.Set();
        }
    }
}
