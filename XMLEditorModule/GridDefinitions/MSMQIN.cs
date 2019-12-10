using System.Xml;
using QXEditorModule.Common;
using System.ComponentModel;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace QXEditorModule.GridDefinitions {
    [DisplayName("Microsoft MQ Input Node")]
    public class MSMQIN : MyNodeInPropertyGrid {

        public MSMQIN(XmlNode dataModel, IView view) {
            this._node = dataModel;
            this.view = view;
            this.type = "MSMQ";
        }

        [CategoryAttribute("Required"), DisplayName("Node Type"), Browsable(true), PropertyOrder(2), DescriptionAttribute("Type of the endpoint node"), ItemsSource(typeof(NodeTypeList))]
        public string NodeType {
            get { return "Microsoft MQ"; }
            set { SetType(value); }
        }


        [CategoryAttribute("Required"), DisplayName("Queue"), Browsable(true), PropertyOrder(3), DescriptionAttribute("MS MQ Queue Name")]
        public string Queue {
            get { return GetAttribute("queue"); }
            set { SetAttribute("queue", value); }
        }
    }
}
