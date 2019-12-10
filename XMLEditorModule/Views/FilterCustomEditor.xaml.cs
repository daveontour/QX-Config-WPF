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
using Xceed.Wpf.Toolkit.PropertyGrid;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;

namespace QXEditorModule.Views {
    /// <summary>
    /// Interaction logic for FilterCustomEditor.xaml
    /// </summary>
    public partial class FilterCustomEditor : UserControl,  ITypeEditor {
        public FilterCustomEditor() {
            InitializeComponent();
        }

         public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        "Value", typeof (IList<string>), typeof (FilterCustomEditor), new PropertyMetadata(default(IList<string>)));

        public FrameworkElement ResolveEditor(PropertyItem propertyItem) {
            var binding = new Binding("Value") {
                Source = propertyItem,
                Mode = propertyItem.IsReadOnly ? BindingMode.OneWay : BindingMode.TwoWay
            };
            BindingOperations.SetBinding(this, ValueProperty, binding);
            return this;
        }
    }
}
