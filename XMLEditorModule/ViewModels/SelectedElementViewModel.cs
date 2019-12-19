using QXEditorModule.Common;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Xml;

namespace QXEditorModule.ViewModels {
    public class SelectedElementViewModel : BaseViewModel {
        private XmlNode currentNode;
        private ObservableCollection<ChildViewModel> children;
        public SelectedElementViewModel(XmlNode dataModel) {
            this.DataModel = dataModel;
            removeChildrenCommand = new RelayCommand((p) => { RemoveChildren(); });

            //addAttributeCommand = new RelayCommand<XmlNodeType>(p => { AddElement(p); });
            Children = new ObservableCollection<ChildViewModel>();
            UpdateChildren();
        }

        private XmlNode dataModel;

        public XmlNode DataModel { get { return dataModel; } private set { dataModel = value; OnPropertyChanged("DataModel"); } }

        public XmlNode CurrentNode {
            get { return currentNode; }
            set {
                currentNode = value;
                OnPropertyChanged("CurrentNode");
            }
        }

        public ObservableCollection<ChildViewModel> Children {
            get { return children; }
            set {
                children = value;
                OnPropertyChanged("Children");
            }
        }


        public Func<XmlNodeType, XmlNode> AddXmlNode { get; set; }

        #region Commands

        private ICommand removeChildrenCommand;

        public ICommand RemoveChildrenCommand {
            get { return removeChildrenCommand; }
        }

        private ICommand addAttributeCommand;

        public ICommand AddAttributeCommand {
            get { return addAttributeCommand; }
        }


        private void RemoveChildren() {
            foreach (var childVM in Children) {
                if (childVM.IsSelected && childVM.DataModel.NodeType == XmlNodeType.Attribute) {
                    this.DataModel.Attributes.Remove(childVM.DataModel as XmlAttribute);
                } else if (childVM.IsSelected && childVM.DataModel.NodeType == XmlNodeType.Text && this.DataModel.ParentNode != null) {
                    this.DataModel.ParentNode.RemoveChild(this.DataModel);
                }
            }

            UpdateChildren();
        }

        #endregion
        private void UpdateChildren() {
            if (this.DataModel == null) {
                return;
            }
            OnPropertyChanged("DataModel");
            this.Children.Clear();
            if (this.DataModel.NodeType == XmlNodeType.Element && this.DataModel.Attributes != null) {
                foreach (XmlAttribute item in this.DataModel.Attributes) {
                    var childVM = new ChildViewModel(item);
                    Children.Add(childVM);
                }
            } else if (this.DataModel.NodeType == XmlNodeType.Text) {
                var childVM = new ChildViewModel(this.DataModel);
                Children.Add(childVM);
            }
        }
    }
}
