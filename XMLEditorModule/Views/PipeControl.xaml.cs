using System.ComponentModel;
using System.Windows.Controls;
using System.Xml;

namespace WXE.Internal.Tools.ConfigEditor.XMLEditorModule.Views {

    public partial class PipeControl : UserControl, INotifyPropertyChanged {

        private XmlNode _node;
        TreeEditorView _treeEditorView;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propName) {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
                _treeEditorView.ControlPropertyChange();
            }
        }
        public PipeControl(XmlNode selectedItem, TreeEditorView treeEditorView) {
            InitializeComponent();
            this.DataContext = this;
            _node = selectedItem;
            _treeEditorView = treeEditorView;
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
                if (_node.Attributes["maxMsgPerMinute"] == null) {
                    XmlAttribute newAttribute = this._treeEditorView.viewModel.DataModel.CreateAttribute("maxMsgPerMinute");
                    newAttribute.Value = "-1";
                    _node.Attributes.Append(newAttribute);
                    return -1;
                } else {
                    return int.Parse(_node.Attributes["maxMsgPerMinute"].Value);
                }
            }
            set {
                _node.Attributes["maxMsgPerMinute"].Value = value.ToString();
                OnPropertyChanged("MaxMess");
                OnPropertyChanged("MaxMessString");
            }
        }

        public string MaxMessString {
            get {
                if (_node.Attributes["maxMsgPerMinute"].Value == "-1") {
                    return "Unlimited";
                } else {
                    return _node.Attributes["maxMsgPerMinute"].Value;
                }
            }
        }

        public string OutputMode {
            get {
                if (_node.Attributes["roundRobinDistribution"] == null) {
                    XmlAttribute newAttribute = this._treeEditorView.viewModel.DataModel.CreateAttribute("roundRobinDistribution");
                    newAttribute.Value = "False";
                    _node.Attributes.Append(newAttribute);
                }
                if (_node.Attributes["randomDistribution"] == null) {
                    XmlAttribute newAttribute = this._treeEditorView.viewModel.DataModel.CreateAttribute("randomDistribution");
                    newAttribute.Value = "False";
                    _node.Attributes.Append(newAttribute);
                }

                if (_node.Attributes["roundRobinDistribution"].Value.ToLower() == "true") {
                    return "Round";
                } else if (_node.Attributes["randomDistribution"].Value.ToLower() == "true") {
                    return "Random";
                } else {
                    return "All";
                }
            }
            set {
                if (value == "Random") {
                    _node.Attributes["roundRobinDistribution"].Value = "false";
                    _node.Attributes["randomDistribution"].Value = "true";
                    OnPropertyChanged("OutputMode");
                } else if (value == "Round") {
                    _node.Attributes["roundRobinDistribution"].Value = "true";
                    _node.Attributes["randomDistribution"].Value = "false";
                    OnPropertyChanged("OutputMode");
                } else {
                    _node.Attributes["roundRobinDistribution"].Value = "false";
                    _node.Attributes["randomDistribution"].Value = "false";
                    OnPropertyChanged("OutputMode");
                }
            }
        }

        public bool OutputIsolation {
            get {
                if (_node.Attributes["outputIsolation"].Value.ToLower() == "true") {
                    return true;
                } else {
                    return false;
                }; ;
            }
            set {
                if (_node.Attributes["outputIsolation"] == null) {
                    XmlAttribute newAttribute = this._treeEditorView.viewModel.DataModel.CreateAttribute("outputIsolation");
                    newAttribute.Value = value.ToString();
                    _node.Attributes.Append(newAttribute);
                } else {
                    _node.Attributes["outputIsolation"].Value = value.ToString();
                }
                OnPropertyChanged("OutputIsolation");
            }
        }

        public bool EnableLogging {
            get {
                if (_node.Attributes["enableLog"].Value.ToLower() == "true") {
                    return true;
                } else {
                    return false;
                }; ;
            }
            set {
                if (_node.Attributes["enableLog"] == null) {
                    XmlAttribute newAttribute = this._treeEditorView.viewModel.DataModel.CreateAttribute("enableLog");
                    newAttribute.Value = value.ToString();
                    _node.Attributes.Append(newAttribute);
                } else {
                    _node.Attributes["enableLog"].Value = value.ToString();
                }
                OnPropertyChanged("EnableLogging");
            }
        }

    }
}
