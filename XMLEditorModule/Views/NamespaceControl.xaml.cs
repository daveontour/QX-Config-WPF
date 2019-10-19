using System.ComponentModel;
using System.Windows.Controls;
using System.Xml;

namespace WXE.Internal.Tools.ConfigEditor.XMLEditorModule.Views {

    public partial class NamespaceControl : UserControl, INotifyPropertyChanged {

        private XmlNode _node;
        TreeEditorView _treeEditorView;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propName) {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
                _treeEditorView.ControlPropertyChange();
            }
        }
        public NamespaceControl(XmlNode selectedItem, TreeEditorView treeEditorView) {
            InitializeComponent();
            this.DataContext = this;
            _node = selectedItem;
            _treeEditorView = treeEditorView;
        }



        public string Prefix {
            get { return _node.Attributes["prefix"].Value; ; }
            set {
                _node.Attributes["prefix"].Value = value;
                OnPropertyChanged("Prefix");
            }
        }

        public string URI {
            get { return _node.Attributes["uri"].Value; ; }
            set {
                _node.Attributes["uri"].Value = value;
                OnPropertyChanged("URI");
            }
        }
    }
}
