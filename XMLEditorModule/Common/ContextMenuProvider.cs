using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using System.Xml;

namespace WXE.Internal.Tools.ConfigEditor.Common
{
   
    public class ContextMenuProvider
    {
        public readonly Dictionary<ContextMenuType, MenuItem> ContextMenus = new Dictionary<ContextMenuType, MenuItem>();

        public ContextMenuProvider()
        {
            ContextMenus.Add(ContextMenuType.Copy, new MenuItem { Header = "Copy"});
            ContextMenus.Add(ContextMenuType.Paste, new MenuItem { Header = "Paste" });
            ContextMenus.Add(ContextMenuType.Delete, new MenuItem { Header = "Delete" });
            ContextMenus.Add(ContextMenuType.AddPipe, new MenuItem { Header = "Add Pipe" });
            ContextMenus.Add(ContextMenuType.Add, new MenuItem { Header = "Add" });
            ContextMenus.Add(ContextMenuType.AddInput, new MenuItem { Header = "Add Input Node" });
            ContextMenus.Add(ContextMenuType.AddOutput, new MenuItem { Header = "Add Output Node" });
            ContextMenus.Add(ContextMenuType.AddFilter, new MenuItem { Header = "Add Filter" });
            ContextMenus.Add(ContextMenuType.AddExpression, new MenuItem { Header = "Add Boolean Expression" });
            ContextMenus.Add(ContextMenuType.AddMonitor, new MenuItem { Header = "Add Monitor Queue" });
            ContextMenus.Add(ContextMenuType.AddLogger, new MenuItem { Header = "Add Logger Queue" });
            ContextMenus.Add(ContextMenuType.AddNamespace, new MenuItem { Header = "Add Namespace" });
        }
    }
}
