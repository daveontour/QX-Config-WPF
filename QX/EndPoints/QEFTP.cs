using FluentFTP;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace QueueExchange {
    class QEFTP : QueueAbstract {


        private string ftpURL = null;
        private string directory = null;
        private bool useFTPS = false;
        private readonly bool OK_TO_SEND = true;

        public QEFTP(XElement defn, IProgress<QueueMonitorMessage> monitorMessageProgress) : base(defn, monitorMessageProgress) {

            try {
                ftpURL = definition.Attribute("ftpURL").Value;
            } catch (Exception) {
                logger.Error($"FTP not defined for {definition.Attribute("name").Value}");
                OK_TO_SEND = false;
            }



            try {
                directory = definition.Attribute("directory").Value;
            } catch (Exception) {
                directory = null;
            }

            try {
                useFTPS = bool.Parse(definition.Attribute("useFTPS").Value);
            } catch (Exception) {
                useFTPS = false;
            }
        }

        public override async Task StartListener() {
            await Task.Run(() =>
            {
            });
        }


        public override async Task<ExchangeMessage> SendToOutputAsync(ExchangeMessage message) {


            await Task.Run(() =>
            {
                {

                    if (!OK_TO_SEND) {
                        logger.Warn("FTP Interface not configured correctly");
                        return;
                    }

                    string guid = Guid.NewGuid().ToString();

                    if (!ftpURL.EndsWith("/")) {
                        ftpURL = ftpURL + "/";
                    }

                    if (!directory.StartsWith("/")) {
                        directory = "/" + directory;
                    }
                    if (!directory.EndsWith("/")) {
                        directory = directory + "/";
                    }
                    using (var ftpClient = new FtpClient(new Uri(ftpURL))) {
                        try {
                            if (useFTPS) {
                                ftpClient.EncryptionMode = FtpEncryptionMode.Explicit;
                                ftpClient.ValidateAnyCertificate = true;
                                ftpClient.Connect();
                            }

                            ftpClient.Upload(Encoding.UTF8.GetBytes(message.payload), directory + guid);
                        } catch (Exception ex) {
                            logger.Error(ex.Message);
                        }
                    }
                }
            });
            return null;
        }

        public override bool SetUp() {
            return true;
        }
    }
}
