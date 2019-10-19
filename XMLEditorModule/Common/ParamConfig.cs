using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WXE.Internal.Tools.ConfigEditor.XMLEditorModule.Common {
    class ParamConfig {

        public static readonly int InputRequired = 0b0001;
        public static readonly int InputVis = 0b0010;
        public static readonly int OutputRequired = 0b0100;
        public static readonly int OutputVis = 0b1000;
        public static readonly int None = 0b0000;

        public static readonly Dictionary<string, Dictionary<string, int>> ParamDict = new Dictionary<string, Dictionary<string, int>>() {
            {
                "MSMQ", new Dictionary<string, int>() {
                    {"Queue", InputRequired | InputVis | OutputRequired | OutputVis  },
                    {"Name", InputRequired | InputVis | OutputRequired | OutputVis  },
                    {"Priority",  InputVis },
                    {"StyleSheet",  InputVis |  OutputVis  },
                    {"XSLVersion",  InputVis |  OutputVis  },
                    {"Connection",  None  },
              }
            },
            {
                "MQ", new Dictionary<string, int>() {
                    {"Queue",  InputVis | OutputRequired | OutputVis  },
                    {"Name", InputRequired | InputVis | OutputRequired | OutputVis  },
                    {"Priority",  InputVis },
                    {"StyleSheet",  InputVis |  OutputVis  },
                    {"XSLVersion",  InputVis |  OutputVis  },
                    {"Connection",   InputRequired | InputVis | OutputRequired | OutputVis   },
             }
            },
            {
                "Kafka", new Dictionary<string, int>() {
                    {"Queue", None  },
                    {"Name", InputRequired | InputVis | OutputRequired | OutputVis  },
                    {"Priority",  InputVis },
                    {"StyleSheet",  InputVis |  OutputVis  },
                    {"XSLVersion",  InputVis |  OutputVis  },
                    {"Connection",   InputRequired | InputVis | OutputRequired | OutputVis  },
              }
            }
        };
    }
}
