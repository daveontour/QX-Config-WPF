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
    public class FILEOUT : MyNodePropertyGrid {


        public FILEOUT(XmlNode dataModel, IView view) {
            this._node = dataModel;
            this.view = view;
            this.type = "File";
        }

        [CategoryAttribute("Required"), DisplayName("Node Type"), PropertyOrder(2), DescriptionAttribute("Type of the endpoint node"), ItemsSource(typeof(NodeTypeList))]
        public string TypeData {
            get { return "File"; }
            set { SetType(value); }
        }

        [CategoryAttribute("Required"), DisplayName("Output File"), PropertyOrder(3),  DescriptionAttribute("The path including filename of the file to output to. Successive files have a numeric suffix.")]
        public string Path {
            get {return GetAttribute("path"); }
            set {SetAttribute("path", value); }
        }
    }
}
