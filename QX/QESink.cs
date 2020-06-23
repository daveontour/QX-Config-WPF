
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace QueueExchange
{
    class QESink : QueueAbstract
    {

        /*
         * It's called SINK, but the factory class will also instantiate it for "TESTSOURCE" input
         */

        private readonly string fullPath;
        private readonly int maxWait;
        private readonly int maxMessages;
        private readonly int fixedInterval;
        private int counter = 0;
        private int sent = 0;
        private readonly Queue<ExchangeMessage> _queue = new Queue<ExchangeMessage>();
        private readonly System.Timers.Timer _timer;

        public QESink(XElement defn, IProgress<QueueMonitorMessage> monitorMessageProgress) : base(defn, monitorMessageProgress)
        {

            try
            {
                fullPath = definition.Attribute("path").Value;
            }
            catch (Exception)
            {
                fullPath = null;
            }

            try
            {
                maxWait = Int32.Parse(definition.Attribute("maxWait").Value);
            }
            catch (Exception)
            {
                maxWait = 60000;
            }

            try
            {
                fixedInterval = Int32.Parse(definition.Attribute("fixedInterval").Value);
            }
            catch (Exception)
            {
                fixedInterval = -1;
            }

            try
            {
                maxMessages = Int32.Parse(definition.Attribute("maxMessages").Value);
            }
            catch (Exception)
            {
                maxMessages = -1;
            }

            if (definition.Name == "input")
            {

                _timer = new System.Timers.Timer
                {
                    AutoReset = false,
                    Enabled = true
                };
                _timer.Elapsed += (source, eventArgs) =>
                {
                    ExchangeMessage xm;
                    try
                    {
                        // If a file has been specified, send a file
                        string message = File.ReadAllText(fullPath, Encoding.UTF8);
                        xm = new ExchangeMessage(message);
                    }
                    catch (Exception)
                    {
                        xm = new ExchangeMessage($"<test>data</test>");
                    }

                    _queue.Enqueue(xm);

                    if (fixedInterval > 0)
                    {
                        _timer.Interval = fixedInterval;
                    }
                    else
                    {
                        _timer.Interval = new Random().Next(maxWait);
                    }

                    try
                    {

                        if (counter >= maxMessages && maxMessages > 0)
                        {
                            _timer.Enabled = false;
                            _timer.Stop();
                            _timer.Close();
                            _timer.Dispose();
                        }
                        else
                        {
                            _timer.Start();
                            _timer.Enabled = true;
                        }

                    }
                    catch (Exception e)
                    {
                        logger.Trace(e);
                    }

                    counter++;

                };

                if (fixedInterval > 0)
                {
                    _timer.Interval = fixedInterval;
                }
                else
                {
                    _timer.Interval = new Random().Next(maxWait);
                }

                _timer.Start();
            }
        }
        public override ExchangeMessage Listen(bool immediateReturn, int priorityWait)
        {

            if (immediateReturn)
            {
                if (_queue.Count == 0)
                {
                    return null;
                }
                else
                {
                    return _queue.Dequeue();
                }
            }
            else
            {
                try
                {
                    if (sent > maxMessages && maxMessages > 0)
                    {
                        Thread.Sleep(Int32.MaxValue);
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    logger.Trace(ex);
                }

                while (_queue.Count == 0)
                {
                    Thread.Sleep(500);
                }
                sent++;

                return _queue.Dequeue();
            }
        }

        public override async Task<ExchangeMessage> SendToOutputAsync(ExchangeMessage message)
        {

            // Do nothing with the message, just dump it.

            logger.Trace($"Message sunk at {name}");
            message.sent = true;
            await Task.Run(() => { });
            return null;
        }

        public override bool SetUp()
        {
            return true;
        }
    }
}
