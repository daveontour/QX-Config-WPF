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
    [DisplayName("File Input")]
    public class FILEIN : MyNodeInPropertyGrid {

        public FILEIN(XmlNode dataModel, IView view) {
            this._node = dataModel;
            this.view = view;
            this.type = "File";
        }

        [CategoryAttribute("Required"), DisplayName("Node Type"), Browsable(true), PropertyOrder(2), DescriptionAttribute("Type of the endpoint node"), ItemsSource(typeof(NodeTypeList))]
        public string TypeData {
            get { return "File"; }
            set { SetType(value); }
        }

        [CategoryAttribute("Required"), DisplayName("Directory Path"), Browsable(true), PropertyOrder(3), DescriptionAttribute("Path to the directory to watch")]
        public string Path {
            get { return GetAttribute("path"); }
            set { SetAttribute("path", value); }
        }
        [CategoryAttribute("Required"), DisplayName("File Filter"), PropertyOrder(4), Browsable(true), DescriptionAttribute("File Pattern to Match. (e.g. *.xml)")]
        public string FileFilter {
            get {return GetAttribute("fileFilter"); }
            set { SetAttribute("fileFilter", value); }
        }
        [CategoryAttribute("Required"), DisplayName("Delete After Send"), PropertyOrder(5), Browsable(true), DescriptionAttribute("Delete the source file after the pipeline picks it up")]
        public bool Delete {
            get {return GetBoolAttribute("deleteAfterSend");}
            set {SetAttribute("deleteAfterSend", value); }
        }
        [CategoryAttribute("Required"), DisplayName("Buffer Queue"), PropertyOrder(6), Browsable(true), DescriptionAttribute("Local MS MQ Queue that is used as an intermediate buffer")]
        public string Buffer {
            get { return GetAttribute("bufferQueueName");}
            set { SetAttribute("bufferQueueName", value);}
        }
    }
}
