using System;

namespace QueueExchange
{
    public class ExchangeMessage
    {
        /*
         * For passing the message around inside the system
         */

        public string uuid;
        public string payload;
        public bool transformed = false;
        public string status;
        public bool pass = true;
        public bool sent = false;
        public bool destinationSet = false;
        public string destination;
        public string time;
        public bool enqueued = false;
        public ExchangeMessage(string message)
        {
            uuid = Guid.NewGuid().ToString();
            payload = message;
        }
        public ExchangeMessage(ExchangeMonitorMessage monMessage)
        {
            payload = monMessage.ToString();
        }

        public override string ToString()
        {
            return $"<ExchangeMessage>\n<time>{time}<time>\n<uuid>{uuid}</uuid>\n<status>{status}</status>\n<transformed>{transformed}</transformed>\n<pass>{pass}</pass>\n<sent>{sent}</sent>\n<destination>{destination}<destination>\n<payload>{payload}</payload>\n</ExchangeMessage>";
        }
    }
}
