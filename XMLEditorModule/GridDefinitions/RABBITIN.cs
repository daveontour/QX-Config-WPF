using QXEditorModule.Common;
using System.ComponentModel;
using System.Reflection;
using System.Xml;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace QXEditorModule.GridDefinitions {

    [DisplayName("Rabbit MQ Input Node")]
    public class RABBITIN : MyNodeInPropertyGrid {

        public RABBITIN(XmlNode dataModel, IView view) {
            this._node = dataModel;
            this.view = view;
            this.type = "RABBITDEFEX";

            // The "Priority Propery is only for input, so make sure it is set correctly
            PropertyDescriptor descriptor = TypeDescriptor.GetProperties(this.GetType())["Priority"];
            BrowsableAttribute theDescriptorBrowsableAttribute = (BrowsableAttribute)descriptor.Attributes[typeof(BrowsableAttribute)];
            FieldInfo isBrowsable = theDescriptorBrowsableAttribute.GetType().GetField("Browsable", BindingFlags.IgnoreCase | BindingFlags.NonPublic | BindingFlags.Instance);

            // Set the Descriptor's "Browsable" Attribute
            isBrowsable.SetValue(theDescriptorBrowsableAttribute, true);
        }

        [CategoryAttribute("Required"), DisplayName("Node Type"), Browsable(true), PropertyOrder(2), DescriptionAttribute("Type of the endpoint node"), ItemsSource(typeof(NodeTypeList))]
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

        [CategoryAttribute("Required"), DisplayName("Rabbit Queue"), ReadOnly(false), Browsable(true), PropertyOrder(7), DescriptionAttribute("The Rabbit MQ Queue")]
        public string RabbitQueue {
            get { return GetAttribute("queue"); }
            set { SetAttribute("queue", value); }
        }
    }
}
