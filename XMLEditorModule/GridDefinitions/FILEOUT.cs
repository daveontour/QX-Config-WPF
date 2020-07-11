using QXEditorModule.Common;
using System.ComponentModel;
using System.IO;
using System.Xml;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;


namespace QXEditorModule.GridDefinitions
{
    [DisplayName("File Output Node")]
    public class FILEOUT : MyNodePropertyGrid
    {


        public FILEOUT(XmlNode dataModel, IView view)
        {
            this._node = dataModel;
            this.view = view;
            this.type = "File";
        }

        [CategoryAttribute("Required"), DisplayName("Node Type"), Browsable(true), PropertyOrder(2), DescriptionAttribute("Type of the endpoint node"), ItemsSource(typeof(NodeTypeListOut))]
        public string TypeData {
            get { return "File"; }
            set { SetType(value); }
        }

        [Editor(typeof(FolderNameSelector), typeof(FolderNameSelector))]
        [CategoryAttribute("Required"), DisplayName("Output File"), Browsable(true), PropertyOrder(3), DescriptionAttribute("The path including filename of the file to output to. Successive files have a numeric suffix.")]
        public string Path {
            get { return GetAttribute("path"); }
            set {
                if (Directory.Exists(value))
                {
                    Path = value + "\\filename";
                }
                else
                {
                    SetAttribute("path", value);
                }
            }
        }
    }
}
