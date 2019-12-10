using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

using System.Windows.Input;
using QXEditorModule.Common;
using System.ComponentModel;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;


namespace QXEditorModule.GridDefinitions {
    [DisplayName("Sink Output - Black Hole For Messages")]
    public class SINK : MyNodePropertyGrid {


        public SINK(XmlNode dataModel, IView view) {
            this._node = dataModel;
            this.view = view;
            this.type = "SINK";
        }

        [CategoryAttribute("Required"), DisplayName("Node Type"), PropertyOrder(1), Browsable(true), DescriptionAttribute("Type of the endpoint node"), ItemsSource(typeof(NodeTypeList))]
        public string TypeData {
            get { return "SINK"; }
            set { SetType(value); }
        }
    }
}
