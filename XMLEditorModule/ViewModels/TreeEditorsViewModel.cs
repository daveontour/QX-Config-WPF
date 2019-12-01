using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using WXE.Internal.Tools.ConfigEditor.XMLEditorModule.Views;

namespace WXE.Internal.Tools.ConfigEditor.XMLEditorModule.ViewModels
{
    public class TreeEditorsViewModel : BaseViewModel
    {
        private ObservableCollection<TreeEditorViewModel> treeEditors = new ObservableCollection<TreeEditorViewModel>();
        private TreeEditorViewModel activeEditor;

        public TreeEditorViewModel ActiveEditor
        {
            get { return activeEditor; }
            set
            {
                activeEditor = value;
                OnPropertyChanged("ActiveEditor");
            }
        }
     

        public int ActiveEditorIndex
        {
            get { return _activeEditorIndex; }
            set
            {
                _activeEditorIndex = value;
                OnPropertyChanged("ActiveEditorIndex");
                if (_activeEditorIndex > -1 && _activeEditorIndex < TreeEditors.Count)
                {
                    CommandsBarView.executeMenuItem.IsEnabled = true;
                    CommandsBarView.exportMenuItem.IsEnabled = true;
                    CommandsBarView.saveMenuItem.IsEnabled = true;
                    CommandsBarView.saveAsMenuItem.IsEnabled = true;
                    ActiveEditor = TreeEditors[ActiveEditorIndex];
                       
                } else {
                    CommandsBarView.executeMenuItem.IsEnabled = false;
                    CommandsBarView.exportMenuItem.IsEnabled = false;
                    CommandsBarView.saveMenuItem.IsEnabled = false;
                    CommandsBarView.saveAsMenuItem.IsEnabled = false;
                }
            }
        }
        

        public ObservableCollection<TreeEditorViewModel> TreeEditors
        {
            get { return treeEditors; }
            set
            {
                treeEditors = value;
                OnPropertyChanged("TreeEditors");
            }
        }

        public void Add(TreeEditorViewModel treeEditor)
        {
            this.TreeEditors.Add(treeEditor);
            ActiveEditorIndex = this.TreeEditors.Count - 1;

        }
        public void Remove(TreeEditorViewModel treeEditor)
        {
            try {
        //        treeEditor.UnloadEditor();
                this.TreeEditors.Remove(treeEditor);
            } catch (Exception ex) {
                Console.WriteLine(ex);
                
            }
            ActiveEditorIndex = this.TreeEditors.Count - 1;
        }

        public int _activeEditorIndex;
    }
}
