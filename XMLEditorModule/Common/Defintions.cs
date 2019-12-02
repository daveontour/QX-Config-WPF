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
using Xceed.Wpf.Toolkit;

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
                "Microsoft MQ", "IBM MQ", "File","HTTP Post","RESTful","Kafka", "Rabbit MQ", "SINK", "Test Source"
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
                "Data Contains Value", "Data Equals Value", "Data Matches Regex.", "Data Minimum Length", "XPath Exists","XPath Equals","XPath Matches", "XPath Date Within Offset", "Context Contains"
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
    [CategoryOrder("Required - Set One of the Below to Route the Message",3)]
    [CategoryOrder("Required - One of the below for Kafka Topic", 3)]
    [CategoryOrder("Optional - Connection", 4)]
    [CategoryOrder("Optional", 5)]
    [CategoryOrder("Optional - Transformation", 6)]
    [CategoryOrder("Optional - Temporal Context Awareness", 7)]
    [RefreshProperties(RefreshProperties.All)]
    public class MyPropertyGrid  {
        public int maxMsgPerMinute = -1;
        public int maxMsg = -1;
        public string type = "MSMQ";
        public XmlNode _node;
        public IView view;

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

        protected bool GetBoolDefaultTrueAttribute(string attribName) {

            if (_node.Attributes[attribName] != null) {
                return bool.Parse(_node.Attributes[attribName].Value);
            } else {
                if (_node.Attributes[attribName] == null) {
                    XmlAttribute newAttribute = _node.OwnerDocument.CreateAttribute(attribName);
                    newAttribute.Value = "True";
                    _node.Attributes.Append(newAttribute);
                } else {
                    _node.Attributes[attribName].Value = "True";
                }
                view.UpdateParamBindings("XMLText");
                return true;
            }
        }

        protected int GetIntAttribute(string attribName) {

            if (_node.Attributes[attribName] != null) {
                return int.Parse(_node.Attributes[attribName].Value);
            } else {
                return -1;
            }
        }

        protected int GetIntAttribute(string attribName, int def) {

            if (_node.Attributes[attribName] != null) {
                return int.Parse(_node.Attributes[attribName].Value);
            } else {
                return def;
            }
        }

        protected void SetAttribute(string attribName, string value) {
            if (value == null || value == "") {
                try {
                    _node.Attributes.Remove(_node.Attributes[attribName]);
                } catch { }
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

        protected void SetAbsoluteAttribute(string attribName, bool value) {
            if ( _node.Attributes[attribName] != null) {
                _node.Attributes[attribName].Value = value.ToString();
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
                        SetAttribute("type", "FILE");
                        view.UpdateSelectedNodeCanvas(_node);
                        if (_node.Name == "input") view.FileInSource(_node);
                        if (_node.Name == "output") view.FileOutSource(_node);
                        break;
                    case "Kafka":
                        SetAttribute("type", "KAFKA");
                        view.UpdateSelectedNodeCanvas(_node);
                        if (_node.Name == "input") view.KafkaIn(_node);
                        if (_node.Name == "output") view.KafkaOut(_node);
                        break;
                    case "HTTP Post":
                        SetAttribute("type", "HTTP");
                        view.UpdateSelectedNodeCanvas(_node);
                        if (_node.Name == "input") view.HTTPIn(_node);
                        if (_node.Name == "output") view.HTTPOut(_node);
                        break;
                    case "Rabbit MQ":
                        SetAttribute("type", "RABBITDEFEX");
                        view.UpdateSelectedNodeCanvas(_node);
                        if (_node.Name == "input") view.RabbitIn(_node);
                        if (_node.Name == "output") view.RabbitOut(_node);
                        break;
                    case "SINK":
                        if (_node.Name == "input") {
                            MessageBox.Show("The Sink Type Cannot Be Used as an Input Type", "Invalid Input Node Type");
                            break;
                        }
                        SetAttribute("type", "SINK");
                        view.UpdateSelectedNodeCanvas(_node);
                        if (_node.Name == "output") view.SinkOut(_node);
                        break;
                    case "Test Source":
                        if (_node.Name == "output") {
                            MessageBox.Show("The Test Source Type Cannot Be Used as an Output Type", "Invalid Output Node Type");
                            break;
                        }
                        SetAttribute("type", "TESTSOURCE");
                        view.UpdateSelectedNodeCanvas(_node);
                        if (_node.Name == "input") view.TestSource(_node);
                        break;

                    case "RESTful":
                        if (_node.Name == "input") {
                            MessageBox.Show("The RESTful Type Cannot Be Used as an Input Type", "Invalid Input Node Type");
                            break;
                        }
                        SetAttribute("type", "REST");
                        view.UpdateSelectedNodeCanvas(_node);
                        if (_node.Name == "output") view.RestOut(_node);
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

                view.RefreshDraw();

            }
        }

        [CategoryAttribute("Optional - Transformation"), DisplayName("XSL Version"), ReadOnly(false), Browsable(true), PropertyOrder(3), DescriptionAttribute("XSLT Version"), ItemsSource(typeof(XSLTypeList))]
        public string XSLType {
            get { return GetAttribute("xslVersion"); }
            set { SetAttribute("xslVersion", value); ; }
        }

        [CategoryAttribute("Optional"), DisplayName("JSON Format"), ReadOnly(false), Browsable(true), PropertyOrder(3), DescriptionAttribute("By default, monitor records are XML format. Select for monitor records to be sent in JSON format")]
        public bool JSON {
            get {
                bool value = GetBoolAttribute("json");
                PropertyDescriptor descriptor = TypeDescriptor.GetProperties(this.GetType())["JSON"];
                BrowsableAttribute theDescriptorBrowsableAttribute = (BrowsableAttribute)descriptor.Attributes[typeof(BrowsableAttribute)];
                FieldInfo isBrowsable = theDescriptorBrowsableAttribute.GetType().GetField("Browsable", BindingFlags.IgnoreCase | BindingFlags.NonPublic | BindingFlags.Instance);

                if (_node.Name == "monitor") {
                    isBrowsable.SetValue(theDescriptorBrowsableAttribute, true);
                } else {
                    isBrowsable.SetValue(theDescriptorBrowsableAttribute, false);
                }


                return value; 
                
                }
            set { SetAttribute("json", value); ; }
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

        //[CategoryAttribute("Required"), DisplayName("XML Data"), PropertyOrder(10), DescriptionAttribute("True if the message in the data will be XML")]
        //public bool IsXML {
        //    get { return GetBoolDefaultTrueAttribute("isXML"); }
        //    set { SetAbsoluteAttribute("isXML", value); }
        //}
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

    public class EqualsFilter : MyPropertyGrid {

        public EqualsFilter(XmlNode dataModel, IView view) {
            this._node = dataModel;
            this.view = view;
            this.type = dataModel.Name;
        }

        [CategoryAttribute("Required"), DisplayName("Data Filter Type"), PropertyOrder(1), DescriptionAttribute("The type of data filter"), ItemsSource(typeof(FilterTypeList))]
        public string Type {
            get { return "Data Equals Value"; }
            set { this.view.ChangeFilterType(value); }
        }

        [CategoryAttribute("Required"), DisplayName("Value"), PropertyOrder(1), DescriptionAttribute("The string the data must equal for the filter to pass")]
        public string Value {
            get { return GetAttribute("value"); }
            set { SetAttribute("value", value); }
        }
    }



    public class MatchesFilter : MyPropertyGrid {

        public MatchesFilter(XmlNode dataModel, IView view) {
            this._node = dataModel;
            this.view = view;
            this.type = dataModel.Name;
        }

        [CategoryAttribute("Required"), DisplayName("Data Filter Type"), PropertyOrder(1), DescriptionAttribute("The type of data filter"), ItemsSource(typeof(FilterTypeList))]
        public string Type {
            get { return "Data Equals Value"; }
            set { this.view.ChangeFilterType(value); }
        }

        [CategoryAttribute("Required"), DisplayName("Value"), PropertyOrder(1), DescriptionAttribute("The Regex that the data must match")]
        public string Value {
            get { return GetAttribute("value"); }
            set { SetAttribute("value", value); }
        }
    }

    public class LengthFilter : MyPropertyGrid {

        public LengthFilter(XmlNode dataModel, IView view) {
            this._node = dataModel;
            this.view = view;
            this.type = dataModel.Name;
        }

        [CategoryAttribute("Required"), DisplayName("Data Filter Type"), PropertyOrder(1), DescriptionAttribute("The type of data filter"), ItemsSource(typeof(FilterTypeList))]
        public string Type {
            get { return "Data Minimum Length"; }
            set { this.view.ChangeFilterType(value); }
        }

        [CategoryAttribute("Required"), DisplayName("Minimum Length"), PropertyOrder(1), DescriptionAttribute("The Minimum Length of the Data")]
        public int Value {
            get { return GetIntAttribute("value"); }
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

        [CategoryAttribute("Required"), DisplayName("XPath"), PropertyOrder(1), DescriptionAttribute("The XPath that must exist for the filter to pass")]
        public string Value {
            get { return GetAttribute("xpath"); }
            set { SetAttribute("xpath", value); }
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

    public class XPEqualsFilter : MyPropertyGrid {

        public XPEqualsFilter(XmlNode dataModel, IView view) {
            this._node = dataModel;
            this.view = view;
            this.type = dataModel.Name;
        }

        [CategoryAttribute("Required"), DisplayName("Data Filter Type"), PropertyOrder(1), DescriptionAttribute("The type of data filter"), ItemsSource(typeof(FilterTypeList))]
        public string Type {
            get { return "XPath Equals"; }
            set { this.view.ChangeFilterType(value); }
        }

        [CategoryAttribute("Required"), DisplayName("XPath"), PropertyOrder(1), DescriptionAttribute("The XPath of the data element")]
        public string XPath {
            get { return GetAttribute("xpath"); }
            set { SetAttribute("xpath", value); }
        }


        [CategoryAttribute("Required"), DisplayName("Value"), PropertyOrder(1), DescriptionAttribute("The value at the specified XPath must equal")]
        public string Value {
            get { return GetAttribute("value"); }
            set { SetAttribute("value", value); }
        }
    }

    public class DateRangeFilter : MyPropertyGrid {

        public DateRangeFilter(XmlNode dataModel, IView view) {
            this._node = dataModel;
            this.view = view;
            this.type = dataModel.Name;
        }

        [CategoryAttribute("Required"), DisplayName("Data Filter Type"), PropertyOrder(1), DescriptionAttribute("The type of data filter"), ItemsSource(typeof(FilterTypeList))]
        public string Type {
            get { return "XPath Date Within Offset"; }
            set { this.view.ChangeFilterType(value); }
        }

        [CategoryAttribute("Required"), DisplayName("XPath"), PropertyOrder(1), DescriptionAttribute("The XPath Date Element to test")]
        public string Value {
            get { return GetAttribute("xpath"); }
            set { SetAttribute("xpath", value); }
        }

        [CategoryAttribute("Required"), DisplayName("From Offset"), PropertyOrder(1), DescriptionAttribute("From Date Offset in Days from Now")]
        public int From {
            get { return GetIntAttribute("fromOffset"); }
            set { SetAttribute("fromOffset", value); }
        }

        [CategoryAttribute("Required"), DisplayName("To Offset"), PropertyOrder(1), DescriptionAttribute("To Date Offset in Days from Now")]
        public int To {
            get { return GetIntAttribute("toOffset"); }
            set { SetAttribute("toOffset", value); }
        }
    }

    [DisplayName("Temporal Context Cache")]
    public class ContextFilter : MyPropertyGrid {

        public ContextFilter(XmlNode dataModel, IView view) {
            this._node = dataModel;
            this.view = view;
            this.type = dataModel.Name;
        }

        [CategoryAttribute("Required"), DisplayName("Data Filter Type"), PropertyOrder(1), DescriptionAttribute("The type of data filter"), ItemsSource(typeof(FilterTypeList))]
        public string Type {
            get { return "Context Contains"; }
            set { this.view.ChangeFilterType(value); }
        }

        [CategoryAttribute("Optional - Temporal Context Awareness"), DisplayName("Use Message Hash As Key"), PropertyOrder(1), DescriptionAttribute("Use a SHA256 hash of the entire message for the Context Cache Key (Duplicate Messages)")]
        public bool UseMessageAsKey {
            get {
                bool value = GetBoolAttribute("useMessageAsKey");
                return value;
            }
            set {
                SetAttribute("useMessageAsKey", value);

                if (ContextExpiry <= 0) {
                    ContextExpiry = 10;
                }
            }
        }

        [RefreshProperties(RefreshProperties.All)]
        [CategoryAttribute("Optional - Temporal Context Awareness"), DisplayName("Context Key"), PropertyOrder(2), DescriptionAttribute("XPath for the Context Key")]
        public string ContextKey {
            get { return GetAttribute("contextCacheKeyXPath"); }
            set {
                SetAttribute("contextCacheKeyXPath", value);
                if (ContextExpiry <= 0) {
                    ContextExpiry = 10;
                }
            }
        }

        [CategoryAttribute("Optional - Temporal Context Awareness"), DisplayName("Context Cache Expiry"), PropertyOrder(3), DescriptionAttribute("How long items remain in the context cache which also determines the rate of messages meeting the key will be sent")]
        public int ContextExpiry {
            get { return GetIntAttribute("contextCacheExpiry"); }
            set {
                if (value <= 0) {
                    value = 1;
                }
                SetAttribute("contextCacheExpiry", value);
            }
        }
    }


    [DisplayName("Service Setting")]
    public class ServiceSetting : MyPropertyGrid {

        public ServiceSetting(XmlNode dataModel, IView view) {
            this._node = dataModel;
            this.view = view;
            this.type = dataModel.Name;
        }

        [CategoryAttribute("Required"), DisplayName("Service Name"), PropertyOrder(1), DescriptionAttribute("The name of the service")]
        public string ServiceName {
            get { return GetAttribute("serviceName"); }
            set { SetAttribute("serviceName", value); }
        }

        [CategoryAttribute("Required"), DisplayName("Service Display Name"), PropertyOrder(2), DescriptionAttribute("The display name of the service")]
        public string Display {
            get { return GetAttribute("serviceDisplayName"); }
            set { SetAttribute("serviceDisplayName", value); }
        }

        [CategoryAttribute("Required"), DisplayName("Service Description"), PropertyOrder(3), DescriptionAttribute("The description of the service")]
        public string Description {
            get { return GetAttribute("serviceDescription"); }
            set { SetAttribute("serviceDescription", value); }
        }
    }
}

