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
