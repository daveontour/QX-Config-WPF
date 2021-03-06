﻿using QXEditorModule.Common;
using System.ComponentModel;
using System.Reflection;
using System.Xml;
using Xceed.Wpf.Toolkit;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace QXEditorModule.GridDefinitions
{
    [DisplayName("Pipe Connecting Input and Output")]
    public class PIPE : MyPropertyGrid
    {
        public PIPE(XmlNode dataModel, IView view)
        {
            this._node = dataModel;
            this.view = view;
        }

        [CategoryAttribute("Required"), DisplayName("Name"), Browsable(true), PropertyOrder(1), DescriptionAttribute("Descriptive name of the pipe")]
        public string Name {
            get { return GetAttribute("name"); }
            set {
                SetAttribute("name", value);
                view.UpdateSelectedPipeCanvas(_node);
            }
        }

        [CategoryAttribute("Required"), DisplayName("Number of Intances"), Browsable(true), PropertyOrder(2), DescriptionAttribute("Number of intances of this Pipeline to run")]
        public int Instances {
            get { return GetIntAttribute("numInstances"); }
            set {
                if (value > 1)
                {
                    MessageBox.Show("Make sure you know what you're doing!", "Multi Thread Warning");
                }
                SetAttribute("numInstances", value);
            }
        }
        [RefreshProperties(RefreshProperties.All)]
        [CategoryAttribute("Optional"), DisplayName("Max Msgs/Min"), Browsable(true), PropertyOrder(1), DescriptionAttribute("Maximum Number of Messages Per Minute (-1 for unlimited)")]
        public int MessPerMinute {
            get {
                int value = GetIntAttribute("maxMsgPerMinute");
                return value;
            }
            set {
                if (value < -1)
                {
                    SetAttribute("maxMsgPerMinute", -1);
                }
                else if (value > 250)
                {
                    SetAttribute("maxMsgPerMinute", 250);
                    SetAttribute("maxMsgPerMinuteProfile", null);
                }
                else
                {
                    SetAttribute("maxMsgPerMinute", value);
                    SetAttribute("maxMsgPerMinuteProfile", null);
                }
            }
        }
        [RefreshProperties(RefreshProperties.All)]
        [CategoryAttribute("Optional"), DisplayName("Max Msgs/Min Profile"), Browsable(true), PropertyOrder(2), DescriptionAttribute("Maximum Number of Messages Per Minute Profile e.g '0:10,60:40,120:10'. The string is a comma seperated list of value pairs, where the values pairs are: TimeFromStartInMinutes:MaxMessagesPerMin")]
        public string MessPerMinuteProfile {
            get {
                string value = GetAttribute("maxMsgPerMinuteProfile");
                return value;
            }
            set {
                SetAttribute("maxMsgPerMinuteProfile", value);
                SetAttribute("maxMsgPerMinute", -1);
            }
        }

        [CategoryAttribute("Optional"), DisplayName("Output Isolation"), Browsable(true), PropertyOrder(4), DescriptionAttribute("Isolate distribution of outputs from each other")]
        public bool OutputIsolation {
            get { return GetBoolAttribute("outputIsolation"); }
            set { SetAttribute("outputIsolation", value); }
        }

        [CategoryAttribute("Optional"), DisplayName("Distribution"), Browsable(true), PropertyOrder(3), DescriptionAttribute("The type of ditribution to the output nodes"), ItemsSource(typeof(OutputTypeList))]
        public string OutputDistribution {
            get {
                string dist = GetAttribute("distribution");
                if (dist == "all" || dist == null || dist == "")
                {
                    return "Distribute to All";
                }
                if (dist == "roundRobin")
                {
                    return "Round Robin";
                }
                if (dist == "random")
                {
                    return "Random";
                }

                return "Distribute to All";
            }
            set {
                if (value == "Distribute to All") SetAttribute("distribution", null);
                if (value == "Round Robin") SetAttribute("distribution", "roundRobin");
                if (value == "Random") SetAttribute("distribution", "random");
            }
        }

        [RefreshProperties(RefreshProperties.All)]
        [CategoryAttribute("Required"), DisplayName("Pipe Input Queue Name"), Browsable(true), PropertyOrder(8), DescriptionAttribute("An 'internal' queue is used as an intermediary between the input nodes and the pipe. This is the queue to use")]
        public string InputQueueName {
            get {

                string val = GetAttribute("pipeInputQueueName");
                if (val == null || val == "")
                {
                    val = @".\private$\Enter_Queue_Name_Here";
                }
                return val;
            }
            set {
                SetAttribute("pipeInputQueueName", value);
            }
        }

        [RefreshProperties(RefreshProperties.All)]
        [CategoryAttribute("Optional"), DisplayName("Context Aware Pipe"), Browsable(true), PropertyOrder(20), DescriptionAttribute("Enable this Pipe to be Context Aware")]
        public bool ContextAware {
            get {

                bool value = GetBoolAttribute("contextAware");


                if (!value)
                {
                    SetAttribute("useMessageAsKey", "");
                    SetAttribute("contextCacheKeyXPath", "");
                    SetAttribute("contextCacheExpiry", "");
                    SetAttribute("discardInCache", "");
                }

                ShowHide("ContextKey", value);
                ShowHide("UseMessageAsKey", value);
                ShowHide("ContextExpiry", value);
                ShowHide("DiscardCacheItems", value);
                ShowHide("FirstOnly", value);
                ShowHide("MostRecentOnly", value);
                ShowHide("ContextStatInterval", value);

                return value;
            }
            set {

                SetAttribute("useMessageAsKey", "");
                SetAttribute("contextCacheKeyXPath", "");
                SetAttribute("contextCacheExpiry", "");
                SetAttribute("discardInCache", "");
                SetAttribute("contextAware", value);

                ShowHide("ContextKey", value);
                ShowHide("UseMessageAsKey", value);
                ShowHide("ContextExpiry", value);
                ShowHide("DiscardCacheItems", value);
                ShowHide("FirstOnly", value);
                ShowHide("MostRecentOnly", value);
                ShowHide("ContextStatInterval", value);

            }
        }

        [RefreshProperties(RefreshProperties.All)]
        [CategoryAttribute("Optional - Temporal Context Awareness"), DisplayName("Use Message Hash As Key"), Browsable(true), PropertyOrder(1), DescriptionAttribute("Use a SHA256 hash of the entire message for the Context Cache Key (Duplicate Messages)")]
        public bool UseMessageAsKey {
            get {
                bool value = GetBoolAttribute("useMessageAsKey");
                PropertyDescriptor descriptor = TypeDescriptor.GetProperties(this.GetType())["ContextKey"];
                BrowsableAttribute theDescriptorBrowsableAttribute = (BrowsableAttribute)descriptor.Attributes[typeof(BrowsableAttribute)];
                FieldInfo isBrowsable = theDescriptorBrowsableAttribute.GetType().GetField("Browsable", BindingFlags.IgnoreCase | BindingFlags.NonPublic | BindingFlags.Instance);

                if (value)
                {
                    isBrowsable.SetValue(theDescriptorBrowsableAttribute, false);
                    SetAttribute("contextCacheKeyXPath", "");
                }
                else
                {
                    isBrowsable.SetValue(theDescriptorBrowsableAttribute, true);
                }
                return value;
            }
            set {
                SetAttribute("useMessageAsKey", value);
                //SetAttribute("maxMsgPerMinute", -1);


                PropertyDescriptor descriptor = TypeDescriptor.GetProperties(this.GetType())["ContextKey"];
                BrowsableAttribute theDescriptorBrowsableAttribute = (BrowsableAttribute)descriptor.Attributes[typeof(BrowsableAttribute)];
                FieldInfo isBrowsable = theDescriptorBrowsableAttribute.GetType().GetField("Browsable", BindingFlags.IgnoreCase | BindingFlags.NonPublic | BindingFlags.Instance);

                if (value)
                {
                    isBrowsable.SetValue(theDescriptorBrowsableAttribute, false);
                    SetAttribute("contextCacheKeyXPath", "");
                }
                else
                {
                    isBrowsable.SetValue(theDescriptorBrowsableAttribute, true);
                }

                if (ContextExpiry <= 0)
                {
                    ContextExpiry = 10;
                }
            }
        }

        [RefreshProperties(RefreshProperties.All)]
        [CategoryAttribute("Optional - Temporal Context Awareness"), DisplayName("Context Key"), Browsable(true), PropertyOrder(2), DescriptionAttribute("XPath for the Context Key. If you want to match all messages, enter '*' ")]
        public string ContextKey {
            get { return GetAttribute("contextCacheKeyXPath"); }
            set {
                //SetAttribute("maxMsgPerMinute", "");
                SetAttribute("contextCacheKeyXPath", value);
                if (ContextExpiry <= 0)
                {
                    ContextExpiry = 10;
                }
            }
        }


        [CategoryAttribute("Optional - Temporal Context Awareness"), DisplayName("Context Cache Expiry"), Browsable(true), PropertyOrder(3), DescriptionAttribute("How long items remain in the context cache which also determines the rate of messages meeting the key will be sent")]
        public int ContextExpiry {
            get { return GetIntAttribute("contextCacheExpiry"); }
            set {
                if (value <= 0)
                {
                    value = 1;
                }
                SetAttribute("contextCacheExpiry", value);
            }
        }

        [CategoryAttribute("Optional - Temporal Context Awareness"), DisplayName("Discard Messages"), Browsable(true), PropertyOrder(4), DescriptionAttribute("Discard Messages if they already exist in the  Context Cache")]
        public bool DiscardCacheItems {
            get { return GetBoolAttribute("discardInCache"); }
            set { SetAttribute("discardInCache", value); }
        }
        [CategoryAttribute("Optional - Temporal Context Awareness"), DisplayName("Most Recent Only"), Browsable(true), PropertyOrder(5), DescriptionAttribute("Only send the most recent message each time")]
        public bool MostRecentOnly {
            get { return GetBoolAttribute("mostRecentOnly"); }
            set { SetAttribute("mostRecentOnly", value); }
        }
        [CategoryAttribute("Optional - Temporal Context Awareness"), DisplayName("Cache First Only"), Browsable(true), PropertyOrder(6), DescriptionAttribute("Only apply Context Cache to first occurance")]
        public bool FirstOnly {
            get { return GetBoolAttribute("firstOnly"); }
            set { SetAttribute("firstOnly", value); }
        }

        [CategoryAttribute("Optional - Temporal Context Awareness"), DisplayName("Context Stats Interval (ms)"), Browsable(true), PropertyOrder(10), DescriptionAttribute("How often the usage context stats are flushed to the log")]
        public int ContextStatInterval {
            get { return GetIntAttribute("contextStatsInterval"); }
            set {
                SetAttribute("contextStatsInterval", value);
            }
        }
    }
}
