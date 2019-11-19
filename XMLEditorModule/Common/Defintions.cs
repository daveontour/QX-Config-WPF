using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

using System.Windows.Input;
using WXE.Internal.Tools.ConfigEditor.Common;
using System.ComponentModel;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using WXE.Internal.Tools.ConfigEditor.XMLEditorModule.Common;
using System.Globalization;
using System.Reflection;
using WXE.Internal.Tools.ConfigEditor.XMLEditorModule.Views;

namespace WXE.Internal.Tools.ConfigEditor.XMLEditorModule.Common {


    public class GenericTypes {
        public String Name { get; set; }
        public String Value { get; set; }

        public override string ToString() {
            return Name;
        }
    }

    public class NodeTypeList : IItemsSource {

        public Xceed.Wpf.Toolkit.PropertyGrid.Attributes.ItemCollection GetValues() {

            var types = new ItemCollection {
                "Microsoft MQ", "IBM MQ", "File","HTTP","RESTful","Kafka", "Rabbit MQ"
            };
            return types; ;
        }
    }

    public class XSLTypeList : IItemsSource {

        public Xceed.Wpf.Toolkit.PropertyGrid.Attributes.ItemCollection GetValues() {

            var types = new ItemCollection {
                "1.0", "2.0", "3.0"
            };
            return types; ;
        }
    }

    public class OutputTypeList : IItemsSource {

        public Xceed.Wpf.Toolkit.PropertyGrid.Attributes.ItemCollection GetValues() {

            var types = new ItemCollection {
                "Distribute to All", "Round Robin", "Random"
            };
            return types; ;
        }
    }

    public class FilterTypeList : IItemsSource {

        public Xceed.Wpf.Toolkit.PropertyGrid.Attributes.ItemCollection GetValues() {

            var types = new ItemCollection {
                "Data Contains Value", "Data Matches Regex.", "Data Minimum Length", "XPath Exists","XPath Equals","XPath Matches", "Xpath Date Within Offset"
            };
            return types; ;
        }
    }

    public class BooleanTypeList : IItemsSource {

        public Xceed.Wpf.Toolkit.PropertyGrid.Attributes.ItemCollection GetValues() {

            var types = new ItemCollection {
                "and", "or", "xor", "not"
            };
            return types; ;
        }
    }

    [CategoryOrder("Required", 1)]
    [CategoryOrder("Required - Connection", 2)]
    [CategoryOrder("Optional - Connection", 3)]
    [CategoryOrder("Optional", 4)]
    [CategoryOrder("Optional - Transformation", 5)]
    [CategoryOrder("Optional - Temporal Context Awareness", 6)]
    [RefreshProperties(RefreshProperties.All)]
    public class MyPropertyGrid : INotifyPropertyChanged {
        public enum NodeTypeEnum {[Description("Microsoft MQ")] MSMQ, [Description("IBM MQ")] IBMMQ, [Description("File")] FILE, [Description("HTTP")] HTTP, [Description("HTTP Rest")] REST, [Description("Kafka")] KAFKA, [Description("Rabbit MQ")] RABBIT, [Description("SINK")] SINK, };
        public enum XSLVerEnum {[Description("1.0")] ONE, [Description("2.0")] TWO, [Description("3.0")] THREE }
        public int maxMsgPerMinute = -1;
        public int maxMsg = -1;
        public string type = "MSMQ";
        public XmlNode _node;
        public IView view;

        public event PropertyChangedEventHandler PropertyChanged;

        protected string GetAttribute(string attribName) {

            if (_node.Attributes[attribName] != null) {
                return _node.Attributes[attribName].Value;
            } else {
                return "";
            }
        }

        protected bool GetBoolAttribute(string attribName) {

            if (_node.Attributes[attribName] != null) {
                return bool.Parse(_node.Attributes[attribName].Value);
            } else {
                return false;
            }
        }

        protected int GetIntAttribute(string attribName) {

            if (_node.Attributes[attribName] != null) {
                return int.Parse(_node.Attributes[attribName].Value);
            } else {
                return -1;
            }
        }

        protected void SetAttribute(string attribName, string value) {
            if ((value == null || value == "") && _node.Attributes[attribName] != null) {
                _node.Attributes.Remove(_node.Attributes[attribName]);
            } else {

                if (_node.Attributes[attribName] == null) {
                    XmlAttribute newAttribute = _node.OwnerDocument.CreateAttribute(attribName);
                    newAttribute.Value = value;
                    _node.Attributes.Append(newAttribute);
                } else {
                    _node.Attributes[attribName].Value = value;
                }
            }

            view.UpdateParamBindings("XMLText");

        }

        protected void SetAttribute(string attribName, bool value) {
            if ((!value) && _node.Attributes[attribName] != null) {
                _node.Attributes.Remove(_node.Attributes[attribName]);
            } else {

                if (_node.Attributes[attribName] == null) {
                    XmlAttribute newAttribute = _node.OwnerDocument.CreateAttribute(attribName);
                    newAttribute.Value = value.ToString();
                    _node.Attributes.Append(newAttribute);
                } else {
                    _node.Attributes[attribName].Value = value.ToString();
                }
            }

            view.UpdateParamBindings("XMLText");
        }

        protected void SetAttribute(string attribName, int value) {
            if ((value == -1) && _node.Attributes[attribName] != null) {
                _node.Attributes.Remove(_node.Attributes[attribName]);
            } else {

                if (_node.Attributes[attribName] == null) {
                    XmlAttribute newAttribute = _node.OwnerDocument.CreateAttribute(attribName);
                    newAttribute.Value = value.ToString();
                    _node.Attributes.Append(newAttribute);
                } else {
                    _node.Attributes[attribName].Value = value.ToString();
                }
            }

            view.UpdateParamBindings("XMLText");
        }

        protected void SetType(string value) {
            if (this.type != value) {
                switch (value) {
                    case "IBM MQ":
                        SetAttribute("type", "MQ");
                        view.UpdateSelectedNodeCanvas(_node);
                        if (_node.Name == "input") view.MQInSource(_node);
                        if (_node.Name == "output") view.MQOutSource(_node);
                        break;
                    case "Microsoft MQ":
                        SetAttribute("type", "MSMQ");
                        view.UpdateSelectedNodeCanvas(_node);
                        if (_node.Name == "input") view.MSMQIn(_node);
                        if (_node.Name == "output") view.MSMQOut(_node);
                        break;
                    case "File":
                        SetAttribute("type", "File");
                        view.UpdateSelectedNodeCanvas(_node);
                        if (_node.Name == "input") view.FileInSource(_node);
                        if (_node.Name == "output") view.FileOutSource(_node);
                        break;
                    case "Kafka":
                        SetAttribute("type", "Kafka");
                        view.UpdateSelectedNodeCanvas(_node);
                        if (_node.Name == "input") view.KafkaIn(_node);
                        if (_node.Name == "output") view.KafkaOut(_node);
                        break;
                }
            };
        }
    }

    // Base Class for the input and output types.
    public class MyNodePropertyGrid : MyPropertyGrid {


        [CategoryAttribute("Required"), DisplayName("Name"), ReadOnly(false), Browsable(true), PropertyOrder(1), DescriptionAttribute("Name of the Output")]
        public string Name {
            get { return GetAttribute("name"); }
            set { SetAttribute("name", value); }
        }

        [RefreshProperties(RefreshProperties.All)]
        [CategoryAttribute("Optional - Transformation"), DisplayName("XSL Transform Style Sheet"), ReadOnly(false), Browsable(true), PropertyOrder(2), DescriptionAttribute("XSL StyleSheet to perform a transformation")]
        public string StyleSheet {
            get {
                string value = GetAttribute("stylesheet");
                PropertyDescriptor descriptor = TypeDescriptor.GetProperties(this.GetType())["XSLType"];
                BrowsableAttribute theDescriptorBrowsableAttribute = (BrowsableAttribute)descriptor.Attributes[typeof(BrowsableAttribute)];
                FieldInfo isBrowsable = theDescriptorBrowsableAttribute.GetType().GetField("Browsable", BindingFlags.IgnoreCase | BindingFlags.NonPublic | BindingFlags.Instance);

                if (value == null || value == "") {
                    isBrowsable.SetValue(theDescriptorBrowsableAttribute, false);
                    XSLType = "";
                } else {
                    isBrowsable.SetValue(theDescriptorBrowsableAttribute, true);
                }
                return value;
            }
            set {
                SetAttribute("stylesheet", value);
                PropertyDescriptor descriptor = TypeDescriptor.GetProperties(this.GetType())["XSLType"];
                BrowsableAttribute theDescriptorBrowsableAttribute = (BrowsableAttribute)descriptor.Attributes[typeof(BrowsableAttribute)];
                FieldInfo isBrowsable = theDescriptorBrowsableAttribute.GetType().GetField("Browsable", BindingFlags.IgnoreCase | BindingFlags.NonPublic | BindingFlags.Instance);

                if (value == null || value == "") {
                    isBrowsable.SetValue(theDescriptorBrowsableAttribute, false);
                    XSLType = "";
                } else {
                    isBrowsable.SetValue(theDescriptorBrowsableAttribute, true);
                }

            }
        }

        [CategoryAttribute("Optional - Transformation"), DisplayName("XSL Version"), ReadOnly(false), Browsable(true), PropertyOrder(3), DescriptionAttribute("XSLT Version"), ItemsSource(typeof(XSLTypeList))]
        public string XSLType {
            get { return GetAttribute("xslVersion"); }
            set { SetAttribute("xslVersion", value); ; }
        }
    }

    // Base Class for the input and output types.
    public class MyNodeInPropertyGrid : MyNodePropertyGrid {

        [CategoryAttribute("Optional"), DisplayName("Priority"), PropertyOrder(1), DescriptionAttribute("Input Priority ( 1 = highest )")]
        public int Priority {
            get {
                int val = GetIntAttribute("priority");
                return Math.Max(val, 1);
            }
            set {
                if (value < 1) {
                    value = 1;
                }
                SetAttribute("priority", value);
            }
        }
    }

    public class NameSpaceGrid : MyPropertyGrid {
        public NameSpaceGrid(XmlNode dataModel, IView view) {
            this._node = dataModel;
            this.view = view;
        }

        [CategoryAttribute("Required"), DisplayName("Prefix"), PropertyOrder(1), DescriptionAttribute("Namespace Prefix")]
        public string Prefix {
            get { return GetAttribute("prefix"); }
            set { SetAttribute("prefix", value); }
        }

        [CategoryAttribute("Required"), DisplayName("URI"), PropertyOrder(2), DescriptionAttribute("Namespace URI")]
        public string URI {
            get { return GetAttribute("uri"); }
            set { SetAttribute("uri", value); }
        }
    }


    public class Filter : MyPropertyGrid {


        public Filter(XmlNode dataModel, IView view) {
            _node = dataModel;
            this.view = view;
            type = dataModel.Name;
        }

        [DisplayName("Alt Queue"), PropertyOrder(1), DescriptionAttribute("The queue the message is sent to if it fails to pass the filter")]
        public string AltQueue {
            get {

                if (_node.InnerXml.Contains("altqueue")) {
                    return "Alt Queue is Set (Select in Tree to Edit)";
                } else {
                    return "Alt Queue is Not Set (Right Click on Filter to Add)";
                }
            }
        }

        [DisplayName("Filter Type"), PropertyOrder(1), DescriptionAttribute("Filters can either be a single data filter or a compound boolean expression made up of several data filters")]
        public string Type {
            get {
                if (_node.InnerXml.Contains("<and>") || _node.InnerXml.Contains("<or>") || _node.InnerXml.Contains("<xor>") || _node.InnerXml.Contains("<not>")) {
                    return "Compound Boolean Expression (Select in Tree to Edit)";
                } else {
                    if ((_node.InnerXml.Contains("altqueue") && _node.ChildNodes.Count == 1) || !_node.HasChildNodes) {
                        return "No Filter Has Been set Yet. (Right Click on Filter to Add)";
                    } else {
                        return "Simple Data Filter. (Select in Tree to Edit)";
                    }
                }
            }
        }
    }
    public class BooleanExpression : MyPropertyGrid {


        public BooleanExpression(XmlNode dataModel, IView view) {
            this._node = dataModel;
            this.view = view;
            this.type = dataModel.Name;
        }

        [CategoryAttribute("Required"), DisplayName("Boolean Type"), PropertyOrder(1), DescriptionAttribute("The selected boolean operator is applied to all the child nodes to produce a result"), ItemsSource(typeof(BooleanTypeList))]
        public string Type {
            get { return this._node.Name; }
            set {
                if (this.view.CanChangeElementType(value)) {
                    this.view.ChangeElementType(value);
                }
            }
        }
    }
    public class ContainsFilter : MyPropertyGrid {

        public ContainsFilter(XmlNode dataModel, IView view) {
            this._node = dataModel;
            this.view = view;
            this.type = dataModel.Name;
        }

        [CategoryAttribute("Required"), DisplayName("Data Filter Type"), PropertyOrder(1), DescriptionAttribute("The type of data filter"), ItemsSource(typeof(FilterTypeList))]
        public string Type {
            get { return "Data Contains Value"; }
            set { this.view.ChangeFilterType(value); }
        }

        [CategoryAttribute("Required"), DisplayName("Value"), PropertyOrder(1), DescriptionAttribute("The string the data must contain for the filter to pass")]
        public string Value {
            get { return GetAttribute("value"); }
            set { SetAttribute("value", value); }
        }
    }
    public class XPExistsFilter : MyPropertyGrid {

        public XPExistsFilter(XmlNode dataModel, IView view) {
            this._node = dataModel;
            this.view = view;
            this.type = dataModel.Name;
        }

        [CategoryAttribute("Required"), DisplayName("Data Filter Type"), PropertyOrder(1), DescriptionAttribute("The type of data filter"), ItemsSource(typeof(FilterTypeList))]
        public string Type {
            get { return "XPath Exists"; }
            set { this.view.ChangeFilterType(value); }
        }

        [CategoryAttribute("Required"), DisplayName("Value"), PropertyOrder(1), DescriptionAttribute("The XPath that must exist for the filter to pass")]
        public string Value {
            get { return GetAttribute("value"); }
            set { SetAttribute("value", value); }
        }
    }
    public class XPMatchesFilter : MyPropertyGrid {

        public XPMatchesFilter(XmlNode dataModel, IView view) {
            this._node = dataModel;
            this.view = view;
            this.type = dataModel.Name;
        }

        [CategoryAttribute("Required"), DisplayName("Data Filter Type"), PropertyOrder(1), DescriptionAttribute("The type of data filter"), ItemsSource(typeof(FilterTypeList))]
        public string Type {
            get { return "XPath Matches"; }
            set { this.view.ChangeFilterType(value); }
        }

        [CategoryAttribute("Required"), DisplayName("XPath"), PropertyOrder(1), DescriptionAttribute("The XPath of the data element")]
        public string XPath {
            get { return GetAttribute("xpath"); }
            set { SetAttribute("xpath", value); }
        }


        [CategoryAttribute("Required"), DisplayName("Value"), PropertyOrder(1), DescriptionAttribute("The RegEx that the data at the specified XPath must match")]
        public string Value {
            get { return GetAttribute("value"); }
            set { SetAttribute("value", value); }
        }
    }

}
