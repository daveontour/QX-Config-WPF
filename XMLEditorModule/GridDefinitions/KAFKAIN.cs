using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

using System.Windows.Input;
using QXEditorModule.Common;
using System.ComponentModel;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;




namespace QXEditorModule.GridDefinitions {

    [DisplayName("Kafka Input Node")]
    public class KAFKAIN : MyNodeInPropertyGrid {

        public KAFKAIN(XmlNode dataModel, IView view) {
            this._node = dataModel;
            this.view = view;
            this.type = "KAFKA";
        }

        [CategoryAttribute("Required"), DisplayName("Node Type"), Browsable(true), PropertyOrder(2), DescriptionAttribute("Type of the endpoint node"), ItemsSource(typeof(NodeTypeList))]
        public string NodeType {
            get { return "Kafka"; }
            set { SetType(value); }
        }

        [CategoryAttribute("Required"), DisplayName("Server"), Browsable(true), PropertyOrder(3), DescriptionAttribute("The URL of the Kafka Server to connect to.")]
        public string Connection {
            get { return GetAttribute("connection"); }
            set { SetAttribute("connection", value); }
        }

        [CategoryAttribute("Required"), DisplayName("Topic"), Browsable(true), PropertyOrder(3), DescriptionAttribute("The Kafka Topic to listen to.")]
        public string Topic {
            get { return GetAttribute("topic"); }
            set { SetAttribute("topic", value); }
        }

        [CategoryAttribute("Required"), DisplayName("Buffer Queue"), PropertyOrder(6), Browsable(true), DescriptionAttribute("Local MS MQ Queue that is used as an intermediate buffer")]
        public string Buffer {
            get { return GetAttribute("bufferQueueName"); }
            set { SetAttribute("bufferQueueName", value); }
        }


        [CategoryAttribute("Optional"), DisplayName("Consumer Group"), Browsable(true), PropertyOrder(3), DescriptionAttribute("The Kafka Consumer Group for the connection")]
        public string ConsumerGroup {
            get { return GetAttribute("consumerGroup"); }
            set { SetAttribute("consumerGroup", value); }
        }

    }
}
