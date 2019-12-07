using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

using System.Windows.Input;
using WXE.Internal.Tools.ConfigEditor.Common;
using System.ComponentModel;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using WXE.Internal.Tools.ConfigEditor.XMLEditorModule.Common;
using System.Globalization;
using System.Reflection;
using WXE.Internal.Tools.ConfigEditor.XMLEditorModule.Views;


namespace WXE.Internal.Tools.ConfigEditor.XMLEditorModule.GridDefinitions {

    [DisplayName("Rabbit MQ Output Node")]
    public class RABBITOUT : MyNodePropertyGrid {

        public RABBITOUT(XmlNode dataModel, IView view) {
            this._node = dataModel;
            this.view = view;
            this.type = "Rabbit MQ";
        }

        [CategoryAttribute("Required"), DisplayName("Node Type"), Browsable(true), PropertyOrder(2), DescriptionAttribute("Type of the endpoint node"), ItemsSource(typeof(NodeTypeList))]
        public string NodeType {
            get { return "Rabbit MQ"; }
            set { SetType(value); }
        }

        [CategoryAttribute("Required"), DisplayName("Server"), Browsable(true), PropertyOrder(3), DescriptionAttribute("The URL of the Rabbit MQ Server to connect to.")]
        public string Connection {
            get { return GetAttribute("connection"); }
            set { SetAttribute("connection", value); }
        }

 
        [CategoryAttribute("Required"), DisplayName("Buffer Queue"), Browsable(true), PropertyOrder(6),  DescriptionAttribute("Local MS MQ Queue that is used as an intermediate buffer")]
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

        [CategoryAttribute("Required - Set One of the Below to Route the Message"), Browsable(true), DisplayName("XPath Content Destination"),  PropertyOrder(3), DescriptionAttribute("The XPath for the Routing Key Data")]
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
