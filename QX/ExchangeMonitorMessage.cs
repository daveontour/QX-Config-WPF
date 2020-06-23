namespace QueueExchange
{
    //public class ExchangeMonitorMessage {
    //    /*
    //     * For passing the message around inside the system
    //     */

    //    public string _monMessageID;
    //    public string _messageID;
    //    public string _queueID;
    //    public string _pipeID;
    //    public string _action;
    //    public string _timestamp;
    //    public string _message;
    //    public string _pipeName;
    //    public string _queueName;

    //    static readonly string fullPattern = DateTimeFormatInfo.CurrentInfo.FullDateTimePattern;

    //    public ExchangeMonitorMessage(string message) {
    //        _monMessageID = Guid.NewGuid().ToString();
    //        _message = message;
    //        _timestamp = DateTime.Now.ToString(fullPattern);


    //        DateTimeOffset dateOffsetValue = DateTime.Now;
    //        _timestamp = dateOffsetValue.ToString("dd/MM/yyyy hh:mm:ss.fff tt");
    //    }

    //    public ExchangeMonitorMessage(string messageID, string queueID, string queueName, string pipeID, string pipeName, string action, string message) {
    //        _monMessageID = Guid.NewGuid().ToString();
    //        _messageID = messageID;
    //        _queueID = queueID;
    //        _queueName = queueName;
    //        _pipeID = pipeID;
    //        _pipeName = pipeName;
    //        _action = action;
    //        _message = message;
    //        DateTimeOffset dateOffsetValue = DateTime.Now;
    //        _timestamp = dateOffsetValue.ToString("dd/MM/yyyy hh:mm:ss.fff tt");

    //    }

    //    public override string ToString() {
    //        return $"<QXMonitorMessage>\n<timestamp>{_timestamp}<timestamp>\n<uuid>{_monMessageID}</uuid>\n<messageID>{_messageID}</messageID>\n<queueID>{_queueID}</queueID>\n<queueName>{_queueName}</queueName>\n<pipeID>{_pipeID}</pipeID>\n<pipeName>{_pipeName}</pipeName>\n<action>{_action}</action>\n<message>{_message}</message>\n</QXMonitorMessage>";
    //    }
    //    public string ToJSONString() {
    //        return $"{{\n\"QXMonitorMessage\":{{\"timestamp\":\"{_timestamp}\", \"uuid\":\"{_monMessageID}\", \"messageID\":\"{_messageID}\", \"queueID\":\"{_queueID}\", \"queueName\":\"{_queueName}\", \"pipeID\":\"{_pipeID}\",  \"pipeName\":\"{_pipeName}\",\"action\":\"{_action}\", \"message\":\"{_message}\"\n }} \n}}";
    //    }
    //}
}
