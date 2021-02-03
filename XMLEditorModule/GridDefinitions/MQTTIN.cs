using QXEditorModule.Common;
using System.ComponentModel;
using System.Xml;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace QXEditorModule.GridDefinitions {

    [DisplayName("MQTT Node")]
    public class MQTTIN : MyNodeInPropertyGrid {

        public MQTTIN(XmlNode dataModel, IView view) {
            this._node = dataModel;
            this.view = view;
            this.type = "MQTT";

            if (_node.Name == "input") {
                Hide("NodeTypeOut");
            } else {
                Hide("NodeType");
                Hide("Priority");
            }
        }

        [CategoryAttribute("Required"), DisplayName("Node Type"), Browsable(true), PropertyOrder(2), DescriptionAttribute("Type of the endpoint node"), ItemsSource(typeof(NodeTypeList))]
        public string NodeType {
            get {
                if (_node.Name == "input") {
                    return "MQTT Subscriber";
                } else {
                    return "MQTT Publisher";
                }
            }
            set { SetType(value); }
        }

        [CategoryAttribute("Required"), DisplayName("Node Type"), Browsable(true), PropertyOrder(2), DescriptionAttribute("Type of the endpoint node"), ItemsSource(typeof(NodeTypeListOut))]
        public string NodeTypeOut {
            get {
                if (_node.Name == "input") {
                    return "MQTT Subscriber";
                } else {
                    return "MQTT Publisher";
                }
            }
            set { SetType(value); }
        }

        [RefreshProperties(RefreshProperties.All)]
        [CategoryAttribute("Required"), DisplayName("MQTT Server Type"), ReadOnly(false), Browsable(true), PropertyOrder(3), DescriptionAttribute("The MQTT Server Type"), ItemsSource(typeof(MQTTServerTypeList))]
        public string MQTTServerType {
            get {
                string t = GetAttribute("mqttServerType");
                if (t == "ws") {
                    Hide(new string[] { "MQTTServer", "MQTTServerPort" });
                    Show(new string[] { "MQTTServerURL" });
                    return "Web Services Server";
                } else if (t == "tcp") {
                    Show(new string[] { "MQTTServer", "MQTTServerPort" });
                    Hide(new string[] { "MQTTServerURL" });
                    return "TCP Server";
                } else {
                    Hide(new string[] { "MQTTServer", "MQTTServerPort" });
                    Show(new string[] { "MQTTServerURL" });
                    SetAttribute("mqttServerType", "ws");
                    return "Web Services Server";
                }

            }
            set {
                if (value == "Web Services Server") {
                    Hide(new string[] { "MQTTServer", "MQTTServerPort" });
                    Show(new string[] { "MQTTServerURL" });
                    SetAttribute("mqttServerType", "ws");
                } else if (value == "TCP Server") {
                    Show(new string[] { "MQTTServer", "MQTTServerPort" });
                    Hide(new string[] { "MQTTServerURL" });
                    SetAttribute("mqttServerType", "tcp");
                }
            }
        }

        //                 "Web Services Server", "TCP Server"
        [CategoryAttribute("Required"), DisplayName("MQTT Server"), ReadOnly(false), Browsable(true), PropertyOrder(4), DescriptionAttribute("The connection string for the MQTT host, eg \"broker.hivemq.com:8000/mqtt\"")]
        public string MQTTServerURL {
            get { return GetAttribute("mqttServer"); }
            set { SetAttribute("mqttServer", value); }
        }
        [CategoryAttribute("Required"), DisplayName("MQTT Server"), ReadOnly(false), Browsable(true), PropertyOrder(4), DescriptionAttribute("The MQTT Server Host Name")]
        public string MQTTServer {
            get { return GetAttribute("mqttServer"); }
            set { SetAttribute("mqttServer", value); }
        }
        [CategoryAttribute("Required"), DisplayName("MQTT Server Port"), ReadOnly(false), Browsable(true), PropertyOrder(5), DescriptionAttribute("The MQTT Server Port")]
        public int MQTTServerPort {
            get { return GetIntAttribute("mqttPort"); }
            set { SetAttribute("mqttPort", value); }
        }
        [CategoryAttribute("Required"), DisplayName("MQTT Topic"), ReadOnly(false), Browsable(true), PropertyOrder(6), DescriptionAttribute("The MQTT Topic")]
        public string MQTTTopic {
            get { return GetAttribute("mqttTopic"); }
            set { SetAttribute("mqttTopic", value); }
        }
    }
}
