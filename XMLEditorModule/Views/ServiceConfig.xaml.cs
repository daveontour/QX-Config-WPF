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

namespace WXE.Internal.Tools.ConfigEditor.XMLEditorModule.Views {
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class ServiceConfig : Window {

        
        public ServiceConfig() {
            InitializeComponent();
            this.DataContext = this;
        }

        private void clickOK(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }

        private string serviceName = "QX Service";

        public string ServiceName {
            get { return serviceName; }
            set { serviceName = value; }
        }

        private string serviceShortName = "QX Service";

        public string ServiceShortName {
            get { return serviceShortName; }
            set { serviceShortName = value; }
        }

        private string serviceDesc = "Connection between input and output nodes";

        public string ServiceDesc {
            get { return serviceDesc; }
            set { serviceDesc = value; }
        }

    }
}
