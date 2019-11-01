using System.ComponentModel;
using System.Windows.Controls;
using System.Xml;

namespace WXE.Internal.Tools.ConfigEditor.XMLEditorModule.Views {

    public partial class BooleanControl : UserControl, INotifyPropertyChanged {

        private XmlNode _node;
        TreeEditorView _treeEditorView;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propName) {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
                _treeEditorView.ControlPropertyChange();
            }
        }
        public BooleanControl(XmlNode selectedItem, TreeEditorView treeEditorView) {
            InitializeComponent();
            this.DataContext = this;
            _node = selectedItem;
            _treeEditorView = treeEditorView;
        }
        private string GetAttribute(string attribName) {

            if (_node.Attributes[attribName] != null) {
                return _node.Attributes[attribName].Value;
            } else {
                return "";
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


        public string PipeName {
            get { return _node.Attributes["name"].Value; ; }
            set {
                if (_node.Attributes["name"] == null) {
                    XmlAttribute newAttribute = this._treeEditorView.viewModel.DataModel.CreateAttribute("name");
                    newAttribute.Value = value;
                    _node.Attributes.Append(newAttribute);
                } else {
                    _node.Attributes["name"].Value = value;
                }
                OnPropertyChanged("PipeName");
            }
        }

        public int MaxMess {
            get {
                if (_node.Attributes["maxMsgPerMinute"] != null) {
                    return int.Parse(_node.Attributes["maxMsgPerMinute"].Value);
                } else {
                    return -1;
                }

            }
            set {
                if (value == -1) {
                    _node.Attributes.Remove(_node.Attributes["maxMsgPerMinute"]);
                } else {
                    SetAttribute("maxMsgPerMinute", value.ToString());
                }
                OnPropertyChanged("MaxMess");
                OnPropertyChanged("MaxMessString");
            }
        }

        public string MaxMessString {
            get {
                if (_node.Attributes["maxMsgPerMinute"] == null) {
                    return "Unlimited";
                } else {
                    return _node.Attributes["maxMsgPerMinute"].Value;
                }
            }
        }

        public bool OutputModeAll {
            get {
                if (_node.Attributes["roundRobinDistribution"] == null && _node.Attributes["randomDistribution"] == null) {
                    return true;
                } else {
                    return false;
                }
            }
            set {
                if (value) {
                    _node.Attributes.Remove(_node.Attributes["roundRobinDistribution"]);
                    _node.Attributes.Remove(_node.Attributes["randomDistribution"]);
                }
                OnPropertyChanged("OutputModeAll");
            }
        }

        public bool OutputModeRound {
            get {
                if (_node.Attributes["roundRobinDistribution"] != null) {
                    return true;
                } else {
                    return false;
                }
            }
            set {
                if (value) {
                    _node.Attributes.Remove(_node.Attributes["randomDistribution"]);
                    SetAttribute("roundRobinDistribution", "true");
                }
                OnPropertyChanged("OutputModeRound");
            }
        }

        public bool OutputModeRandom {
            get {
                if (_node.Attributes["randomDistribution"] != null) {
                    return true;
                } else {
                    return false;
                }
            }
            set {
                if (value) {
                    _node.Attributes.Remove(_node.Attributes["roundRobinDistribution"]);
                    SetAttribute("randomDistribution", "true");
                }
                OnPropertyChanged("OutputModeRandom");
            }
        }

        public bool OutputIsolation {
            get {
                if (_node.Attributes["outputIsolation"] != null) {
                    return true;
                } else {
                    return false;
                }
            }
            set {
                if (value) {
                    SetAttribute("outputIsolation", value.ToString());
                } else {
                    _node.Attributes.Remove(_node.Attributes["outputIsolation"]);
                }
                OnPropertyChanged("OutputIsolation");
            }
        }

        public bool EnableLogging {
            get {
                if (_node.Attributes["enableLog"] != null) {
                    return true;
                } else {
                    return false;
                }
            }
            set {
                if (value) {
                    SetAttribute("enableLog", value.ToString());
                } else {
                    _node.Attributes.Remove(_node.Attributes["enableLog"]);
                }
                OnPropertyChanged("EnableLogging");
            }
        }
    }
}
