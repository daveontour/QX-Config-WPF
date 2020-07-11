using QXEditorModule.Common;
using System.ComponentModel;
using System.Xml;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;


namespace QXEditorModule.GridDefinitions
{
    [DisplayName("File Input Node")]
    public class FILEIN : MyNodeInPropertyGrid
    {

        public FILEIN(XmlNode dataModel, IView view)
        {
            this._node = dataModel;
            this.view = view;
            this.type = "File";
        }

        [CategoryAttribute("Required"), DisplayName("Node Type"), Browsable(true), PropertyOrder(2), DescriptionAttribute("Type of the endpoint node"), ItemsSource(typeof(NodeTypeList))]
        public string TypeData {
            get { return "File"; }
            set { SetType(value); }
        }

        [Editor(typeof(FolderNameSelector), typeof(FolderNameSelector))]
        [CategoryAttribute("Required"), DisplayName("Directory Path"), Browsable(true), PropertyOrder(3), DescriptionAttribute("Path to the directory to watch")]
        public string Path {
            get { return GetAttribute("path"); }
            set { SetAttribute("path", value); }
        }
        [CategoryAttribute("Required"), DisplayName("File Filter"), PropertyOrder(4), Browsable(true), DescriptionAttribute("File Pattern to Match. (e.g. *.xml)")]
        public string FileFilter {
            get {
                string val = GetAttribute("fileFilter");
                if (val == null || val == "")
                {
                    val = "*.*";
                    SetAttribute("fileFilter", val);
                }
                return val;
            }
            set { SetAttribute("fileFilter", value); }
        }
        [CategoryAttribute("Required"), DisplayName("Delete After Send"), PropertyOrder(6), Browsable(true), DescriptionAttribute("Delete the source file after the pipeline picks it up")]
        public bool Delete {
            get { return GetBoolAttribute("deleteAfterSend"); }
            set { SetAttribute("deleteAfterSend", value); }
        }
    }
}
