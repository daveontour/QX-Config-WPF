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
                    {"queue", InputRequired | InputVis | OutputRequired | OutputVis  },
                    {"name", InputRequired | InputVis | OutputRequired | OutputVis  },
                    {"priority",  InputVis },
                    {"stylesheet",  InputVis |  OutputVis  },
                    {"xslVersion",  InputVis |  OutputVis  },
                    {"connection",  None  },
                    {"createQueue", OutputVis},
                    {"retryInterval", OutputVis},
                    {"getTimeout", OutputVis},
                    {"pause", OutputVis},
                    {"maxRetry", OutputVis},
                    {"undeliverableQueue", OutputVis},
                    {"bufferQueueName", OutputVis},
                    {"xpathDestination", None},
                    {"xpathContentDestination", None},
             }
            },
            {
                "MQ", new Dictionary<string, int>() {
                    {"queue",  InputVis | OutputRequired | OutputVis  },
                    {"name", InputRequired | InputVis | OutputRequired | OutputVis  },
                    {"priority",  InputVis },
                    {"stylesheet",  InputVis |  OutputVis  },
                    {"xslVersion",  InputVis |  OutputVis  },
                    {"connection",   InputRequired | InputVis | OutputRequired | OutputVis   },
                    {"createQueue", None},
                    {"retryInterval", OutputVis},
                    {"getTimeout", OutputVis},
                    {"pause", OutputVis},
                    {"maxRetry", OutputVis},
                    {"undeliverableQueue", OutputVis},
                    {"bufferQueueName", OutputVis},
                    {"xpathDestination", None},
                    {"xpathContentDestination", None},
           }
            },
            {
                "Kafka", new Dictionary<string, int>() {
                    {"queue", None  },
                    {"name", InputRequired | InputVis | OutputRequired | OutputVis  },
                    {"priority",  InputVis },
                    {"stylesheet",  InputVis |  OutputVis  },
                    {"xslVersion",  InputVis |  OutputVis  },
                    {"connection",   InputRequired | InputVis | OutputRequired | OutputVis  },
                    {"createQueue", None},
                    {"retryInterval", OutputVis},
                    {"getTimeout", OutputVis},
                    {"pause", OutputVis},
                    {"maxRetry", OutputVis},
                    {"undeliverableQueue", OutputVis},
                    {"bufferQueueName", OutputVis},
                    {"xpathDestination", OutputVis},
                    {"xpathContentDestination", OutputVis},
             }
            }
        };
    }
}
