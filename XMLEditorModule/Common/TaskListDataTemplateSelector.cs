using System.Windows;
using System.Windows.Controls;
using System.Xml;

namespace WXE.Internal.Tools.ConfigEditor.XMLEditorModule.Common {
    public class TaskListDataTemplateSelector : DataTemplateSelector {

        public override DataTemplate
        SelectTemplate(object item, DependencyObject container) {

            FrameworkElement element = container as FrameworkElement;
            XmlElement el = item as XmlElement;

            if (el.Name == "input") {
                return element.FindResource("InputNodeTemplate") as DataTemplate;
            } else if (el.Name == "output") {
                return element.FindResource("OutputNodeTemplate") as DataTemplate;
            } else if (el.Name == "pipes") {
                return element.FindResource("PipesNodeTemplate") as DataTemplate;
            } else if (el.Name == "pipe") {
                return element.FindResource("PipeNodeTemplate") as DataTemplate;
            } else if (el.Name == "config") {
                return element.FindResource("ConfigNodeTemplate") as DataTemplate;
            } else if (el.Name == "settings") {
                return element.FindResource("SettingsNodeTemplate") as DataTemplate;
            } else if (el.Name == "logger") {
                return element.FindResource("LoggerNodeTemplate") as DataTemplate;
            } else if (el.Name == "monitor") {
                return element.FindResource("MonitorNodeTemplate") as DataTemplate;
            } else if (el.Name == "filter") {
                return element.FindResource("FilterNodeTemplate") as DataTemplate;
            } else if (el.Name == "service") {
                return element.FindResource("ServiceNodeTemplate") as DataTemplate;
            } else if (el.Name == "namespace") {
                return element.FindResource("NamespaceNodeTemplate") as DataTemplate;
            } else if (el.Name == "altqueue") {
                return element.FindResource("AltQueueNodeTemplate") as DataTemplate;
            } else if (el.Name == "or") {
                return element.FindResource("ORExpression") as DataTemplate;
            } else if (el.Name == "xor") {
                return element.FindResource("XORExpression") as DataTemplate;
            } else if (el.Name == "and") {
                return element.FindResource("ANDExpression") as DataTemplate;
            } else if (el.Name == "not") {
                return element.FindResource("NOTExpression") as DataTemplate;
            } else {
                return element.FindResource("NodeTemplate") as DataTemplate;
            }
        }
    }
}
