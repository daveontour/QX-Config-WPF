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
    [DisplayName("HTTP Post Input Node")]
    public class HTTPIN : MyNodeInPropertyGrid {


        public HTTPIN(XmlNode dataModel, IView view) {
            this._node = dataModel;
            this.view = view;
            this.type = "HTTP Post";
        }

        [CategoryAttribute("Required"), DisplayName("Node Type"), PropertyOrder(1), Browsable(true), DescriptionAttribute("Type of the endpoint node"), ItemsSource(typeof(NodeTypeList))]
        public string TypeData {
            get { return "HTTP Post"; }
            set { SetType(value); }
        }

        [CategoryAttribute("Required"), DisplayName("HTTP Post URL"), PropertyOrder(2), Browsable(true), DescriptionAttribute("The URL on the local server for messgaes to be posted to. Messages will be consumed by the pipe when posted")]
        public string URL {
            get {return GetAttribute("postURL"); }
            set {SetAttribute("postURL", value); }
        }
    }
}
