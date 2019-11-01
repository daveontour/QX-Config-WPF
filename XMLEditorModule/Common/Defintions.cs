using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

using System.Windows.Input;
using WXE.Internal.Tools.ConfigEditor.Common;
using System.ComponentModel;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using WXE.Internal.Tools.ConfigEditor.XMLEditorModule.Common;

namespace WXE.Internal.Tools.ConfigEditor.XMLEditorModule.Common {
    [CategoryOrder("Required", 1)]
    [CategoryOrder("Optional", 2)]
    public class MyPropertyGrid {
        public enum ETestEnum { MSMQ, IBMMQ, KAFKA }
        public int maxMsgPerMinute = -1;
        public int maxMsg = -1;
        public ETestEnum type = ETestEnum.MSMQ;
        public XmlNode _node;

        protected string GetAttribute(string attribName) {

            if (_node.Attributes[attribName] != null) {
                return _node.Attributes[attribName].Value;
            } else {
                return "";
            }
        }

        protected void SetAttribute(string attribName, string value) {
            if ((value == null || value == "") && _node.Attributes[attribName] != null) {
                _node.Attributes.Remove(_node.Attributes[attribName]);
            } else {

                if (_node.Attributes[attribName] == null) {
                    XmlAttribute newAttribute = _node.OwnerDocument.CreateAttribute(attribName);
                    newAttribute.Value = value;
                    _node.Attributes.Append(newAttribute);
                } else {
                    _node.Attributes[attribName].Value = value;
                }
            }
        }
    }

    public class MQ : MyPropertyGrid {


        public MQ() {

        }

        [CategoryAttribute("Required"),
        DisplayName("Queue Manager"),
        PropertyOrder(1),
        DescriptionAttribute("IBM MQ Queue Manager Name")]
        public string QManager {
            get;
            set;
        }

        [CategoryAttribute("Required"),
        DisplayName("Host"),
        PropertyOrder(2),
        DescriptionAttribute("Host name")]
        public string HostName {
            get;
            set;
        }

        [CategoryAttribute("Required"), DisplayName("Name"), PropertyOrder(1), DescriptionAttribute("Descriptive name of the queue")]
        public string Name {
            get {
                return GetAttribute("name");
            }
            set {
                SetAttribute("name", value);
            }
        }

        [CategoryAttribute("Required"), DisplayName("Queue"), PropertyOrder(1), DescriptionAttribute("MQ Queue Name")]
        public string Queue {
            get {
                return GetAttribute("name");
            }
            set {
                SetAttribute("name", value);
            }
        }
    }

    public class PIPE : MyPropertyGrid {


        public PIPE(XmlNode dataModel) {
            this._node = dataModel;
        }

        [CategoryAttribute("Required"), DisplayName("Name"), PropertyOrder(1), DescriptionAttribute("Descriptive name of the pipe")]
        public string Name {
            get {
                return GetAttribute("name");
            }
            set {
                SetAttribute("name", value);
            }
        }
        [CategoryAttribute("Optional"), DisplayName("Maximum Messages/Minute"), PropertyOrder(1), DescriptionAttribute("Maximum Number of Messages Per Minute (-1 for unlimited)")]
        public int MessPerMinute {
            get { return maxMsgPerMinute; }
            set {
                if (value < -1) {
                    maxMsgPerMinute = -1;
                } else if (value > 250) {
                    maxMsgPerMinute = 250;
                } else {
                    maxMsgPerMinute = value;
                }
            }
        }

        [CategoryAttribute("Required"), DisplayName("Enable Logging"), PropertyOrder(3), DescriptionAttribute("Log envent on this pipe")]
        public bool EnableLogging {
            get;
            set;
        }




    }

    public class MSMQ : MyPropertyGrid {


        public MSMQ(XmlNode dataModel) {
            this._node = dataModel;
        }

        [CategoryAttribute("Required"), DisplayName("Name"), PropertyOrder(1), DescriptionAttribute("Descriptive name of the queue")]
        public string Name {
            get {
                return GetAttribute("name");
            }
            set {
                SetAttribute("name", value);
            }
        }

        [CategoryAttribute("Required"), DisplayName("Queue"), PropertyOrder(3), DescriptionAttribute("MS MQ Queue Name")]
        public string Queue {
            get {
                return GetAttribute("name");
            }
            set {
                SetAttribute("name", value);
            }
        }
    }

}
