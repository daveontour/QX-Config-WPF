using System.Xml;

namespace QXEditorModule.Common {


    public enum ContextMenuType {
        Cut,
        Copy,
        Paste,
        Delete,
        Add,
        AddPipe,
        AddLogger,
        AddMonitor,
        AddNamespace,
        AddInput,
        AddOutput,
        AddFilter,
        AddExpression,
        AddAltQueue,
        AddDataFilter,
        AddXPathExists,
        AddServiceSettings,
        AddMSMQInput,
        AddMQInput,
        AddFileInput,
        AddHTTPInput,
        AddKafkaInput,
        AddRabbitInput,
        AddTestInput,
        AddMSMQOutput,
        AddMQOutput,
        AddFileOutput,
        AddHTTPOutput,
        AddTCPOutput,
        AddTCPInput,
        AddHTTPRest,
        AddKafkaOutput,
        AddRabbitOutput,
        AddSINK,
        AddSMTPOutput,
        AddMQTTInput,
        AddMQTTOutput,
        AddFTPOutput
    }

    public interface IView {
        //  Canvas selectedCanvas { get; set; }

        void DrawQXConfig();
        void HightLightCanvas(XmlNode node);
        void UpdateParamBindings(string param);
        void MSMQIn(XmlNode node);
        void MSMQOut(XmlNode node);
        void FileInSource(XmlNode node);
        void FileOutSource(XmlNode node);
        void MQInSource(XmlNode node);
        void MQOutSource(XmlNode node);
        void UpdateSelectedNodeCanvas(XmlNode node);
        void UpdateSelectedPipeCanvas(XmlNode node);
        void ChangeElementType(string value);
        bool CanChangeElementType(string value);
        void ChangeFilterType(string value);
        void KafkaIn(XmlNode node);
        void KafkaOut(XmlNode node);
        void RestOut(XmlNode node);
        void HTTPOut(XmlNode node);
        void HTTPIn(XmlNode node);
        void TCPOUT(XmlNode node);
        void TCPIN(XmlNode node);
        void RabbitOut(XmlNode node);
        void RabbitIn(XmlNode node);
        void SinkOut(XmlNode node);
        void TestSource(XmlNode node);
        void RefreshDraw();
        void SMTPOut(XmlNode node);
        void MQTT(XmlNode node);
        void FTP(XmlNode node);
    }
}
