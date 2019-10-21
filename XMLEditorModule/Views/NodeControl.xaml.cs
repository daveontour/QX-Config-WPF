using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

using System.Xml;
using WXE.Internal.Tools.ConfigEditor.Common;
using WXE.Internal.Tools.ConfigEditor.XMLEditorModule.Common;

namespace WXE.Internal.Tools.ConfigEditor.XMLEditorModule.Views {

    public partial class NodeControl : UserControl, INotifyPropertyChanged {

        private XmlNode _node;
        TreeEditorView _treeEditorView;
        private static readonly KeyValuePair<string, string>[] nodeTypeList = {
            new KeyValuePair<string, string>("MSMQ","Microsoft MQ"),
            new KeyValuePair<string, string>("MQ","IBM MQ"),
            new KeyValuePair<string, string>("Kafka","Kafka"),
         };

        private static readonly KeyValuePair<string, string>[] xslVersionList = {
            new KeyValuePair<string, string>("1.0","1.0"),
            new KeyValuePair<string, string>("2.0","2.0"),
            new KeyValuePair<string, string>("3.0","3.0"),
         };

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propName) {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
                _treeEditorView.ControlPropertyChange();
            }
        }
        public NodeControl(XmlNode selectedItem, TreeEditorView treeEditorView) {
            InitializeComponent();
            this.DataContext = this;
            _node = selectedItem;
            _treeEditorView = treeEditorView;
        }

        public KeyValuePair<string, string>[] NodeTypeList {
            get {
                return nodeTypeList;
            }
        }

        public KeyValuePair<string, string>[] XSLVersionList {
            get {
                return xslVersionList;
            }
        }
        private void SetAttribute(string attribName, string value) {
            if ((value == null || value == "") && _node.Attributes[attribName] != null) {
                _node.Attributes.Remove(_node.Attributes[attribName]);
            } else {

                if (_node.Attributes[attribName] == null) {
                    XmlAttribute newAttribute = this._treeEditorView.viewModel.DataModel.CreateAttribute(attribName);
                    newAttribute.Value = value;
                    _node.Attributes.Append(newAttribute);
                } else {
                    _node.Attributes[attribName].Value = value;
                }
            }
        }

        private string GetAttribute(string attribName) {

            if (_node.Attributes[attribName] != null) {
                return _node.Attributes[attribName].Value;
            } else {
                return "";
            }
        }
        public string GetAttributeTitle(string title, string param) {

            int conf = ParamConfig.ParamDict[NodeType][param];
            int inQ = conf & ParamConfig.InputRequired;
            int outQ = conf & ParamConfig.OutputRequired;

            if (IsInput && inQ > 0 || !IsInput && outQ > 0) {
                return $"*{title}";
            } else {
                return title;
            };
        }

        public bool IsInput {
            get {
                if (_node.Name == "input") {
                    return true;
                } else {
                    return false;
                };
            }
        }
        private void ResetVisibility() {
            OnPropertyChanged("Queue");
            OnPropertyChanged("QueueTitle");
            OnPropertyChanged("QName");
            OnPropertyChanged("NodeType");
            OnPropertyChanged("ConnectionString");
            OnPropertyChanged("QueueVisibile");
            OnPropertyChanged("Priority");
            OnPropertyChanged("XSLVersion");
            UpdateLayout();
        }

        public string PageTitle {
            get {
                if (_node.Name == "input") {
                    return "Input Node Configurarion";
                } else if (_node.Name == "output") {
                    return "Output Node Configurarion";
                } else if (_node.Name == "logger") {
                    return "Logger Queue Configurarion";
                } else if (_node.Name == "monitor") {
                    return "Monitor Queue Configurarion";
                } else if (_node.Name == "altqueue") {
                    return "Alternative Queue Configuration (messages that fail the filter)";
                } else {
                    return "Configuration";
                }
            }
        }

        public string QueueTitle {
            get { return GetAttributeTitle("Queue", "queue"); }
        }
        public string NameTitle {
            get { return GetAttributeTitle("Name", "name"); }
        }
        public string PriorityTitle {
            get { return GetAttributeTitle("Priority", "priority"); }
        }
        public string ConnectionTitle {
            get { return GetAttributeTitle("Connection", "connection"); }
        }


        public string Queue {
            get { return _node.Attributes["queue"].Value; ; }
            set {
                _node.Attributes["queue"].Value = value;
                ResetVisibility();
            }
        }

        public string ConnectionString {
            get { return GetAttribute("connection"); }
            set {
                SetAttribute("connection", value);
                ResetVisibility();
            }
        }
        public string Stylesheet {
            get { return GetAttribute("stylesheet"); }
            set {
                SetAttribute("stylesheet", value);
                OnPropertyChanged("Stylesheet");
            }
        }

        public string CreateQueue {
            get { return GetAttribute("createQueue"); }
            set {
                SetAttribute("createQueue", value.ToString());
                OnPropertyChanged("CreateQueue");
            }
        }
        public string Priority {
            get { return GetAttribute("priority"); }
            set {
                SetAttribute("priority", (string)value);
                OnPropertyChanged("Priority");
                ResetVisibility();
            }
        }



        public string EndPointType {
            get { return _node.Name; }
        }


        public string QName {
            get { return _node.Attributes["name"].Value; ; }
            set {
                SetAttribute("name", (string)value);
                ResetVisibility();
            }
        }

        public string XSLVersion {
            get { return GetAttribute("xslVersion"); }
            set {
                SetAttribute("xslVersion", (string)value);
                ResetVisibility();
            }
        }
        public string NodeType {
            get { return _node.Attributes["type"].Value; ; }
            set {
                SetAttribute("type", (string)value);
                ResetVisibility();
            }
        }
    }
}
