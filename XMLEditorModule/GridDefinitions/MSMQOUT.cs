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

namespace WXE.Internal.Tools.ConfigEditor.XMLEditorModule.GridDefinitions {
    [DisplayName("Microsoft MQ Output Node")]
    public class MSMQOUT : MyNodePropertyGrid {


        public MSMQOUT(XmlNode dataModel, IView view) {
            this._node = dataModel;
            this.view = view;
            this.type = "MSMQ";
        }

        [CategoryAttribute("Required"), DisplayName("Node Type"), PropertyOrder(2), Browsable(true), DescriptionAttribute("Type of the endpoint node"), ItemsSource(typeof(NodeTypeList))]
        public string NodeType {
            get { return "Microsoft MQ"; }
            set { SetType(value); }
        }


        [CategoryAttribute("Required"), DisplayName("Queue"), PropertyOrder(3), Browsable(true), DescriptionAttribute("MS MQ Queue Name")]
        public string Queue {
            get { return GetAttribute("queue"); }
            set { SetAttribute("queue", value); }
        }

        [CategoryAttribute("Optional"), DisplayName("Create Queue"), PropertyOrder(1), Browsable(true), DescriptionAttribute("Create the Queue if it doesn't exist")]
        public bool CreatQueue {
            get { return GetBoolAttribute("createQueue"); }
            set { SetAttribute("queue", value); }
        }

        [CategoryAttribute("Optional"), DisplayName("Maximum Messages"), PropertyOrder(2), Browsable(true), DescriptionAttribute("Maximum Number of Messages Allowed in the Queue. (Older messages will be replaced)")]
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
}
