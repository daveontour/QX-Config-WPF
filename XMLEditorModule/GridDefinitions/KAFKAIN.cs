﻿using QXEditorModule.Common;
using System.ComponentModel;
using System.Reflection;
using System.Xml;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace QXEditorModule.GridDefinitions
{

    [DisplayName("Kafka Input Node")]
    public class KAFKAIN : MyNodeInPropertyGrid
    {

        public KAFKAIN(XmlNode dataModel, IView view)
        {
            this._node = dataModel;
            this.view = view;
            this.type = "KAFKA";

            // The "Priority Propery is only for input, so make sure it is set correctly
            PropertyDescriptor descriptor = TypeDescriptor.GetProperties(this.GetType())["Priority"];
            BrowsableAttribute theDescriptorBrowsableAttribute = (BrowsableAttribute)descriptor.Attributes[typeof(BrowsableAttribute)];
            FieldInfo isBrowsable = theDescriptorBrowsableAttribute.GetType().GetField("Browsable", BindingFlags.IgnoreCase | BindingFlags.NonPublic | BindingFlags.Instance);

            // Set the Descriptor's "Browsable" Attribute
            isBrowsable.SetValue(theDescriptorBrowsableAttribute, true);
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

        [CategoryAttribute("Optional"), DisplayName("Consumer Group"), Browsable(true), PropertyOrder(3), DescriptionAttribute("The Kafka Consumer Group for the connection")]
        public string ConsumerGroup {
            get { return GetAttribute("consumerGroup"); }
            set { SetAttribute("consumerGroup", value); }
        }

    }
}
