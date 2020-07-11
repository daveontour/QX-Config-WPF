using QXEditorModule.Common;
using System.ComponentModel;
using System.Xml;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace QXEditorModule.GridDefinitions
{
    [DisplayName("HTTP Post Input Node")]
    public class HTTPIN : MyNodeInPropertyGrid
    {


        public HTTPIN(XmlNode dataModel, IView view)
        {
            this._node = dataModel;
            this.view = view;
            this.type = "HTTP Post";
        }

        [CategoryAttribute("Required"), DisplayName("Node Type"), PropertyOrder(1), Browsable(true), DescriptionAttribute("Type of the endpoint node"), ItemsSource(typeof(NodeTypeList))]
        public string TypeData {
            get { return "HTTP Post"; }
            set { SetType(value); }
        }

        [CategoryAttribute("Required"), DisplayName("HTTP Post URL"), PropertyOrder(2), Browsable(true), DescriptionAttribute("The URL on the local server for messgaes to be posted to. Messages will be consumed by the pipe when posted. The form of the URL is 'http://localhost:8080/endpoint/'. Important! - it must include the '/' at the end")]
        public string URL {
            get { return GetAttribute("postURL"); }
            set { SetAttribute("postURL", value); }
        }
    }
}
