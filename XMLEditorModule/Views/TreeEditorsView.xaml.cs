using QXEditorModule.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;

namespace QXEditorModule.Views {

    public partial class TreeEditorsView : UserControl {
        private TreeEditorsViewModel viewModel;
        public TreeEditorsViewModel ViewModel {
            get { return viewModel; }
            set { this.DataContext = viewModel = value; }
        }
        public TreeEditorsView() {
            InitializeComponent();
        }


        void CloseTab_Handler(object sender, RoutedEventArgs e) {
            try {
                ViewModel.Remove((sender as CloseableTabItem).Content as TreeEditorViewModel);
            } catch (Exception ex) {
                Console.WriteLine(ex.Message);
            }
        }


    }
}
