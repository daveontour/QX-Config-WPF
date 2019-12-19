using QXEditorModule.Views;
using System;
using System.Collections.ObjectModel;

namespace QXEditorModule.ViewModels {
    public class TreeEditorsViewModel : BaseViewModel
    {
        private int activeTabIndex;
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
            get { return activeEditorIndex; }
            set
            {
                activeEditorIndex = value;
                OnPropertyChanged("ActiveEditorIndex");
                if (activeEditorIndex > -1 && activeEditorIndex < TreeEditors.Count)
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

        public int activeEditorIndex { get; set; }
    }
}
