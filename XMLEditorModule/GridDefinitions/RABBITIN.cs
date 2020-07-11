using QXEditorModule.Common;
using System.ComponentModel;
using System.Reflection;
using System.Xml;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace QXEditorModule.GridDefinitions
{

    [DisplayName("Rabbit MQ Input Node")]
    public class RABBITIN : MyNodeInPropertyGrid
    {

        public RABBITIN(XmlNode dataModel, IView view)
        {
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

        [CategoryAttribute("Required"), DisplayName("Server"), Browsable(true), PropertyOrder(3), DescriptionAttribute("The URL of the Rabbit Server to connect to.")]
        public string Connection {
            get { return GetAttribute("connection"); }
            set { SetAttribute("connection", value); }
        }

        [CategoryAttribute("Required"), DisplayName("Queue"), Browsable(true), PropertyOrder(3), DescriptionAttribute("The Rabbit Queue to listen to.")]
        public string Topic {
            get { return GetAttribute("queue"); }
            set { SetAttribute("queue", value); }
        }
    }
}
