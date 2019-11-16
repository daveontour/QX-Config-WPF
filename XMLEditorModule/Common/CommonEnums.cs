using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

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
        AddDataFilter,
        AddXPathExists
    }

    public interface IView {
        void DrawQXConfig();
        void HightLightCanvas(XmlNode node);
        void UpdateParamBindings(string param);
        void MSMQSource(XmlNode node);
        void MQSource(XmlNode node);
        void UpdateSelectedNodeCanvas(XmlNode node);
        void UpdateSelectedPipeCanvas(XmlNode node);
        void ChangeElementType(string value);
        bool CanChangeElementType(string value);
        void ChangeFilterType(string value);
    }
}
