﻿using System;
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
                "Microsoft MQ", "IBM MQ", "Kafka"
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
    [CategoryOrder("Optional", 2)]
    [CategoryOrder("Optional - Transformation", 3)]
    [CategoryOrder("Optional - Context Aware", 4)]
    public class MyPropertyGrid {
        public enum NodeTypeEnum {[Description("Microsoft MQ")] MSMQ, [Description("IBM MQ")] IBMMQ, [Description("Kafka")] KAFKA };
        public enum XSLVerEnum {[Description("1.0")] ONE, [Description("2.0")] TWO, [Description("3.0")] THREE }
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
                        view.MQSource(_node);
                        break;
                    case "Microsoft MQ":
                        SetAttribute("type", "MSMQ");
                        view.UpdateSelectedNodeCanvas(_node);
                        view.MSMQSource(_node);
                        break;
                }
            };
        }
    }

    public class PIPE : MyPropertyGrid {

        public PIPE(XmlNode dataModel, IView view) {
            this._node = dataModel;
            this.view = view;
        }

        [CategoryAttribute("Required"), DisplayName("Name"), PropertyOrder(1), DescriptionAttribute("Descriptive name of the pipe")]
        public string Name {
            get { return GetAttribute("name"); }
            set {
                SetAttribute("name", value);
                view.UpdateSelectedPipeCanvas(_node);
            }
        }
        [CategoryAttribute("Optional"), DisplayName("Maximum Messages/Minute"), PropertyOrder(1), DescriptionAttribute("Maximum Number of Messages Per Minute (-1 for unlimited)")]
        public int MessPerMinute {
            get { return GetIntAttribute("maxMsgPerMinute"); }
            set {
                if (value < -1) {
                    SetAttribute("maxMsgPerMinute", -1);
                } else if (value > 250) {
                    SetAttribute("maxMsgPerMinute", 250);
                } else {
                    SetAttribute("maxMsgPerMinute", value);
                }
            }
        }

        [CategoryAttribute("Required"), DisplayName("Enable Logging"), PropertyOrder(3), DescriptionAttribute("Log envent on this pipe")]
        public bool EnableLogging {
            get { return GetBoolAttribute("enableLog"); }
            set { SetAttribute("enableLog", value); }
        }

        [CategoryAttribute("Optional"), DisplayName("Output Isolation"), PropertyOrder(4), DescriptionAttribute("Isolate distribution of outputs from each other")]
        public bool OutputIsolation {
            get { return GetBoolAttribute("outputIsolation"); }
            set { SetAttribute("outputIsolation", value); }
        }
        [CategoryAttribute("Optional"), DisplayName("Distribution"), PropertyOrder(3), DescriptionAttribute("The type of ditribution to the output nodes"), ItemsSource(typeof(OutputTypeList))]
        public string OutputDistribution {
            get {
                string dist = GetAttribute("distribution");
                if (dist == "all" || dist == null || dist == "") {
                    return "Distribute to All";
                }
                if (dist == "roundRobin") {
                    return "Round Robin";
                }
                if (dist == "random") {
                    return "Random";
                }

                return "Distribute to All";
            }
            set {
                if (value == "Distribute to All") SetAttribute("distribution", null);
                if (value == "Round Robin") SetAttribute("distribution", "roundRobin");
                if (value == "Random") SetAttribute("distribution", "random");
            }
        }

        [CategoryAttribute("Optional - Context Aware"), DisplayName("Context Key"), PropertyOrder(1), DescriptionAttribute("XPath for the Context Key")]
        public string ContextKey {
            get { return GetAttribute("contextCacheKeyXPath"); }
            set {
                SetAttribute("maxMsgPerMinute", -1);
                SetAttribute("contextCacheKeyXPath", value);
                if (ContextExpiry <= 0) {
                    ContextExpiry = 10;
                }
            }
        }

        [CategoryAttribute("Optional - Context Aware"), DisplayName("Context Cache Expiry"), PropertyOrder(2), DescriptionAttribute("How long items remain in the context cache which also determines the rate of messages meeting the key will be sent")]
        public int ContextExpiry {
            get { return GetIntAttribute("contextCacheExpiry"); }
            set {
                if (value <= 0) {
                    value = 1;
                }
                SetAttribute("contextCacheExpiry", value);
            }
        }

        [CategoryAttribute("Optional - Context Aware"), DisplayName("Discard Messages"), PropertyOrder(3), DescriptionAttribute("Discard Messages if they already exist in the  Context Cache")]
        public bool DiscardCacheItems {
            get { return GetBoolAttribute("discardInCache"); }
            set { SetAttribute("discardInCache", value); }
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

    public class MQ : MyPropertyGrid {


        public MQ(XmlNode dataModel, IView view) {
            this._node = dataModel;
            this.view = view;
            this.type = "MQ";
        }
        [CategoryAttribute("Required"), DisplayName("Node Type"), PropertyOrder(1), DescriptionAttribute("Type of the endpoint node"), ItemsSource(typeof(NodeTypeList))]
        public string TypeData {
            get { return "IBM MQ"; }
            set { SetType(value); }
        }

        [CategoryAttribute("Required"),
        DisplayName("Queue Manager"),
        PropertyOrder(1),
        DescriptionAttribute("IBM MQ Queue Manager Name")]
        public string QManager {
            get;
            set;
        }

        [CategoryAttribute("Required"),
        DisplayName("Host"),
        PropertyOrder(2),
        DescriptionAttribute("Host name")]
        public string HostName {
            get;
            set;
        }

        [CategoryAttribute("Required"), DisplayName("Name"), PropertyOrder(1), DescriptionAttribute("Descriptive name of the queue")]
        public string Name {
            get {
                return GetAttribute("name");
            }
            set {
                SetAttribute("name", value);
            }
        }

        [CategoryAttribute("Required"), DisplayName("Queue"), PropertyOrder(1), DescriptionAttribute("MQ Queue Name")]
        public string Queue {
            get {
                return GetAttribute("name");
            }
            set {
                SetAttribute("name", value);
            }
        }
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

        [CategoryAttribute("Transformation"), DisplayName("XSL Transform Style Sheet"), PropertyOrder(2), DescriptionAttribute("XSL StyleSheet to perform a transformation")]
        public string StyleSheet {
            get { return GetAttribute("stylesheet"); }
            set { SetAttribute("stylesheet", value); }
        }

        [CategoryAttribute("Transformation"), DisplayName("XSL Version"), PropertyOrder(3), DescriptionAttribute("XSLT Version"), ItemsSource(typeof(XSLTypeList))]
        public string XSLType {
            get { return GetAttribute("xslVersion"); }
            set { SetAttribute("xslVersion", value); ; }
        }

        [CategoryAttribute("Optional"), DisplayName("Get Interval"), PropertyOrder(4), DescriptionAttribute("Time in seconds of the wait time for each interval of reading a message")]
        public int GetTimeout {
            get { return Math.Max(1, GetIntAttribute("getTimeout") / 1000); }
            set {
                value = Math.Max(1, value);
                SetAttribute("getTimeout", value * 1000);
            }
        }

        //[Editor(typeof(FilterCustomEditor), typeof(FilterCustomEditor))]
        //public int Filter {
        //    get { return view.Se }; 
        //    set;
        //}
    }

    public class MSMQInput : MyPropertyGrid {


        public MSMQInput(XmlNode dataModel, IView view) {
            this._node = dataModel;
            this.view = view;
            this.type = "MSMQ";
        }

        [CategoryAttribute("Required"), DisplayName("Node Type"), PropertyOrder(1), DescriptionAttribute("Type of the endpoint node"), ItemsSource(typeof(NodeTypeList))]
        public string NodeType {
            get { return "Microsoft MQ"; }
            set { SetType(value); }
        }


        [CategoryAttribute("Required"), DisplayName("Name"), PropertyOrder(1), DescriptionAttribute("Descriptive name of the queue")]
        public string Name {
            get {
                return GetAttribute("name");
            }
            set {
                SetAttribute("name", value);
            }
        }

        [CategoryAttribute("Required"), DisplayName("Queue"), PropertyOrder(3), DescriptionAttribute("MS MQ Queue Name")]
        public string Queue {
            get {
                return GetAttribute("queue");
            }
            set {
                SetAttribute("queue", value);
            }
        }

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

        [CategoryAttribute("Optional - Transformation"), DisplayName("XSL Transform Style Sheet"), PropertyOrder(2), DescriptionAttribute("XSL StyleSheet to perform a transformation")]
        public string StyleSheet {
            get { return GetAttribute("stylesheet"); }
            set { SetAttribute("stylesheet", value);
                view.DrawQXConfig();
            }
        }

        [CategoryAttribute("Optional - Transformation"), DisplayName("XSL Version"), PropertyOrder(3), DescriptionAttribute("XSLT Version"), ItemsSource(typeof(XSLTypeList))]
        public string XSLType {
            get { return GetAttribute("xslVersion"); }
            set { SetAttribute("xslVersion", value); ; }
        }

        [CategoryAttribute("Optional - Transformation"), DisplayName("Get Interval"), PropertyOrder(4), DescriptionAttribute("Time in seconds of the wait time for each interval of reading a message")]
        public int GetTimeout {
            get { return Math.Max(1, GetIntAttribute("getTimeout") / 1000); }
            set {
                value = Math.Max(1, value);
                SetAttribute("getTimeout", value * 1000);
            }
        }
    }

    public class BooleanExpression : MyPropertyGrid {


        public BooleanExpression(XmlNode dataModel, IView view) {
            this._node = dataModel;
            this.view = view;
            this.type = dataModel.Name;
        }

        [CategoryAttribute("Required"), DisplayName("Boolean Type"), PropertyOrder(1), DescriptionAttribute("Boolean Type"), ItemsSource(typeof(BooleanTypeList))]
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

        [CategoryAttribute("Required"), DisplayName("Value"), PropertyOrder(1), DescriptionAttribute("The XPath that must exist for the filter to pass")]
        public string Value {
            get { return GetAttribute("value"); }
            set { SetAttribute("value", value); }
        }
    }

}