using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WXE.Internal.Tools.ConfigEditor.Common
{
   

    public enum ContextMenuType
    {
        Cut,
        Copy,
        Paste,
        Delete,
        Add,
        AddPipe,
        AddLogger,
        AddMonitor,
        AddNamespace,
        AddInput,
        AddOutput,
        AddFilter,
        AddExpression,
        AddAltQueue,
        AddDataFilter
    }

    public enum ParamState {
        InputRequired = 0b0001,
        InputVis = 0b0010,
        OutputRequired = 0b0100,
        OutputVis = 0b1000,
    }


}
