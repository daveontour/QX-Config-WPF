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
    [CategoryOrder("Optional - Context Aware", 6)]
    public class MyPropertyGrid {
        public enum NodeTypeEnum {[Description("Microsoft MQ")] MSMQ, [Description("IBM MQ")] IBMMQ, [Description("File")] FILE, [Description("HTTP")] HTTP, [Description("HTTP Rest")] REST, [Description("Kafka")] KAFKA, [Description("Rabbit MQ")] RABBIT, [Description("SINK")] SINK, };
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
                        if (_node.Name == "input")view.MQInSource(_node);
                        if (_node.Name == "output") view.MQOutSource(_node);
                        break;
                    case "Microsoft MQ":
                        SetAttribute("type", "MSMQ");
                        view.UpdateSelectedNodeCanvas(_node);
                        view.MSMQSource(_node);
                        break;
                    case "File":
                        SetAttribute("type", "File");
                        view.UpdateSelectedNodeCanvas(_node);
                        if (_node.Name == "input") view.FileInSource(_node);
                        if (_node.Name == "output") view.FileOutSource(_node);
                        break;
                }
            };
        }
    }

    [DisplayName("Pipe Connecting Input and Output")]
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

        [CategoryAttribute("Required"), DisplayName("Enable Logging"), PropertyOrder(3), DescriptionAttribute("Log envent on this pipe")]
        public bool EnableLogging {
            get { return GetBoolAttribute("enableLog"); }
            set { SetAttribute("enableLog", value); }
        }

        [CategoryAttribute("Optional"), DisplayName("Maximum Messages/Min"), PropertyOrder(1), DescriptionAttribute("Maximum Number of Messages Per Minute (-1 for unlimited)")]
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

        [CategoryAttribute("Optional - Context Aware"), DisplayName("Context Cache Expiry"), PropertyOrder(3), DescriptionAttribute("How long items remain in the context cache which also determines the rate of messages meeting the key will be sent")]
        public int ContextExpiry {
            get { return GetIntAttribute("contextCacheExpiry"); }
            set {
                if (value <= 0) {
                    value = 1;
                }
                SetAttribute("contextCacheExpiry", value);
            }
        }

        [CategoryAttribute("Optional - Context Aware"), DisplayName("Discard Messages"), PropertyOrder(4), DescriptionAttribute("Discard Messages if they already exist in the  Context Cache")]
        public bool DiscardCacheItems {
            get { return GetBoolAttribute("discardInCache"); }
            set { SetAttribute("discardInCache", value); }
        }
        [CategoryAttribute("Optional - Context Aware"), DisplayName("Use Message As Key"), PropertyOrder(2), DescriptionAttribute("Use a SHA256 hash of the entire message Isolate for the Context Cache Key (Duplicate Messages)")]
        public bool UseMessageAsKey {
            get { return GetBoolAttribute("useMessageAsKey"); }
            set { SetAttribute("useMessageAsKey", value); }
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

    [DisplayName("IBM MQ Input Node")]
    public class MQIN : MyPropertyGrid {


        public MQIN(XmlNode dataModel, IView view) {
            this._node = dataModel;
            this.view = view;
            this.type = "MQ";
        }

        [CategoryAttribute("Required"), DisplayName("Name"), PropertyOrder(1), DescriptionAttribute("Name of the Output")]
        public string Name {
            get { return GetAttribute("name"); }
            set { SetAttribute("name", value); }
        }
        [CategoryAttribute("Required"), DisplayName("Node Type"), PropertyOrder(1), DescriptionAttribute("Type of the endpoint node"), ItemsSource(typeof(NodeTypeList))]
        public string TypeData {
            get { return "IBM MQ"; }
            set { SetType(value); }
        }

        [CategoryAttribute("Required - Connection"),
        DisplayName("Queue Manager"),
        PropertyOrder(1),
        DescriptionAttribute("IBM MQ Queue Manager Name")]
        public string QManager {
            get {
                return GetAttribute("queueMgr");
            }
            set {
                SetAttribute("queueMgr", value);
            }
        }
        [CategoryAttribute("Required - Connection"), DisplayName("Queue"), PropertyOrder(2), DescriptionAttribute("MQ Queue Name")]
        public string Queue {
            get {
                return GetAttribute("queue");
            }
            set {
                SetAttribute("queue", value);
            }
        }

        [CategoryAttribute("Required - Connection"), DisplayName("Channel"), PropertyOrder(3), DescriptionAttribute("Descriptive name of the queue")]
        public string Channel {
            get {
                return GetAttribute("channel");
            }
            set {
                SetAttribute("channel", value);
            }
        }
        [CategoryAttribute("Required - Connection"),
        DisplayName("Host"),
        PropertyOrder(4),
        DescriptionAttribute("Host name")]
        public string HostName {
            get {
                return GetAttribute("host");
            }
            set {
                SetAttribute("host", value);
            }
        }

        [CategoryAttribute("Required - Connection"), DisplayName("Port"), PropertyOrder(5), DescriptionAttribute("TCP Port Number of Queue Manager")]
        public string Port {
            get {
                return GetAttribute("port");
            }
            set {
                SetAttribute("port", value);
            }
        }

 
        [CategoryAttribute("Optional - Connection"), DisplayName("User Name"), PropertyOrder(1), DescriptionAttribute("MQ User Name")]
        public string UserName {
            get {
                return GetAttribute("username");
            }
            set {
                SetAttribute("username", value);
            }
        }
        [CategoryAttribute("Optional - Connection"), DisplayName("User Password"), PropertyOrder(2), DescriptionAttribute("Connection password for this user")]
        public string UserPass {
            get {
                return GetAttribute("password");
            }
            set {
                SetAttribute("password", value);
            }
        }


        [CategoryAttribute("Optional - Transformation"), DisplayName("XSL Transform Style Sheet"), PropertyOrder(2), DescriptionAttribute("XSL StyleSheet to perform a transformation")]
        public string StyleSheet {
            get { return GetAttribute("stylesheet"); }
            set { SetAttribute("stylesheet", value); }
        }

        [CategoryAttribute("Optional - Transformation"), DisplayName("XSL Version"), PropertyOrder(3), DescriptionAttribute("XSLT Version"), ItemsSource(typeof(XSLTypeList))]
        public string XSLType {
            get { return GetAttribute("xslVersion"); }
            set { SetAttribute("xslVersion", value); ; }
        }
    }


    [DisplayName("IBM MQ Output Node")]
    public class MQOUT : MQIN {


        public MQOUT(XmlNode dataModel, IView view) :base(dataModel, view){
        }

        [CategoryAttribute("Optional"), DisplayName("Maximum Messages"), PropertyOrder(3), DescriptionAttribute("Maximum Number of Messages Allowed in the Queue. (Older messages will be replaced)")]
        public int MaxMessages {
            get { return GetIntAttribute("maxMessages"); }
            set {
                if (value <= -1) {
                    value = -1;
                }
                SetAttribute("maxMessages", value);
            }
        }
    }

        [DisplayName("File Input")]
    public class FILEIN : MyPropertyGrid {


        public FILEIN(XmlNode dataModel, IView view) {
            this._node = dataModel;
            this.view = view;
            this.type = "File";
        }

        [CategoryAttribute("Required"), DisplayName("Name"), PropertyOrder(1), DescriptionAttribute("Name of the Output")]
        public string Name {
            get { return GetAttribute("name"); }
            set { SetAttribute("name", value); }
        }
        [CategoryAttribute("Required"), DisplayName("Node Type"), PropertyOrder(2), DescriptionAttribute("Type of the endpoint node"), ItemsSource(typeof(NodeTypeList))]
        public string TypeData {
            get { return "File"; }
            set { SetType(value); }
        }

        [CategoryAttribute("Required"),
        DisplayName("Directory Path"),
        PropertyOrder(3),
        DescriptionAttribute("Path to the directory to watch")]
        public string Path {
            get {
                return GetAttribute("path");
            }
            set {
                SetAttribute("path", value);
            }
        }
        [CategoryAttribute("Required"), DisplayName("File Filter"), PropertyOrder(4), DescriptionAttribute("File Pattern to Match. (e.g. *.xml)")]
        public string FileFilter {
            get {
                return GetAttribute("fileFilter");
            }
            set {
                SetAttribute("fileFilter", value);
            }
        }
        [CategoryAttribute("Required"), DisplayName("Delete After Send"), PropertyOrder(5), DescriptionAttribute("Delete the source file after the pipeline picks it up")]
        public bool Delete {
            get {
                return GetBoolAttribute("deleteAfterSend");
            }
            set {
                SetAttribute("deleteAfterSend", value);
            }
        }
        [CategoryAttribute("Required"), DisplayName("Buffer Queue"), PropertyOrder(6), DescriptionAttribute("Local MS MQ Queue that is used as an intermediate buffer")]
        public string Buffer {
            get {
                return GetAttribute("bufferQueueName");
            }
            set {
                SetAttribute("bufferQueueName", value);
            }
        }

        [CategoryAttribute("Optional - Transformation"), DisplayName("XSL Transform Style Sheet"), PropertyOrder(1), DescriptionAttribute("XSL StyleSheet to perform a transformation")]
        public string StyleSheet {
            get { return GetAttribute("stylesheet"); }
            set { SetAttribute("stylesheet", value); }
        }

        [CategoryAttribute("Optional - Transformation"), DisplayName("XSL Version"), PropertyOrder(2), DescriptionAttribute("XSLT Version"), ItemsSource(typeof(XSLTypeList))]
        public string XSLType {
            get { return GetAttribute("xslVersion"); }
            set { SetAttribute("xslVersion", value); ; }
        }

    }

    public class FILEOUT : MyPropertyGrid {


        public FILEOUT(XmlNode dataModel, IView view) {
            this._node = dataModel;
            this.view = view;
            this.type = "File";
        }

        [CategoryAttribute("Required"), DisplayName("Name"), PropertyOrder(1), DescriptionAttribute("Name of the Output")]
        public string Name {
            get { return GetAttribute("name"); }
            set { SetAttribute("name", value); }
        }
        [CategoryAttribute("Required"), DisplayName("Node Type"), PropertyOrder(2), DescriptionAttribute("Type of the endpoint node"), ItemsSource(typeof(NodeTypeList))]
        public string TypeData {
            get { return "File"; }
            set { SetType(value); }
        }

        [CategoryAttribute("Required"),
        DisplayName("Output File"),
        PropertyOrder(3),
        DescriptionAttribute("THe path including filename of the file to output to. Successive files have a numeric suffix.")]
        public string Path {
            get {
                return GetAttribute("path");
            }
            set {
                SetAttribute("path", value);
            }
        }


        [CategoryAttribute("Optional - Transformation"), DisplayName("XSL Transform Style Sheet"), PropertyOrder(2), DescriptionAttribute("XSL StyleSheet to perform a transformation")]
        public string StyleSheet {
            get { return GetAttribute("stylesheet"); }
            set { SetAttribute("stylesheet", value); }
        }

        [CategoryAttribute("Optional - Transformation"), DisplayName("XSL Version"), PropertyOrder(3), DescriptionAttribute("XSLT Version"), ItemsSource(typeof(XSLTypeList))]
        public string XSLType {
            get { return GetAttribute("xslVersion"); }
            set { SetAttribute("xslVersion", value); ; }
        }

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
            set {
                SetAttribute("stylesheet", value);
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
                if (_node.InnerXml.Contains("<and>") || _node.InnerXml.Contains("<or>") || _node.InnerXml.Contains("<xor>") || _node.InnerXml.Contains("<not>")  ) {
                    return "Compound Boolean Expression (Select in Tree to Edit)";
                } else {
                    if ((_node.InnerXml.Contains("altqueue") && _node.ChildNodes.Count == 1) || !_node.HasChildNodes ) {
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

        [CategoryAttribute("Required"), DisplayName("Value"), PropertyOrder(1), DescriptionAttribute("The XPath that must exist for the filter to pass")]
        public string Value {
            get { return GetAttribute("value"); }
            set { SetAttribute("value", value); }
        }
    }

}
