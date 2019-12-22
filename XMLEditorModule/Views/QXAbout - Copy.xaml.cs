using System;
using System.Reflection;
using System.Windows;

namespace QXEditorModule.Views {
    public partial class QXAbout : Window {


        public QXAbout() {
            InitializeComponent();
            this.DataContext = this;
            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            this.VersionInfo = String.Format("Last Build: - version {0}", version);
        }

        private string VersionInfo;

        public string VersionString {
            get { return VersionInfo; }
            set { VersionInfo = value; }
        }


        private void clickOK(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }
    }
}
