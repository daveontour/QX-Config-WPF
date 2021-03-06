﻿using QXEditorModule.Common;
using System.ComponentModel;
using System.Xml;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace QXEditorModule.GridDefinitions
{
    [DisplayName("Test Source Message Generator")]
    public class TESTSOURCE : MyNodeInPropertyGrid
    {

        public TESTSOURCE(XmlNode dataModel, IView view)
        {
            this._node = dataModel;
            this.view = view;
            this.type = "TESTSOURCE";
        }

        [CategoryAttribute("Required"), DisplayName("Node Type"), Browsable(true), PropertyOrder(2), DescriptionAttribute("Type of the endpoint node"), ItemsSource(typeof(NodeTypeList))]
        public string TypeData {
            get { return "Test Source"; }
            set { SetType(value); }
        }


        [Editor(typeof(FileNameSelector), typeof(FileNameSelector))]
        [CategoryAttribute("Optional"), DisplayName("File Path"), Browsable(true), PropertyOrder(1), DescriptionAttribute("Path to the file to use")]
        public string Path {
            get { return GetAttribute("path"); }
            set { SetAttribute("path", value); }
        }

        [CategoryAttribute("Optional"), DisplayName("Test Text"), Browsable(true), PropertyOrder(2), DescriptionAttribute("The text to send. If a file or text is not specified, then an random string will be sent ")]
        public string TestText {
            get { return GetAttribute("testText"); }
            set { SetAttribute("testText", value); }
        }

        [CategoryAttribute("Optional"), DisplayName("Number of Messages"), Browsable(true), PropertyOrder(3), DescriptionAttribute("Maximum Number of Messages to Send (default is unlimited)")]
        public int MaxMess {
            get { return GetIntAttribute("maxMessages"); }
            set { SetAttribute("maxMessages", value); }
        }

        [CategoryAttribute("Optional - Enter one of the below"), DisplayName("Interval"), Browsable(true), PropertyOrder(3), DescriptionAttribute("Fixed interval to use between messages (ms)")]
        public int FixedInterval {
            get { return GetIntAttribute("fixedInterval"); }
            set { SetAttribute("fixedInterval", value); }
        }
        [CategoryAttribute("Optional - Enter one of the below"), DisplayName("Max Wait"), Browsable(true), PropertyOrder(3), DescriptionAttribute("Maximum time for a random interval to use between messages (ms)")]
        public int MaxInterval {
            get { return GetIntAttribute("maxWait"); }
            set { SetAttribute("maxWait", value); }
        }
    }
}
