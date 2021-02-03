using System.Collections.Generic;
using System.Windows.Controls;

namespace QXEditorModule.Common {

    public class ContextMenuProvider {
        public readonly Dictionary<ContextMenuType, MenuItem> ContextMenus = new Dictionary<ContextMenuType, MenuItem>();

        public ContextMenuProvider() {
            ContextMenus.Add(ContextMenuType.Copy, new MenuItem { Header = "Copy" });
            ContextMenus.Add(ContextMenuType.Paste, new MenuItem { Header = "Paste" });
            ContextMenus.Add(ContextMenuType.Delete, new MenuItem { Header = "Delete" });
            ContextMenus.Add(ContextMenuType.AddPipe, new MenuItem { Header = "Add Pipe" });
            ContextMenus.Add(ContextMenuType.Add, new MenuItem { Header = "Add" });

            ContextMenus.Add(ContextMenuType.AddMSMQInput, new MenuItem { Header = "MS MQ Input Node" });
            ContextMenus.Add(ContextMenuType.AddMQInput, new MenuItem { Header = "IBM MQ Input Node" });
            ContextMenus.Add(ContextMenuType.AddFileInput, new MenuItem { Header = "File Input Node" });
            ContextMenus.Add(ContextMenuType.AddHTTPInput, new MenuItem { Header = "HTTP POST Input Node" });
            ContextMenus.Add(ContextMenuType.AddKafkaInput, new MenuItem { Header = "Kafka Topic Input Node" });
            ContextMenus.Add(ContextMenuType.AddRabbitInput, new MenuItem { Header = "Rabbit MQ Input Node" });
            ContextMenus.Add(ContextMenuType.AddMQTTInput, new MenuItem { Header = "MQTT Subscriber Node" });
            ContextMenus.Add(ContextMenuType.AddTCPInput, new MenuItem { Header = "TCP Server Input Node" });
            ContextMenus.Add(ContextMenuType.AddTestInput, new MenuItem { Header = "Test Message Source" });

            ContextMenus.Add(ContextMenuType.AddMSMQOutput, new MenuItem { Header = "MS MQ Output Node" });
            ContextMenus.Add(ContextMenuType.AddMQOutput, new MenuItem { Header = "IBM MQ Output Node" });
            ContextMenus.Add(ContextMenuType.AddFileOutput, new MenuItem { Header = "File Output Node" });
            ContextMenus.Add(ContextMenuType.AddHTTPOutput, new MenuItem { Header = "HTTP POST Output Node" });
            ContextMenus.Add(ContextMenuType.AddKafkaOutput, new MenuItem { Header = "Kafka Topic Output Node" });
            ContextMenus.Add(ContextMenuType.AddRabbitOutput, new MenuItem { Header = "Rabbit MQ Output Node" });
            ContextMenus.Add(ContextMenuType.AddMQTTOutput, new MenuItem { Header = "MQTT Publisher Node" });
            ContextMenus.Add(ContextMenuType.AddFTPOutput, new MenuItem { Header = "FTP Node" });
            ContextMenus.Add(ContextMenuType.AddSMTPOutput, new MenuItem { Header = "SMTP Output Node" });
            ContextMenus.Add(ContextMenuType.AddTCPOutput, new MenuItem { Header = "TCP Client Output Node" });
            ContextMenus.Add(ContextMenuType.AddHTTPRest, new MenuItem { Header = "Rest Server Retrieval Output Node" });
            ContextMenus.Add(ContextMenuType.AddSINK, new MenuItem { Header = "Message Sink" });


            ContextMenus.Add(ContextMenuType.AddInput, new MenuItem { Header = "Add Input Node" });
            ContextMenus.Add(ContextMenuType.AddOutput, new MenuItem { Header = "Add Output Node" });
            ContextMenus.Add(ContextMenuType.AddFilter, new MenuItem { Header = "Add Filter" });
            ContextMenus.Add(ContextMenuType.AddExpression, new MenuItem { Header = "Add Boolean Expression" });
            ContextMenus.Add(ContextMenuType.AddMonitor, new MenuItem { Header = "Add Monitor Queue" });
            ContextMenus.Add(ContextMenuType.AddLogger, new MenuItem { Header = "Add Logger Queue" });
            ContextMenus.Add(ContextMenuType.AddNamespace, new MenuItem { Header = "Add Namespace" });
            ContextMenus.Add(ContextMenuType.AddServiceSettings, new MenuItem { Header = "Add Service Settings" });
            ContextMenus.Add(ContextMenuType.AddAltQueue, new MenuItem { Header = "Add Alternative Queue" });
            ContextMenus.Add(ContextMenuType.AddDataFilter, new MenuItem { Header = "Add Data Filter" });
            ContextMenus.Add(ContextMenuType.AddXPathExists, new MenuItem { Header = "Add XPath Exists Filter" });

        }
    }
}
