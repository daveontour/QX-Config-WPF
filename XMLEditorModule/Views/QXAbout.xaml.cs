using System.Windows;

namespace QXEditorModule.Views {
    public partial class QXAbout : Window {

        public QXAbout() {
            InitializeComponent();
            this.DataContext = this;
        }

        private void clickOK(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }
    }
}
