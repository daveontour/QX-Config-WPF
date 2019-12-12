using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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

        public string  FileName { get; set; }
        public string ServiceName { get; set; }
        public string ServiceDisplayName { get; set; }
        public string ServiceDescription { get; set; }




    }
}
