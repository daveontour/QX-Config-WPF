using QXEditorModule.Common;
using System.ComponentModel;
using System.Xml;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace QXEditorModule.GridDefinitions {

    [DisplayName("HTTP Post Output Node")]
    public class HTTPOUT : MyNodePropertyGrid {


        public HTTPOUT(XmlNode dataModel, IView view) {
            this._node = dataModel;
            this.view = view;
            this.type = "HTTP Post";
        }

        [CategoryAttribute("Required"), DisplayName("Node Type"), PropertyOrder(1), Browsable(true), DescriptionAttribute("Type of the endpoint node"), ItemsSource(typeof(NodeTypeList))]
        public string TypeData {
            get { return "HTTP Post"; }
            set { SetType(value); }
        }

        [CategoryAttribute("Required"), DisplayName("HTTP Post URL"), PropertyOrder(2), Browsable(true), DescriptionAttribute("The URL to HTTP Post the message to.")]
        public string URL {
            get {return GetAttribute("postURL"); }
            set {SetAttribute("postURL", value); }
        }
    }
}
