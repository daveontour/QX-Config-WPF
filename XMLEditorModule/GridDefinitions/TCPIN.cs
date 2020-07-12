using QXEditorModule.Common;
using System.ComponentModel;
using System.Xml;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace QXEditorModule.GridDefinitions {

    [DisplayName("TCP Input Node")]
    public class TCPIN : MyNodePropertyGrid {


        public TCPIN(XmlNode dataModel, IView view) {
            this._node = dataModel;
            this.view = view;
            this.type = "TCP Server";
        }


        [CategoryAttribute("Required"), DisplayName("Node Type"), PropertyOrder(1), Browsable(true), DescriptionAttribute("Type of the endpoint node"), ItemsSource(typeof(NodeTypeListOut))]
        public string TypeData {
            get { return "TCP Server"; }
            set { SetType(value); }
        }
        [CategoryAttribute("Required"), DisplayName("TCP Server IP"), PropertyOrder(1), Browsable(true), DescriptionAttribute("The the IP address to run TCP Server on")]
        public string TCPHost {
            get {
                string val = GetAttribute("tcpServerIP");
                if (val == null) {
                    val = "127.0.0.1";
                    SetAttribute("tcpServerIP", val);
                }
                return val;
            }
            set { SetAttribute("tcpServerIP", value); }
        }

        [CategoryAttribute("Required"), DisplayName("TCP Port"), PropertyOrder(2), Browsable(true), DescriptionAttribute("The TCP Port")]
        public string TCPPort {
            get { return GetAttribute("tcpServerPort"); }
            set { SetAttribute("tcpServerPort", value); }
        }

        [CategoryAttribute("Required"), DisplayName("End of message marker"), PropertyOrder(4), Browsable(true), DescriptionAttribute("The text that indicates the end of the message")]
        public string EOF {
            get { return GetAttribute("eof"); }
            set { SetAttribute("eof", value); }
        }

        [CategoryAttribute("Required"), DisplayName("Remove EOM marker"), PropertyOrder(4), Browsable(true), DescriptionAttribute("REmove the End Of Message marker from the forwarded message")]
        public bool RemoveEOF {
            get { return GetBoolAttribute("eofRemove"); }
            set { SetAttribute("eofRemove", value); }
        }
    }
}
