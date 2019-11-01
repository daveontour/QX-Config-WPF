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

namespace WXE.Internal.Tools.ConfigEditor.XMLEditorModule.Views {
    /// <summary>
    /// Interaction logic for ExpressionControl.xaml
    /// </summary>
    public partial class ExpressionControl : UserControl {

        private static readonly KeyValuePair<string, string>[] nodeTypeList = {
            new KeyValuePair<string, string>("MSMQ","Microsoft MQ"),
            new KeyValuePair<string, string>("MQ","IBM MQ"),
            new KeyValuePair<string, string>("Kafka","Kafka"),
         };
        public ExpressionControl() {
            InitializeComponent();
        }
    }
}
