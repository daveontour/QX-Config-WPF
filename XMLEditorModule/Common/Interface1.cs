using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace WXE.Internal.Tools.ConfigEditor.XMLEditorModule.Common {
    public interface IView {
        void DrawQXConfig();
        void HightLightCanvas(XmlNode node);
        void UpdateParamBindings(string param);

        void MSMQSource(XmlNode node);

        void MQSource(XmlNode node);

        void UpdateSelectedNodecanvas(XmlNode node);
        void UpdateSelectedPipeCanvas(XmlNode node);
        void ChangeElementType(string value);
        bool CanChangeElementType(string value);
    }
}
