using System;

namespace QueueExchange
{
    public class ExchangeMonitorMessage
    {
        public string topic;
        public string message;
        public string type;
        public string _monMessageID;
        public string _timestamp;
        public string xid;

        public PipelineMonitorMessage pipeMessage = new PipelineMonitorMessage();

        public ExchangeMonitorMessage(string topic, string message, string type)
        {
            _monMessageID = Guid.NewGuid().ToString();
            DateTimeOffset dateOffsetValue = DateTime.Now;
            _timestamp = dateOffsetValue.ToString("dd/MM/yyyy hh:mm:ss.fff tt");

            this.type = type;
            this.topic = topic;
            this.message = message;
        }
        public ExchangeMonitorMessage(PipelineMonitorMessage pipeMsg)
        {
            _monMessageID = Guid.NewGuid().ToString();
            DateTimeOffset dateOffsetValue = DateTime.Now;
            _timestamp = dateOffsetValue.ToString("dd/MM/yyyy hh:mm:ss.fff tt");
            this.pipeMessage = pipeMsg;
        }
        public override string ToString()
        {
            if (pipeMessage == null)
            {
                pipeMessage = new PipelineMonitorMessage();
            }
            return $"{_timestamp},{_monMessageID},{xid},{type},{topic},{message},{pipeMessage?.ToString()}";
        }


    }
    public class PipelineMonitorMessage
    {

        public string pipeID;
        public string pipeName;
        public string pipetopic;
        public string pipemessage;
        public string pipemessageType;

        private QueueMonitorMessage queueMessage = new QueueMonitorMessage();

        public PipelineMonitorMessage(string id, string name, string topic, string message, string messageType)
        {
            this.pipeID = id;
            this.pipeName = name;
            this.pipetopic = topic;
            this.pipemessage = message;
            this.pipemessageType = messageType;
        }
        public PipelineMonitorMessage(QueueMonitorMessage queueMessage)
        {
            this.queueMessage = queueMessage;
        }

        public PipelineMonitorMessage()
        {
        }

        public override string ToString()
        {
            if (queueMessage == null)
            {
                queueMessage = new QueueMonitorMessage();
            }
            return $"{pipeID},{pipeName},{pipemessageType},{pipetopic},{pipemessage},{queueMessage.ToString()}";
        }
    }

    public class QueueMonitorMessage
    {
        public string messageUUID;
        public string nodeName;
        public string nodeID;
        public string topic;
        public string message;
        public string messageType;

        public QueueMonitorMessage()
        {
        }

        public QueueMonitorMessage(string nodeID, string nodeName, string messageUUID, string topic, string message, string messageType)
        {
            this.messageUUID = messageUUID;
            this.nodeName = nodeName;
            this.nodeID = nodeID;
            this.topic = topic;
            this.message = message;
            this.messageType = messageType;
        }

        public override string ToString()
        {
            return $"{nodeID},{nodeName},{messageUUID},{messageType},{topic},{messageType}";
        }
    }
}