using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Xml.XPath;
using WXE.Internal.Tools.ConfigEditor.XMLEditorModule.ViewModels;
using WXE.Internal.Tools.ConfigEditor.Common;
using System.ComponentModel;
using System.IO;

namespace WXE.Internal.Tools.ConfigEditor.XMLEditorModule.Views
{
  
    public partial class TreeEditorView : UserControl, INotifyPropertyChanged {
        public TreeEditorViewModel viewModel;
        private ContextMenuProvider contextMenuProvider;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propName) {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }
        public TreeEditorView()
        {           
            InitializeComponent();
            contextMenuProvider = new ContextMenuProvider();
            this.xmlTreeView.ContextMenu = new ContextMenu();
            this.DataContextChanged += new DependencyPropertyChangedEventHandler(TreeEditorView_DataContextChanged);
         }

        public string XMLText {
            get {
                StringBuilder sb = new StringBuilder();
                TextWriter tr = new System.IO.StringWriter(sb);
                XmlTextWriter wr = new XmlTextWriter(tr);
                wr.Formatting = Formatting.Indented;
                viewModel.DataModel.Save(wr);
                wr.Close();
                return sb.ToString();
            }
          
        }

        internal void ControlPropertyChange() {
            viewModel.OnPropertyChanged("XMLText");
        }

        void TreeEditorView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
           
               ViewModel = e.NewValue as TreeEditorViewModel;            
        }

        public TreeEditorViewModel ViewModel
        {
            get { return viewModel; }
            set
            {
                viewModel = value;
                this.Dispatcher.BeginInvoke((Action)delegate
                {
                    this.Cursor = Cursors.Wait;
                BindUIElementToViewModel();
                this.Cursor = Cursors.Arrow;
                }, System.Windows.Threading.DispatcherPriority.Background);
            }
        }

        private void xmlTreeView_Selected(object sender, RoutedEventArgs e)
        {
            XmlNode selectedItem  = xmlTreeView.SelectedItem as XmlNode;
            ViewModel.ViewAttributesCommand.Execute(selectedItem);

            if (selectedItem.Name == "input" || selectedItem.Name == "output" || selectedItem.Name == "logger" || selectedItem.Name == "monitor") {
                nodeEditorCntrl.Content = new UserControl1(selectedItem, this);
            } else if (selectedItem.Name == "pipe" ) {
                nodeEditorCntrl.Content = new PipeControl(selectedItem, this);
            } else if (selectedItem.Name == "namespace") {
                nodeEditorCntrl.Content = new NamespaceControl(selectedItem, this);
            } else {
                nodeEditorCntrl.Content = new NodeEndPointEditorView(selectedItem);
            }
        }

        private void BindUIElementToViewModel()
        {
            //this.DataContext = viewModel;
            if (viewModel == null)
            {
                return;
            }
           
            XmlDataProvider dataProvider = this.FindResource("xmlDataProvider") as XmlDataProvider;
            dataProvider.Document = viewModel.DataModel;
            dataProvider.Refresh();        
            this.xmlTreeView.ContextMenu.Items.Clear();

            contextMenuProvider.ContextMenus[ContextMenuType.AddMonitor].Command = ViewModel.AddMonitorCommand;
            contextMenuProvider.ContextMenus[ContextMenuType.AddMonitor].CommandParameter = XmlNodeType.Element;
            this.xmlTreeView.ContextMenu.Items.Add(contextMenuProvider.ContextMenus[ContextMenuType.AddMonitor]);

            contextMenuProvider.ContextMenus[ContextMenuType.AddLogger].Command = ViewModel.AddLoggerCommand;
            contextMenuProvider.ContextMenus[ContextMenuType.AddLogger].CommandParameter = XmlNodeType.Element;
            this.xmlTreeView.ContextMenu.Items.Add(contextMenuProvider.ContextMenus[ContextMenuType.AddLogger]);

            contextMenuProvider.ContextMenus[ContextMenuType.AddNamespace].Command = ViewModel.AddNamespaceCommand;
            contextMenuProvider.ContextMenus[ContextMenuType.AddNamespace].CommandParameter = XmlNodeType.Element;
            this.xmlTreeView.ContextMenu.Items.Add(contextMenuProvider.ContextMenus[ContextMenuType.AddNamespace]);


            // Add Pipes
            this.xmlTreeView.ContextMenu.Items.Add(new Separator());

            contextMenuProvider.ContextMenus[ContextMenuType.AddPipe].Command = ViewModel.AddPipeCommand;
            contextMenuProvider.ContextMenus[ContextMenuType.AddPipe].CommandParameter = XmlNodeType.Element;
            this.xmlTreeView.ContextMenu.Items.Add(contextMenuProvider.ContextMenus[ContextMenuType.AddPipe]);

            // Input and Output Nodes
            this.xmlTreeView.ContextMenu.Items.Add(new Separator());

            contextMenuProvider.ContextMenus[ContextMenuType.AddInput].Command = ViewModel.AddInputCommand;
            contextMenuProvider.ContextMenus[ContextMenuType.AddInput].CommandParameter = XmlNodeType.Element;
            this.xmlTreeView.ContextMenu.Items.Add(contextMenuProvider.ContextMenus[ContextMenuType.AddInput]);

            contextMenuProvider.ContextMenus[ContextMenuType.AddOutput].Command = ViewModel.AddOutputCommand;
            contextMenuProvider.ContextMenus[ContextMenuType.AddOutput].CommandParameter = XmlNodeType.Element;
            this.xmlTreeView.ContextMenu.Items.Add(contextMenuProvider.ContextMenus[ContextMenuType.AddOutput]);

            this.xmlTreeView.ContextMenu.Items.Add(new Separator());

            contextMenuProvider.ContextMenus[ContextMenuType.AddFilter].Command = ViewModel.AddFilterCommand;
            contextMenuProvider.ContextMenus[ContextMenuType.AddFilter].CommandParameter = XmlNodeType.Element;
            this.xmlTreeView.ContextMenu.Items.Add(contextMenuProvider.ContextMenus[ContextMenuType.AddFilter]);

            contextMenuProvider.ContextMenus[ContextMenuType.AddExpression].Command = ViewModel.AddExpressionCommand;
            contextMenuProvider.ContextMenus[ContextMenuType.AddExpression].CommandParameter = XmlNodeType.Element;
            this.xmlTreeView.ContextMenu.Items.Add(contextMenuProvider.ContextMenus[ContextMenuType.AddExpression]);

            this.xmlTreeView.ContextMenu.Items.Add(new Separator());


            contextMenuProvider.ContextMenus[ContextMenuType.Delete].Command = ViewModel.DeleteElementCommand;
            this.xmlTreeView.ContextMenu.Items.Add(contextMenuProvider.ContextMenus[ContextMenuType.Delete]);

            ViewModel.AddXmlNode = AddNewNodeFromUI;
            ViewModel.HighlightNodeInUI = HighlightNode;
        }

        XmlNode AddNewNodeFromUI(XmlNodeType xmlNodeType)
        {
            AddChildView popup = new AddChildView(this.ViewModel.DataModel, xmlNodeType);
            popup.ShowDialog();           
            return popup.NewNode;            
        }

       
        private void TreeViewItem_PreviewMouseRightButtonDown(object sender, MouseEventArgs e)
        {
            TreeViewItem selectedItem = sender as TreeViewItem;
            if (selectedItem != null)
            {
                selectedItem.IsSelected = true;
            }
          

        }


        #region TreeNode Search


        public void HighlightNode(XmlNode xmlNode)
        {
            bool isSelected = false;
            
            TreeViewItem rootNode = null;
            try
            {
                rootNode = xmlTreeView.ItemContainerGenerator.ContainerFromIndex(0) as TreeViewItem;
              
            }
            catch
            {

            }
            if (xmlNode == null)
            {
                isSelected = SelectTreeViewItem(ref rootNode, "");
            }
            else
            {
                isSelected = SelectTreeViewItem(ref rootNode, xmlNode);
            }
            if (!isSelected)
            {
                MessageBox.Show("Could not locate the node.");
            }

            //temp
            //XmlNode childNode = (xmlTreeView.SelectedItem as XmlNode).FirstChild.CloneNode(true);
            //SelectedNode.InsertAfter(childNode, SelectedNode.FirstChild);

        }

        private bool SelectTreeViewItem(ref TreeViewItem rootNode, XmlNode toBeSelectedNode)
        {
            bool isSelected = false;
            if (rootNode == null)
                return isSelected;

            if (!rootNode.IsExpanded)
            {
                rootNode.Focus();
                rootNode.IsExpanded = true;
            }
            XmlNode tempNode = rootNode.Header as XmlNode;
            if (tempNode == null)
            {
                return isSelected;
            }
            if(tempNode == toBeSelectedNode)
            //if (string.Compare(tempNode.Name, toBeSelectedNode.Name, true) == 0 && tempNode.NodeType == toBeSelectedNode.NodeType)
            {
                rootNode.IsSelected = true;
                rootNode.IsExpanded = true;
                isSelected = true;
                return isSelected;
            }
            else
            {
                for (int i = 0; i < rootNode.Items.Count; i++)
                {
                    TreeViewItem childItem = rootNode.ItemContainerGenerator.ContainerFromIndex(i) as TreeViewItem;

                    isSelected = SelectTreeViewItem(ref childItem, toBeSelectedNode);
                    if (isSelected)
                    {
                        break;
                    }
                }
                return isSelected;
            }


        }

        private bool SelectTreeViewItem(ref TreeViewItem rootNode, string elementName)
        {
            bool isSelected = false;
            if (rootNode == null)
                return isSelected;

            if (!rootNode.IsExpanded)
            {
                rootNode.Focus();
                rootNode.IsExpanded = true;
            }
            XmlNode tempNode = rootNode.Header as XmlNode;
            if (tempNode == null)
            {
                return isSelected;
            }
            if (string.Compare(tempNode.Name, elementName, true) == 0)
            {
                rootNode.IsSelected = true;
                rootNode.IsExpanded = true;
                isSelected = true;
                return isSelected;
            }
            else
            {
                for (int i = 0; i < rootNode.Items.Count; i++)
                {
                    TreeViewItem childItem = rootNode.ItemContainerGenerator.ContainerFromIndex(i) as TreeViewItem;

                    isSelected = SelectTreeViewItem(ref childItem, elementName);
                    if (isSelected)
                    {
                        break;
                    }
                }
                return isSelected;
            }


        }
        #endregion
    }
}
