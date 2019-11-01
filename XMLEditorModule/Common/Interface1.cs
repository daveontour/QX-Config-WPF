using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace WXE.Internal.Tools.ConfigEditor.XMLEditorModule.Common {
    public interface IView {
        void DrawQXConfig();
        void HightLightCanvas(XmlNode node);
    }
}
