using QXEditorModule.Common;
using System.ComponentModel;
using System.Xml;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace QXEditorModule.GridDefinitions
{

    [DisplayName("TCPOUT Client Output Node")]
    public class TCPOUT : MyNodePropertyGrid
    {


        public TCPOUT(XmlNode dataModel, IView view)
        {
            this._node = dataModel;
            this.view = view;
            this.type = "TCP Client";
        }


        [CategoryAttribute("Required"), DisplayName("Node Type"), PropertyOrder(1), Browsable(true), DescriptionAttribute("Type of the endpoint node"), ItemsSource(typeof(NodeTypeListOut))]
        public string TypeData {
            get { return "TCP Client"; }
            set { SetType(value); }
        }
        [CategoryAttribute("Required"), DisplayName("TCP Server"), PropertyOrder(1), Browsable(true), DescriptionAttribute("The TCP Server")]
        public string TCPHost {
            get { return GetAttribute("tcpServerIP"); }
            set { SetAttribute("tcpServerIP", value); }
        }

        [CategoryAttribute("Required"), DisplayName("TCP Port"), PropertyOrder(2), Browsable(true), DescriptionAttribute("The TCP Port")]
        public string TCPPort {
            get { return GetAttribute("tcpServerPort"); }
            set { SetAttribute("tcpServerPort", value); }
        }
    }
}
