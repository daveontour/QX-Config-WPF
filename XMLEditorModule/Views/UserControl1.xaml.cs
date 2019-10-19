using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

using System.Xml;
using WXE.Internal.Tools.ConfigEditor.Common;
using WXE.Internal.Tools.ConfigEditor.XMLEditorModule.Common;

namespace WXE.Internal.Tools.ConfigEditor.XMLEditorModule.Views {

    public partial class UserControl1 : UserControl, INotifyPropertyChanged {

        private XmlNode _node;
        TreeEditorView _treeEditorView;
        private static readonly KeyValuePair<string, string>[] nodeTypeList = {
            new KeyValuePair<string, string>("MSMQ","Microsoft MQ"),
            new KeyValuePair<string, string>("MQ","IBM MQ"),
            new KeyValuePair<string, string>("Kafka","Kafka"),
         };
        private bool _seeQueue = false;


        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propName) {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
                _treeEditorView.ControlPropertyChange();
            }
        }
        public UserControl1(XmlNode selectedItem, TreeEditorView treeEditorView) {
            InitializeComponent();
            this.DataContext = this;
            _node = selectedItem;
            _treeEditorView = treeEditorView;
            ResetVisibility();

        }

        public KeyValuePair<string, string>[] NodeTypeList {
            get {
                return nodeTypeList;
            }
        }

        private void ResetVisibility() {
            if (NodeType != "Kafka") {
                SeeQueue = true;
            } else {
                SeeQueue = false;
            }
            OnPropertyChanged("Queue");
            OnPropertyChanged("QueueTitle");
            OnPropertyChanged("QName");
            OnPropertyChanged("NodeType");
            OnPropertyChanged("ConnectionString");
            OnPropertyChanged("QueueVisibile");
            OnPropertyChanged("Priority");
            UpdateLayout();
        }

        public string Queue {
            get { return _node.Attributes["queue"].Value; ; }
            set {
                _node.Attributes["queue"].Value = value;
                ResetVisibility();
            }
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
                } else {
                    return "Configuration";
                }
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
        public string QueueTitle {
            get { return GetAttributeTitle("Queue", "Queue"); }
        }
        public string NameTitle {
            get { return GetAttributeTitle("Name", "Name"); }
        }
        public string PriorityTitle {
            get { return GetAttributeTitle("Priority", "Priority"); }
        }
        public string ConnectionTitle {
            get { return GetAttributeTitle("Connection", "Connection"); }
        }
        public string ConnectionString {
            get { return _node.Attributes["connection"].Value; ; }
            set {
                _node.Attributes["connection"].Value = value;
                ResetVisibility();
            }
        }
        public string Priority {
            get { return _node.Attributes["priority"].Value; ; }
            set {
                _node.Attributes["priority"].Value = value;
                ResetVisibility();
            }
        }

        public string EndPointType {
            get { return _node.Name; }
        }
        public bool SeeQueue {
            get { return _seeQueue; }
            set {
                _seeQueue = value;
                OnPropertyChanged("Queue");
                UpdateLayout();
            }
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
        public string QName {
            get { return _node.Attributes["name"].Value; ; }
            set {
                _node.Attributes["name"].Value = value;
                ResetVisibility();
            }
        }

        public string NodeType {
            get { return _node.Attributes["type"].Value; ; }
            set {
                _node.Attributes["type"].Value = value;
                ResetVisibility();
            }
        }


        private void OK_Click(object sender, RoutedEventArgs e) {


        }

        private void Cancel_Click(object sender, RoutedEventArgs e) {


        }


    }
}
