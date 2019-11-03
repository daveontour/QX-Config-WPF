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
        public IView view;

        protected string GetAttribute(string attribName) {

            if (_node.Attributes[attribName] != null) {
                return _node.Attributes[attribName].Value;
            } else {
                return "";
            }
        }

        protected bool GetBoolAttribute(string attribName) {

            if (_node.Attributes[attribName] != null) {
                return bool.Parse(_node.Attributes[attribName].Value);
            } else {
                return false;
            }
        }

        protected int GetIntAttribute(string attribName) {

            if (_node.Attributes[attribName] != null) {
                return int.Parse(_node.Attributes[attribName].Value);
            } else {
                return -1;
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

            view.UpdateParamBindings("XMLText");

        }

        protected void SetAttribute(string attribName, bool value) {
            if ((!value) && _node.Attributes[attribName] != null) {
                _node.Attributes.Remove(_node.Attributes[attribName]);
            } else {

                if (_node.Attributes[attribName] == null) {
                    XmlAttribute newAttribute = _node.OwnerDocument.CreateAttribute(attribName);
                    newAttribute.Value = value.ToString();
                    _node.Attributes.Append(newAttribute);
                } else {
                    _node.Attributes[attribName].Value = value.ToString();
                }
            }

            view.UpdateParamBindings("XMLText");
        }

        protected void SetAttribute(string attribName, int value) {
            if ((value == -1) && _node.Attributes[attribName] != null) {
                _node.Attributes.Remove(_node.Attributes[attribName]);
            } else {

                if (_node.Attributes[attribName] == null) {
                    XmlAttribute newAttribute = _node.OwnerDocument.CreateAttribute(attribName);
                    newAttribute.Value = value.ToString();
                    _node.Attributes.Append(newAttribute);
                } else {
                    _node.Attributes[attribName].Value = value.ToString();
                }
            }

            view.UpdateParamBindings("XMLText");
        }

        protected void SetType(ETestEnum value) {
            if (this.type != value) {
                switch (value) {
                    case ETestEnum.IBMMQ:
                        SetAttribute("type", "MQ");
                        view.UpdateSelectedNodecanvas(_node);
                        view.MQSource(_node);
                        break;
                    case ETestEnum.MSMQ:
                        SetAttribute("type", "MSMQ");
                        view.UpdateSelectedNodecanvas(_node);
                        view.MSMQSource(_node);
                        break;
                }
            };
        }
    }

    public class PIPE : MyPropertyGrid {


        public PIPE(XmlNode dataModel, IView view) {
            this._node = dataModel;
            this.view = view;
        }

        [CategoryAttribute("Required"), DisplayName("Name"), PropertyOrder(1), DescriptionAttribute("Descriptive name of the pipe")]
        public string Name {
            get {
                return GetAttribute("name");
            }
            set {
                SetAttribute("name", value);
                view.UpdateSelectedPipeCanvas(_node);
            }
        }
        [CategoryAttribute("Optional"), DisplayName("Maximum Messages/Minute"), PropertyOrder(1), DescriptionAttribute("Maximum Number of Messages Per Minute (-1 for unlimited)")]
        public int MessPerMinute {
            get { return GetIntAttribute("maxMsgPerMinute"); }
            set {
                if (value < -1) {
                    SetAttribute("maxMsgPerMinute", -1);
                } else if (value > 250) {
                    SetAttribute("maxMsgPerMinute", 250);
                } else {
                    SetAttribute("maxMsgPerMinute", value);
                }
            }
        }

        [CategoryAttribute("Required"), DisplayName("Enable Logging"), PropertyOrder(3), DescriptionAttribute("Log envent on this pipe")]
        public bool EnableLogging {
            get { return GetBoolAttribute("enableLog"); }
            set { SetAttribute("enableLog", value); }
        }
    }

    public class NameSpaceGrid : MyPropertyGrid {
        public NameSpaceGrid(XmlNode dataModel, IView view) {
            this._node = dataModel;
            this.view = view;
         }

        [CategoryAttribute("Required"), DisplayName("Prefix"), PropertyOrder(1), DescriptionAttribute("Namespace Prefix")]
        public string Prefix {
            get { return GetAttribute("prefix");}
            set {SetAttribute("prefix", value);}
        }

        [CategoryAttribute("Required"), DisplayName("URI"), PropertyOrder(2), DescriptionAttribute("Namespace URI")]
        public string URI {
            get { return GetAttribute("uri"); }
            set { SetAttribute("uri", value); }
        }
    }

    public class MQ : MyPropertyGrid {


        public MQ(XmlNode dataModel, IView view) {
            this._node = dataModel;
            this.view = view;
            this.type = ETestEnum.IBMMQ;
        }
        [CategoryAttribute("Required"), DisplayName("Node Type"), PropertyOrder(1), DescriptionAttribute("Type of the endpoint node")]
        public ETestEnum ComboData {
            get { return this.type; }
            set { SetType(value); }
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



    public class MSMQ : MyPropertyGrid {


        public MSMQ(XmlNode dataModel, IView view) {
            this._node = dataModel;
            this.view = view;
            this.type = ETestEnum.MSMQ;
        }

        [CategoryAttribute("Required"), DisplayName("Node Type"), PropertyOrder(1), DescriptionAttribute("Type of the endpoint node")]
        public ETestEnum ComboData {
            get { return this.type; }
            set { SetType(value); }
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
