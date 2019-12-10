using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

using System.Windows.Input;
using QXEditorModule.Common;
using System.ComponentModel;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using System.Globalization;
using System.Reflection;


namespace QXEditorModule.GridDefinitions {

    [DisplayName("IBM MQ Input Node")]
    [RefreshProperties(System.ComponentModel.RefreshProperties.All)]
    public class MQIN : MyNodeInPropertyGrid {

        public MQIN(XmlNode dataModel, IView view) {
            this._node = dataModel;
            this.view = view;
            this.type = "MQ";

            // The "Priority Propery is only for input, so make sure it is set correctly
            PropertyDescriptor descriptor = TypeDescriptor.GetProperties(this.GetType())["Priority"];
            BrowsableAttribute theDescriptorBrowsableAttribute = (BrowsableAttribute)descriptor.Attributes[typeof(BrowsableAttribute)];
            FieldInfo isBrowsable = theDescriptorBrowsableAttribute.GetType().GetField("Browsable", BindingFlags.IgnoreCase | BindingFlags.NonPublic | BindingFlags.Instance);

            // Set the Descriptor's "Browsable" Attribute
            isBrowsable.SetValue(theDescriptorBrowsableAttribute, true);


        }

        [CategoryAttribute("Required"), DisplayName("Node Type"), ReadOnly(false), Browsable(true), PropertyOrder(1), DescriptionAttribute("Type of the endpoint node"), ItemsSource(typeof(NodeTypeList))]
        public string TypeData {
            get { return "IBM MQ"; }
            set { SetType(value); }
        }

        [CategoryAttribute("Required - Connection"), DisplayName("Queue Manager"), ReadOnly(false), Browsable(true), PropertyOrder(1),  DescriptionAttribute("IBM MQ Queue Manager Name")]
        public string QManager {
            get {return GetAttribute("queueMgr"); }
            set { SetAttribute("queueMgr", value); }
        }
        [CategoryAttribute("Required - Connection"), DisplayName("Queue"), ReadOnly(false), Browsable(true), PropertyOrder(2), DescriptionAttribute("MQ Queue Name")]
        public string Queue {
            get {return GetAttribute("queue");}
            set { SetAttribute("queue", value);}
        }

        [CategoryAttribute("Required - Connection"), DisplayName("Channel"), ReadOnly(false), Browsable(true), PropertyOrder(3), DescriptionAttribute("Descriptive name of the queue")]
        public string Channel {
            get {return GetAttribute("channel");}
            set { SetAttribute("channel", value);}
        }

        [CategoryAttribute("Required - Connection"),  DisplayName("Host"), ReadOnly(false), Browsable(true), PropertyOrder(4),    DescriptionAttribute("Host name")]
        public string HostName {
            get {return GetAttribute("host"); }
            set {SetAttribute("host", value); }
        }

        [CategoryAttribute("Required - Connection"), DisplayName("Port"), ReadOnly(false), Browsable(true), PropertyOrder(5), DescriptionAttribute("TCP Port Number of Queue Manager")]
        public string Port {
            get { return GetAttribute("port"); }
            set { SetAttribute("port", value); }
        }

        [CategoryAttribute("Optional - Connection"), DisplayName("User Name"), ReadOnly(false), Browsable(true), PropertyOrder(1), DescriptionAttribute("MQ User Name")]
        public string UserName {
            get { return GetAttribute("username"); }
            set { SetAttribute("username", value); }
        }
        [CategoryAttribute("Optional - Connection"), DisplayName("User Password"), ReadOnly(false), Browsable(true), PropertyOrder(2), DescriptionAttribute("Connection password for this user")]
        public string UserPass {
            get { return GetAttribute("password"); }
            set {SetAttribute("password", value);  }
        }
    }
}
