
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace QueueExchange {
    class QESink : QueueAbstract {

        /*
         * It's called SINK, but the factory class will also instantiate it for "TESTSOURCE" input
         */

        private readonly string fullPath;
        private readonly int maxWait;
        private readonly int maxMessages;
        private readonly int fixedInterval;
        private int counter = 0;
        private readonly string testText;

        private System.Timers.Timer _timer;

        public QESink(XElement defn, IProgress<QueueMonitorMessage> monitorMessageProgress) : base(defn, monitorMessageProgress) {

            try {
                fullPath = definition.Attribute("path").Value;
            } catch (Exception) {
                fullPath = null;
            }

            try {
                testText = definition.Attribute("testText").Value;
            } catch (Exception) {
                testText = null;
            }

            try {
                maxWait = Int32.Parse(definition.Attribute("maxWait").Value);
            } catch (Exception) {
                maxWait = 60000;
            }

            try {
                fixedInterval = Int32.Parse(definition.Attribute("fixedInterval").Value);
            } catch (Exception) {
                fixedInterval = -1;
            }

            try {
                maxMessages = Int32.Parse(definition.Attribute("maxMessages").Value);
            } catch (Exception) {
                maxMessages = -1;
            }
        }

        public override async Task StartListener() {
            await Task.Run(() =>
            {

                _timer = new System.Timers.Timer {
                    AutoReset = false,
                    Enabled = true
                };
                _timer.Elapsed += (source, eventArgs) =>
                {
                    string message;
                    try {
                        // If a file has been specified, send a file
                        message = File.ReadAllText(fullPath, Encoding.UTF8);
                    } catch (Exception) {
                        message = testText;
                    }

                    if (message == null) {
                        message = Guid.NewGuid().ToString();
                    }

                    SendToPipe(message);

                    if (fixedInterval > 0) {
                        _timer.Interval = fixedInterval;
                    } else {
                        _timer.Interval = new Random().Next(maxWait);
                    }

                    try {

                        if (counter >= maxMessages && maxMessages > 0) {
                            _timer.Enabled = false;
                            _timer.Stop();
                            _timer.Close();
                            _timer.Dispose();
                        } else {
                            _timer.Start();
                            _timer.Enabled = true;
                        }

                    } catch (Exception e) {
                        logger.Trace(e);
                    }

                    counter++;

                };

                if (fixedInterval > 0) {
                    _timer.Interval = fixedInterval;
                } else {
                    _timer.Interval = new Random().Next(maxWait);
                }

                _timer.Start();
            });
        }


        public override async Task<ExchangeMessage> SendToOutputAsync(ExchangeMessage message) {
            await Task.Run(() =>
            {
                logger.Info($"Sunk Message");
                message.sent = true;
            });
            return null;
        }

        public override bool SetUp() {
            return true;
        }
    }
}
