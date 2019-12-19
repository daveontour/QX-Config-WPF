using System.Windows;

namespace QXEditorModule.Views {
    public partial class QXPackageMsg : Window {

        public QXPackageMsg(string serviceName, string serviceDisplayName, string serviceDescription, string fileName) {
            InitializeComponent();
            this.DataContext = this;

            ServiceName = serviceName;
            ServiceDisplayName = serviceDisplayName;
            ServiceDescription = serviceDescription;
            FileName = fileName;
        }

        private void clickOK(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }

        public string FileName { get; set; }
        public string ServiceName { get; set; }
        public string ServiceDisplayName { get; set; }
        public string ServiceDescription { get; set; }
    }
}
