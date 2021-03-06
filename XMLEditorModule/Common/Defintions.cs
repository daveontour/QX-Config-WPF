﻿using QXEditorModule.GridDefinitions;
using System;
using System.ComponentModel;
using System.Reflection;
using System.Xml;
using Xceed.Wpf.Toolkit;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace QXEditorModule.Common {


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
                "Microsoft MQ", "IBM MQ", "File","HTTP Post","Kafka", "Rabbit MQ", "TCP Server", "MQTT Subscriber", "Test Source"
            };
            return types; ;
        }
    }

    public class MQTTServerTypeList : IItemsSource {

        public Xceed.Wpf.Toolkit.PropertyGrid.Attributes.ItemCollection GetValues() {

            var types = new ItemCollection {
                "Web Services Server", "TCP Server"
            };
            return types; ;
        }
    }

    public class PriorityTypeList : IItemsSource {

        public Xceed.Wpf.Toolkit.PropertyGrid.Attributes.ItemCollection GetValues() {

            var types = new ItemCollection {
                "Highest", "Very High", "High", "Above Normal", "Normal", "Low", "Very Low", "Lowest"
            };
            return types; ;
        }
    }
    public class NodeTypeListOut : IItemsSource {

        public Xceed.Wpf.Toolkit.PropertyGrid.Attributes.ItemCollection GetValues() {

            var types = new ItemCollection {
                "Microsoft MQ", "IBM MQ", "File","HTTP Post","RESTful","Kafka", "Rabbit MQ", "TCP Client", "MQTT Publisher","SMTP","FTP", "SINK"
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
    [CategoryOrder("Required - Set One of the Below to Route the Message", 3)]
    [CategoryOrder("Required - One of the below for Kafka Topic", 3)]
    [CategoryOrder("Optional - Connection", 4)]
    [CategoryOrder("Optional", 5)]
    [CategoryOrder("Optional - Transformation", 6)]
    [CategoryOrder("Optional - Temporal Context Awareness", 7)]
    [RefreshProperties(RefreshProperties.All)]
    public class MyPropertyGrid {
        public string type = "MSMQ";
        public XmlNode _node;
        public IView view;

        protected void Show(string[] fields) {
            foreach (string field in fields) {
                ShowHide(field, true);
            }
        }
        protected void Hide(string[] fields) {
            foreach (string field in fields) {
                ShowHide(field, false);
            }
        }
        protected void Show(string field) {
            ShowHide(field, true);
        }
        protected void Hide(string field) {
            ShowHide(field, false);
        }
        protected void ShowHide(string field, bool value) {

            try {
                PropertyDescriptor descriptor = TypeDescriptor.GetProperties(this.GetType())[field];
                BrowsableAttribute theDescriptorBrowsableAttribute = (BrowsableAttribute)descriptor.Attributes[typeof(BrowsableAttribute)];
                FieldInfo isBrowsable = theDescriptorBrowsableAttribute.GetType().GetField("Browsable", BindingFlags.IgnoreCase | BindingFlags.NonPublic | BindingFlags.Instance);
                isBrowsable.SetValue(theDescriptorBrowsableAttribute, value);
            } catch (Exception) {
                Console.WriteLine($"Show/Hide Issue for {field}");
            }
        }

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

        protected bool GetBoolDefaultFalseAttribute(XmlNode _node, string attribName) {

            if (_node.Attributes[attribName] != null) {
                return bool.Parse(_node.Attributes[attribName].Value);
            } else {
                if (_node.Attributes[attribName] == null) {
                    XmlAttribute newAttribute = _node.OwnerDocument.CreateAttribute(attribName);
                    newAttribute.Value = "False";
                    _node.Attributes.Append(newAttribute);
                } else {
                    _node.Attributes[attribName].Value = "False";
                }
                view.UpdateParamBindings("XMLText");
                return false;
            }
        }
        protected int GetIntAttribute(string attribName) {

            if (_node.Attributes[attribName] != null) {
                int value;
                try {
                    value = int.Parse(_node.Attributes[attribName].Value);
                } catch (Exception e) {
                    Console.WriteLine(e.Message);
                    value = -1;
                }

                return value;
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


        protected void ClearAttributes() {
            _node.Attributes.RemoveAll();
            view.UpdateParamBindings("XMLText");
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
            if (_node.Attributes[attribName] != null) {
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

                string n = GetAttribute("name");
                string i = GetAttribute("id");

                ClearAttributes();

                SetAttribute("name", n);
                SetAttribute("id", i);

                switch (value) {
                    case "IBM MQ":
                        SetAttribute("type", "MQ");
                        view.UpdateSelectedNodeCanvas(_node);
                        if (_node.Name == "input") view.MQInSource(_node);
                        if (_node.Name == "output" || _node.Name == "altqueue" || _node.Name == "monitor") view.MQOutSource(_node);
                        break;
                    case "Microsoft MQ":
                        SetAttribute("type", "MSMQ");
                        view.UpdateSelectedNodeCanvas(_node);
                        if (_node.Name == "input") view.MSMQIn(_node);
                        if (_node.Name == "output" || _node.Name == "altqueue" || _node.Name == "monitor") view.MSMQOut(_node);
                        break;
                    case "File":
                        SetAttribute("type", "FILE");
                        view.UpdateSelectedNodeCanvas(_node);
                        if (_node.Name == "input") view.FileInSource(_node);
                        if (_node.Name == "output" || _node.Name == "altqueue" || _node.Name == "monitor") view.FileOutSource(_node);
                        break;
                    case "Kafka":
                        SetAttribute("type", "KAFKA");
                        view.UpdateSelectedNodeCanvas(_node);
                        if (_node.Name == "input") view.KafkaIn(_node);
                        if (_node.Name == "output" || _node.Name == "altqueue" || _node.Name == "monitor") view.KafkaOut(_node);
                        break;
                    case "HTTP Post":
                        SetAttribute("type", "HTTP");
                        view.UpdateSelectedNodeCanvas(_node);
                        if (_node.Name == "input") view.HTTPIn(_node);
                        if (_node.Name == "output" || _node.Name == "altqueue" || _node.Name == "monitor") view.HTTPOut(_node);
                        break;
                    case "Rabbit MQ":
                        SetAttribute("type", "RABBITDEFEX");
                        view.UpdateSelectedNodeCanvas(_node);
                        if (_node.Name == "input") view.RabbitIn(_node);
                        if (_node.Name == "output" || _node.Name == "altqueue" || _node.Name == "monitor") view.RabbitOut(_node);
                        break;
                    case "SMTP":
                        SetAttribute("type", "SMTP");
                        view.UpdateSelectedNodeCanvas(_node);
                        if (_node.Name == "output" || _node.Name == "altqueue" || _node.Name == "monitor") view.SMTPOut(_node);
                        break;
                    case "TCP Server":
                        SetAttribute("type", "TCPSERVER");
                        view.UpdateSelectedNodeCanvas(_node);
                        if (_node.Name == "input") view.TCPIN(_node);
                        break;
                    case "TCP Client":
                        SetAttribute("type", "TCPCLIENT");
                        view.UpdateSelectedNodeCanvas(_node);
                        if (_node.Name == "output" || _node.Name == "altqueue" || _node.Name == "monitor") view.TCPOUT(_node);
                        break;
                    case "MQTT Publisher":
                    case "MQTT Subscriber":
                        SetAttribute("type", "MQTT");
                        view.UpdateSelectedNodeCanvas(_node);
                        if (_node.Name == "input") view.MQTT(_node);
                        if (_node.Name == "output" || _node.Name == "altqueue" || _node.Name == "monitor") view.MQTT(_node);
                        break;
                    case "FTP":
                        SetAttribute("type", "FTP");
                        view.UpdateSelectedNodeCanvas(_node);
                        if (_node.Name == "output" || _node.Name == "altqueue" || _node.Name == "monitor") view.FTP(_node);
                        break;
                    case "SINK":
                        if (_node.Name == "input") {
                            MessageBox.Show("The Sink Type Cannot Be Used as an Input Type", "Invalid Input Node Type");
                            break;
                        }
                        SetAttribute("type", "SINK");
                        view.UpdateSelectedNodeCanvas(_node);
                        if (_node.Name == "output" || _node.Name == "altqueue" || _node.Name == "monitor") view.SinkOut(_node);
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
                        if (_node.Name == "output" || _node.Name == "altqueue" || _node.Name == "monitor") view.RestOut(_node);
                        break;

                }
            };
        }
    }

    // Base Class for the input and output types.
    public class MyNodePropertyGrid : MyPropertyGrid {


        [CategoryAttribute("Required"), DisplayName("Name"), ReadOnly(false), Browsable(true), PropertyOrder(1), DescriptionAttribute("Name of the Output")]
        public string Name {
            get {
                PropertyDescriptor descriptor = TypeDescriptor.GetProperties(this.GetType())["JSON"];
                BrowsableAttribute theDescriptorBrowsableAttribute = (BrowsableAttribute)descriptor.Attributes[typeof(BrowsableAttribute)];
                FieldInfo isBrowsable = theDescriptorBrowsableAttribute.GetType().GetField("Browsable", BindingFlags.IgnoreCase | BindingFlags.NonPublic | BindingFlags.Instance);

                if (_node.Name == "monitor") {
                    isBrowsable.SetValue(theDescriptorBrowsableAttribute, true);
                } else {
                    isBrowsable.SetValue(theDescriptorBrowsableAttribute, false);
                }
                return GetAttribute("name");
            }
            set { SetAttribute("name", value); }
        }


        [Editor(typeof(FileNameSelector), typeof(FileNameSelector))]
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

        [CategoryAttribute("Optional"), DisplayName("JSON Format"), Browsable(true), PropertyOrder(3), DescriptionAttribute("By default, monitor records are XML format. Select for monitor records to be sent in JSON format")]
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


        [CategoryAttribute("Required"), DisplayName("Priority"), Browsable(true), PropertyOrder(10), DescriptionAttribute("Input Priority"), ItemsSource(typeof(PriorityTypeList))]
        public string Priority {
            get {
                int val = GetIntAttribute("priority");
                switch (val) {
                    case 0:
                        return "Lowest";
                    case 1:
                        return "VeryLow";
                    case 2:
                        return "Low";
                    case 3:
                        return "Normal";
                    case 4:
                        return "Above Normal";
                    case 5:
                        return "High";
                    case 6:
                        return "Very High";
                    case 7:
                        return "Highest";
                    default:
                        SetAttribute("priority", 3);
                        return "Normal";
                }

            }
            set {

                switch (value) {
                    case "Highest":
                        SetAttribute("priority", 7);
                        break;
                    case "Very High":
                        SetAttribute("priority", 6);
                        break;
                    case "High":
                        SetAttribute("priority", 5);
                        break;
                    case "Above Normal":
                        SetAttribute("priority", 4);
                        break;
                    case "Normal":
                        SetAttribute("priority", 3);
                        break;
                    case "Low":
                        SetAttribute("priority", 2);
                        break;
                    case "Very Low":
                        SetAttribute("priority", 1);
                        break;
                    case "Lowest":
                        SetAttribute("priority", 0);
                        break;
                }
            }
        }
    }



    [DisplayName("Monitor")]
    public class MonitorGrid : MyPropertyGrid {
        public MonitorGrid(XmlNode dataModel, IView view) {
            this._node = dataModel;
            this.view = view;
        }

        [CategoryAttribute("Required"), DisplayName("Monitor Host IP"), Browsable(true), PropertyOrder(1), DescriptionAttribute("The IP address of the host running the monityr")]
        public string MonHost {
            get { return GetAttribute("host"); }
            set { SetAttribute("host", value); }
        }

        [CategoryAttribute("Required"), DisplayName("Monitor Host Port"), Browsable(true), PropertyOrder(2), DescriptionAttribute("The monitor port on the host")]
        public string MonIP {
            get { return GetAttribute("ip"); }
            set { SetAttribute("ip", value); }
        }

    }




    [DisplayName("Name Space Definition")]
    public class NameSpaceGrid : MyPropertyGrid {
        public NameSpaceGrid(XmlNode dataModel, IView view) {
            this._node = dataModel;
            this.view = view;
        }

        [CategoryAttribute("Required"), DisplayName("Prefix"), Browsable(true), PropertyOrder(1), DescriptionAttribute("Namespace Prefix")]
        public string Prefix {
            get { return GetAttribute("prefix"); }
            set { SetAttribute("prefix", value); }
        }

        [CategoryAttribute("Required"), DisplayName("URI"), Browsable(true), PropertyOrder(2), DescriptionAttribute("Namespace URI")]
        public string URI {
            get { return GetAttribute("uri"); }
            set { SetAttribute("uri", value); }
        }

        [CategoryAttribute("Required"), DisplayName("URI"), Browsable(false), PropertyOrder(2), DescriptionAttribute("Namespace URI")]
        public string Name {
            get { return "Name Spaces can be used in filter definitions, destination routing and context key definitions"; }
            set { }
        }
    }


    public class Filter : MyPropertyGrid {


        public Filter(XmlNode dataModel, IView view) {
            _node = dataModel;
            this.view = view;
            type = dataModel.Name;
        }

        [DisplayName("Alt Queue"), PropertyOrder(1), Browsable(true), DescriptionAttribute("The queue the message is sent to if it fails to pass the filter")]
        public string AltQueue {
            get {

                if (_node.InnerXml.Contains("altqueue")) {
                    return "Alt Queue is Set (Select in Tree to Edit)";
                } else {
                    return "Alt Queue is Not Set (Right Click on Filter to Add)";
                }
            }
        }

        [DisplayName("Filter Type"), PropertyOrder(1), Browsable(true), DescriptionAttribute("Filters can either be a single data filter or a compound boolean expression made up of several data filters")]
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

    [DisplayName("Boolean Expression of Child Nodes")]
    public class BooleanExpression : MyPropertyGrid {


        public BooleanExpression(XmlNode dataModel, IView view) {
            this._node = dataModel;
            this.view = view;
            this.type = dataModel.Name;
        }

        [CategoryAttribute("Required"), DisplayName("Boolean Type"), Browsable(true), PropertyOrder(1), DescriptionAttribute("The selected boolean operator is applied to all the child nodes to produce a result"), ItemsSource(typeof(BooleanTypeList))]
        public string Type {
            get { return this._node.Name; }
            set {
                if (this.view.CanChangeElementType(value)) {
                    this.view.ChangeElementType(value);
                }
            }
        }
    }

    [DisplayName("Data Contains Filter")]
    public class ContainsFilter : MyPropertyGrid {

        public ContainsFilter(XmlNode dataModel, IView view) {
            try {
                this._node = dataModel;
                this.view = view;
                this.type = dataModel.Name;
            } catch (Exception e) {
                Console.WriteLine("ERROR IN FILTER GRID CONSTRUCTOR");
                Console.WriteLine(e.Message);
            }
        }

        [CategoryAttribute("Required"), DisplayName("Data Filter Type"), Browsable(true), PropertyOrder(1), DescriptionAttribute("The type of data filter"), ItemsSource(typeof(FilterTypeList))]
        public string Type {
            get { return "Data Contains Value"; }
            set { this.view.ChangeFilterType(value); }
        }

        [CategoryAttribute("Required"), DisplayName("Value"), PropertyOrder(1), Browsable(true), DescriptionAttribute("The string the data must contain for the filter to pass")]
        public string Value {
            get { return GetAttribute("value"); }
            set { SetAttribute("value", value); }
        }
    }

    [DisplayName("Data Equals Filter")]
    public class EqualsFilter : MyPropertyGrid {

        public EqualsFilter(XmlNode dataModel, IView view) {
            this._node = dataModel;
            this.view = view;
            this.type = dataModel.Name;
        }

        [CategoryAttribute("Required"), DisplayName("Data Filter Type"), Browsable(true), PropertyOrder(1), DescriptionAttribute("The type of data filter"), ItemsSource(typeof(FilterTypeList))]
        public string Type {
            get { return "Data Equals Value"; }
            set { this.view.ChangeFilterType(value); }
        }

        [CategoryAttribute("Required"), DisplayName("Value"), PropertyOrder(1), Browsable(true), DescriptionAttribute("The string the data must equal for the filter to pass")]
        public string Value {
            get { return GetAttribute("value"); }
            set { SetAttribute("value", value); }
        }
    }




    [DisplayName("Data Matched RegEx Filter")]
    public class MatchesFilter : MyPropertyGrid {

        public MatchesFilter(XmlNode dataModel, IView view) {
            this._node = dataModel;
            this.view = view;
            this.type = dataModel.Name;
        }

        [CategoryAttribute("Required"), DisplayName("Data Filter Type"), Browsable(true), PropertyOrder(1), DescriptionAttribute("The type of data filter"), ItemsSource(typeof(FilterTypeList))]
        public string Type {
            get { return "Data Equals Value"; }
            set { this.view.ChangeFilterType(value); }
        }

        [CategoryAttribute("Required"), DisplayName("Value"), PropertyOrder(1), Browsable(true), DescriptionAttribute("The Regex that the data must match")]
        public string Value {
            get { return GetAttribute("value"); }
            set { SetAttribute("value", value); }
        }
    }

    [DisplayName("Data Minimum Length Filter")]
    public class LengthFilter : MyPropertyGrid {

        public LengthFilter(XmlNode dataModel, IView view) {
            this._node = dataModel;
            this.view = view;
            this.type = dataModel.Name;
        }

        [CategoryAttribute("Required"), DisplayName("Data Filter Type"), Browsable(true), PropertyOrder(1), DescriptionAttribute("The type of data filter"), ItemsSource(typeof(FilterTypeList))]
        public string Type {
            get { return "Data Minimum Length"; }
            set { this.view.ChangeFilterType(value); }
        }

        [CategoryAttribute("Required"), DisplayName("Minimum Length"), Browsable(true), PropertyOrder(1), DescriptionAttribute("The Minimum Length of the Data")]
        public int Value {
            get { return GetIntAttribute("value"); }
            set { SetAttribute("value", value); }
        }
    }
    [DisplayName("XPath Exists Filter")]
    public class XPExistsFilter : MyPropertyGrid {

        public XPExistsFilter(XmlNode dataModel, IView view) {
            this._node = dataModel;
            this.view = view;
            this.type = dataModel.Name;
        }

        [CategoryAttribute("Required"), DisplayName("Data Filter Type"), Browsable(true), PropertyOrder(1), DescriptionAttribute("The type of data filter"), ItemsSource(typeof(FilterTypeList))]
        public string Type {
            get { return "XPath Exists"; }
            set { this.view.ChangeFilterType(value); }
        }

        [CategoryAttribute("Required"), DisplayName("XPath"), PropertyOrder(1), Browsable(true), DescriptionAttribute("The XPath that must exist for the filter to pass")]
        public string Value {
            get { return GetAttribute("xpath"); }
            set { SetAttribute("xpath", value); }
        }
    }
    [DisplayName("Value at XPath Matches RegEx Filter")]
    public class XPMatchesFilter : MyPropertyGrid {

        public XPMatchesFilter(XmlNode dataModel, IView view) {
            this._node = dataModel;
            this.view = view;
            this.type = dataModel.Name;
        }

        [CategoryAttribute("Required"), DisplayName("Data Filter Type"), Browsable(true), PropertyOrder(1), DescriptionAttribute("The type of data filter"), ItemsSource(typeof(FilterTypeList))]
        public string Type {
            get { return "XPath Matches"; }
            set { this.view.ChangeFilterType(value); }
        }

        [CategoryAttribute("Required"), DisplayName("XPath"), Browsable(true), PropertyOrder(1), DescriptionAttribute("The XPath of the data element")]
        public string XPath {
            get { return GetAttribute("xpath"); }
            set { SetAttribute("xpath", value); }
        }


        [CategoryAttribute("Required"), DisplayName("Value"), Browsable(true), PropertyOrder(1), DescriptionAttribute("The RegEx that the data at the specified XPath must match")]
        public string Value {
            get { return GetAttribute("value"); }
            set { SetAttribute("value", value); }
        }
    }

    [DisplayName("Value at XPath Equals Filter")]
    public class XPEqualsFilter : MyPropertyGrid {

        public XPEqualsFilter(XmlNode dataModel, IView view) {
            this._node = dataModel;
            this.view = view;
            this.type = dataModel.Name;
        }

        [CategoryAttribute("Required"), DisplayName("Data Filter Type"), PropertyOrder(1), Browsable(true), DescriptionAttribute("The type of data filter"), ItemsSource(typeof(FilterTypeList))]
        public string Type {
            get { return "XPath Equals"; }
            set { this.view.ChangeFilterType(value); }
        }

        [CategoryAttribute("Required"), DisplayName("XPath"), PropertyOrder(1), Browsable(true), DescriptionAttribute("The XPath of the data element")]
        public string XPath {
            get { return GetAttribute("xpath"); }
            set { SetAttribute("xpath", value); }
        }


        [CategoryAttribute("Required"), DisplayName("Value"), PropertyOrder(1), Browsable(true), DescriptionAttribute("The value at the specified XPath must equal")]
        public string Value {
            get { return GetAttribute("value"); }
            set { SetAttribute("value", value); }
        }
    }

    [DisplayName("Date at XPath fall within Offset Filter")]
    public class DateRangeFilter : MyPropertyGrid {

        public DateRangeFilter(XmlNode dataModel, IView view) {
            this._node = dataModel;
            this.view = view;
            this.type = dataModel.Name;
        }

        [CategoryAttribute("Required"), DisplayName("Data Filter Type"), PropertyOrder(1), Browsable(true), DescriptionAttribute("The type of data filter"), ItemsSource(typeof(FilterTypeList))]
        public string Type {
            get { return "XPath Date Within Offset"; }
            set { this.view.ChangeFilterType(value); }
        }

        [CategoryAttribute("Required"), DisplayName("XPath"), PropertyOrder(1), Browsable(true), DescriptionAttribute("The XPath Date Element to test")]
        public string Value {
            get { return GetAttribute("xpath"); }
            set { SetAttribute("xpath", value); }
        }

        [CategoryAttribute("Required"), DisplayName("From Offset"), PropertyOrder(1), Browsable(true), DescriptionAttribute("From Date Offset in Days from Now")]
        public int From {
            get { return GetIntAttribute("fromOffset"); }
            set { SetAttribute("fromOffset", value); }
        }

        [CategoryAttribute("Required"), DisplayName("To Offset"), PropertyOrder(1), Browsable(true), DescriptionAttribute("To Date Offset in Days from Now")]
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

        [CategoryAttribute("Required"), DisplayName("Data Filter Type"), PropertyOrder(1), Browsable(true), DescriptionAttribute("The type of data filter"), ItemsSource(typeof(FilterTypeList))]
        public string Type {
            get { return "Context Contains"; }
            set { this.view.ChangeFilterType(value); }
        }

        [CategoryAttribute("Optional - Temporal Context Awareness"), DisplayName("Use Message Hash As Key"), PropertyOrder(1), Browsable(true), DescriptionAttribute("Use a SHA256 hash of the entire message for the Context Cache Key (Duplicate Messages)")]
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
        [CategoryAttribute("Optional - Temporal Context Awareness"), DisplayName("Context Key"), PropertyOrder(2), Browsable(true), DescriptionAttribute("XPath for the Context Key")]
        public string ContextKey {
            get { return GetAttribute("contextCacheKeyXPath"); }
            set {
                SetAttribute("contextCacheKeyXPath", value);
                if (ContextExpiry <= 0) {
                    ContextExpiry = 10;
                }
            }
        }

        [CategoryAttribute("Optional - Temporal Context Awareness"), DisplayName("Context Cache Expiry"), PropertyOrder(3), Browsable(true), DescriptionAttribute("How long items remain in the context cache which also determines the rate of messages meeting the key will be sent")]
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

        [CategoryAttribute("Required"), DisplayName("Service Name"), PropertyOrder(1), Browsable(true), DescriptionAttribute("The name of the service")]
        public string ServiceName {
            get { return GetAttribute("serviceName"); }
            set { SetAttribute("serviceName", value); }
        }

        [CategoryAttribute("Required"), DisplayName("Service Display Name"), PropertyOrder(2), Browsable(true), DescriptionAttribute("The display name of the service")]
        public string Display {
            get { return GetAttribute("serviceDisplayName"); }
            set { SetAttribute("serviceDisplayName", value); }
        }

        [CategoryAttribute("Required"), DisplayName("Service Description"), PropertyOrder(3), Browsable(true), DescriptionAttribute("The description of the service")]
        public string Description {
            get { return GetAttribute("serviceDescription"); }
            set { SetAttribute("serviceDescription", value); }
        }
    }
}

