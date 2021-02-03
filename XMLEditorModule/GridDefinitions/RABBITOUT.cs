using QXEditorModule.Common;
using System.ComponentModel;
using System.Xml;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace QXEditorModule.GridDefinitions {

    [DisplayName("Rabbit MQ Output Node")]
    public class RABBITOUT : MyNodePropertyGrid {

        public RABBITOUT(XmlNode dataModel, IView view) {
            this._node = dataModel;
            this.view = view;
            this.type = "Rabbit MQ";
        }

        [CategoryAttribute("Required"), DisplayName("Node Type"), Browsable(true), PropertyOrder(2), DescriptionAttribute("Type of the endpoint node"), ItemsSource(typeof(NodeTypeListOut))]
        public string NodeType {
            get { return "Rabbit MQ"; }
            set { SetType(value); }
        }

        [CategoryAttribute("Required"), DisplayName("Rabbit Server"), ReadOnly(false), Browsable(true), PropertyOrder(2), DescriptionAttribute("The Rabbit MQ Server")]
        public string RabbitServer {
            get { return GetAttribute("connection"); }
            set { SetAttribute("connection", value); }
        }
        [CategoryAttribute("Required"), DisplayName("Rabbit Server Port"), ReadOnly(false), Browsable(true), PropertyOrder(3), DescriptionAttribute("The Rabbit MQ Server Port")]
        public int RabbitServerPort {
            get { return GetIntAttribute("rabbitPort"); }
            set { SetAttribute("rabbitPort", value); }
        }
        [CategoryAttribute("Required"), DisplayName("Rabbit User"), ReadOnly(false), Browsable(true), PropertyOrder(4), DescriptionAttribute("The Rabbit MQ Username")]
        public string RabbitUser {
            get { return GetAttribute("rabbitUser"); }
            set { SetAttribute("rabbitUser", value); }
        }
        [CategoryAttribute("Required"), DisplayName("Rabbit Password"), ReadOnly(false), Browsable(true), PropertyOrder(5), DescriptionAttribute("The Rabbit MQ User Possword")]
        public string RabbitPass {
            get { return GetAttribute("rabbitPass"); }
            set { SetAttribute("rabbitPass", value); }
        }
        [CategoryAttribute("Required"), DisplayName("Rabbit Virtual Server"), ReadOnly(false), Browsable(true), PropertyOrder(6), DescriptionAttribute("The Rabbit Virtual Host. For example \"/\", \"ams\"")]
        public string RabbitVirtualHost {
            get { return GetAttribute("rabbitVHost"); }
            set { SetAttribute("rabbitVHost", value); }
        }


        [CategoryAttribute("Required"), DisplayName("Buffer Queue"), Browsable(true), PropertyOrder(16), DescriptionAttribute("Local MS MQ Queue that is used as an intermediate buffer")]
        public string Buffer {
            get { return GetAttribute("bufferQueueName"); }
            set { SetAttribute("bufferQueueName", value); }
        }

        [CategoryAttribute("Required - Set One of the Below to Route the Message"), Browsable(true), DisplayName("Queue Name"), PropertyOrder(2), DescriptionAttribute("The Rabbit MQ Queue Name")]
        public string QueueName {
            get { return GetAttribute("queueName"); }
            set { SetAttribute("queueName", value); }
        }
        [CategoryAttribute("Required - Set One of the Below to Route the Message"), Browsable(true), DisplayName("XPath Destination"), PropertyOrder(2), DescriptionAttribute("The XPath for the Routing Key")]
        public string XpathDestination {
            get { return GetAttribute("xpathDestination"); }
            set { SetAttribute("xpathDestination", value); }
        }

        [CategoryAttribute("Required - Set One of the Below to Route the Message"), Browsable(true), DisplayName("XPath Content Destination"), PropertyOrder(3), DescriptionAttribute("The XPath for the Routing Key Data")]
        public string XpathContentDestination {
            get { return GetAttribute("xpathContentDestination"); }
            set { SetAttribute("xpathContentDestination", value); }
        }

        [CategoryAttribute("Optional"), DisplayName("Undeliverable Queue"), PropertyOrder(4), ReadOnly(false), Browsable(true), DescriptionAttribute("Local MSMQ queue to send messages if they cannot be delivered")]
        public string Undeliverable {
            get { return GetAttribute("undeliverableQueue"); }
            set {
                SetAttribute("undeliverableQueue", value);
            }
        }
    }
}
