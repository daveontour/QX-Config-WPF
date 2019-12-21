using QXEditorModule.Common;
using System.ComponentModel;
using System.Xml;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace QXEditorModule.GridDefinitions {
    public class RESTOUT : MyNodePropertyGrid {


        public RESTOUT(XmlNode dataModel, IView view) {
            this._node = dataModel;
            this.view = view;
            this.type = "RESTFul";
        }

        [CategoryAttribute("Required"), DisplayName("Node Type"), PropertyOrder(1), Browsable(true), DescriptionAttribute("Type of the endpoint node"), ItemsSource(typeof(NodeTypeListOut))]
        public string TypeData {
            get { return "RESTful"; }
            set { SetType(value); }
        }

        [CategoryAttribute("Required"), DisplayName("Rest URL"), PropertyOrder(2), Browsable(true), DescriptionAttribute("The URL on the local machine that the output messages will be made available on. The form of the URL is 'http://localhost:8080/endpoint/'. Important! - it must include the '/' at the end")]
        public string URL {
            get { return GetAttribute("requestURL"); }
            set { SetAttribute("requestURL", value); }
        }

        [CategoryAttribute("Required"), DisplayName("Buffer Queue"), PropertyOrder(3), Browsable(true), DescriptionAttribute("The MS MQ queue that the messages will be held on until they are retrieved")]
        public string Buffer {
            get { return GetAttribute("bufferQueueName"); }
            set { SetAttribute("bufferQueueName", value); }
        }

        [CategoryAttribute("Optional"), DisplayName("Buffer Queue Max Size"), PropertyOrder(4), Browsable(true), DescriptionAttribute("The maximum number of messages to hold in the buffer. Oldest messages are deleted")]
        public int BufferSize {
            get {
                int val = GetIntAttribute("maxMessages", 10);
                view.UpdateParamBindings("XMLText");
                return val;
            }
            set { SetAttribute("maxMessages", value); }
        }

    }
}
