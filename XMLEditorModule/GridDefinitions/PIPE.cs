using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

using System.Windows.Input;
using WXE.Internal.Tools.ConfigEditor.Common;
using System.ComponentModel;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using WXE.Internal.Tools.ConfigEditor.XMLEditorModule.Common;
using System.Globalization;
using System.Reflection;
using WXE.Internal.Tools.ConfigEditor.XMLEditorModule.Views;
using Xceed.Wpf.Toolkit;

namespace WXE.Internal.Tools.ConfigEditor.XMLEditorModule.GridDefinitions {
    [DisplayName("Pipe Connecting Input and Output")]
    public class PIPE : MyPropertyGrid {

       
        public PIPE(XmlNode dataModel, IView view) {
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
                if (value > 1) {
                    MessageBox.Show("Make sure you know what you're doing!", "Multi Thread Warning");
                }
                SetAttribute("numInstances", value);
            }
        }

        [CategoryAttribute("Optional"), DisplayName("Max Msgs/Min"), Browsable(true), PropertyOrder(1), DescriptionAttribute("Maximum Number of Messages Per Minute (-1 for unlimited)")]
        public int MessPerMinute {
            get { 
                int value = GetIntAttribute("maxMsgPerMinute");
                return value; 
                }
            set {
                if (value < -1) {
                    SetAttribute("maxMsgPerMinute", -1);
                } else if (value > 250) {
                    SetAttribute("maxMsgPerMinute", 250);
                } else {
                    SetAttribute("maxMsgPerMinute", value);
                }
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
                if (dist == "all" || dist == null || dist == "") {
                    return "Distribute to All";
                }
                if (dist == "roundRobin") {
                    return "Round Robin";
                }
                if (dist == "random") {
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
        [CategoryAttribute("Optional"), DisplayName("Context Aware Pipe"), Browsable(true), PropertyOrder(7), DescriptionAttribute("Enable this Pipe to be Context Aware")]
        public bool ContextAware {
            get {

                bool value = GetBoolAttribute("contextAware");
                
                
                if (!value) {
                    SetAttribute("useMessageAsKey", "");
                    SetAttribute("contextCacheKeyXPath", "");
                    SetAttribute("contextCacheExpiry", "");
               //     SetAttribute("maxMsgPerMinute", "");
                    SetAttribute("discardInCache", "");
                }


                PropertyDescriptor descriptor = TypeDescriptor.GetProperties(this.GetType())["ContextKey"];
                BrowsableAttribute theDescriptorBrowsableAttribute = (BrowsableAttribute)descriptor.Attributes[typeof(BrowsableAttribute)];
                FieldInfo isBrowsable = theDescriptorBrowsableAttribute.GetType().GetField("Browsable", BindingFlags.IgnoreCase | BindingFlags.NonPublic | BindingFlags.Instance);
                isBrowsable.SetValue(theDescriptorBrowsableAttribute, value);

                descriptor = TypeDescriptor.GetProperties(this.GetType())["UseMessageAsKey"];
                theDescriptorBrowsableAttribute = (BrowsableAttribute)descriptor.Attributes[typeof(BrowsableAttribute)];
                isBrowsable = theDescriptorBrowsableAttribute.GetType().GetField("Browsable", BindingFlags.IgnoreCase | BindingFlags.NonPublic | BindingFlags.Instance);
                isBrowsable.SetValue(theDescriptorBrowsableAttribute, value);

                descriptor = TypeDescriptor.GetProperties(this.GetType())["ContextExpiry"];
                theDescriptorBrowsableAttribute = (BrowsableAttribute)descriptor.Attributes[typeof(BrowsableAttribute)];
                isBrowsable = theDescriptorBrowsableAttribute.GetType().GetField("Browsable", BindingFlags.IgnoreCase | BindingFlags.NonPublic | BindingFlags.Instance);
                isBrowsable.SetValue(theDescriptorBrowsableAttribute, value);

                descriptor = TypeDescriptor.GetProperties(this.GetType())["DiscardCacheItems"];
                theDescriptorBrowsableAttribute = (BrowsableAttribute)descriptor.Attributes[typeof(BrowsableAttribute)];
                isBrowsable = theDescriptorBrowsableAttribute.GetType().GetField("Browsable", BindingFlags.IgnoreCase | BindingFlags.NonPublic | BindingFlags.Instance);
                isBrowsable.SetValue(theDescriptorBrowsableAttribute, value);

                return value;
            }
            set {

                    SetAttribute("useMessageAsKey", "");
                    SetAttribute("contextCacheKeyXPath", "");
                    SetAttribute("contextCacheExpiry", "");
               //     SetAttribute("maxMsgPerMinute", "");
                    SetAttribute("discardInCache", "");

                PropertyDescriptor descriptor = TypeDescriptor.GetProperties(this.GetType())["ContextKey"];
                BrowsableAttribute theDescriptorBrowsableAttribute = (BrowsableAttribute)descriptor.Attributes[typeof(BrowsableAttribute)];
                FieldInfo isBrowsable = theDescriptorBrowsableAttribute.GetType().GetField("Browsable", BindingFlags.IgnoreCase | BindingFlags.NonPublic | BindingFlags.Instance);

                // Set the Descriptor's "Browsable" Attribute
                isBrowsable.SetValue(theDescriptorBrowsableAttribute, value);

                descriptor = TypeDescriptor.GetProperties(this.GetType())["UseMessageAsKey"];
                theDescriptorBrowsableAttribute = (BrowsableAttribute)descriptor.Attributes[typeof(BrowsableAttribute)];
                isBrowsable = theDescriptorBrowsableAttribute.GetType().GetField("Browsable", BindingFlags.IgnoreCase | BindingFlags.NonPublic | BindingFlags.Instance);
                isBrowsable.SetValue(theDescriptorBrowsableAttribute, value);

                descriptor = TypeDescriptor.GetProperties(this.GetType())["ContextExpiry"];
                theDescriptorBrowsableAttribute = (BrowsableAttribute)descriptor.Attributes[typeof(BrowsableAttribute)];
                isBrowsable = theDescriptorBrowsableAttribute.GetType().GetField("Browsable", BindingFlags.IgnoreCase | BindingFlags.NonPublic | BindingFlags.Instance);
                isBrowsable.SetValue(theDescriptorBrowsableAttribute, value);

                descriptor = TypeDescriptor.GetProperties(this.GetType())["DiscardCacheItems"];
                theDescriptorBrowsableAttribute = (BrowsableAttribute)descriptor.Attributes[typeof(BrowsableAttribute)];
                isBrowsable = theDescriptorBrowsableAttribute.GetType().GetField("Browsable", BindingFlags.IgnoreCase | BindingFlags.NonPublic | BindingFlags.Instance);
                isBrowsable.SetValue(theDescriptorBrowsableAttribute, value);

                SetAttribute("contextAware", value);
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

                if (value) {
                    isBrowsable.SetValue(theDescriptorBrowsableAttribute, false);
                    SetAttribute("contextCacheKeyXPath", "");
                } else {
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

                if (value) {
                    isBrowsable.SetValue(theDescriptorBrowsableAttribute, false);
                    SetAttribute("contextCacheKeyXPath", "");
                } else {
                    isBrowsable.SetValue(theDescriptorBrowsableAttribute, true);
                }

                if (ContextExpiry <= 0) {
                    ContextExpiry = 10;
                }
            }
        }

        [RefreshProperties(RefreshProperties.All)]
        [CategoryAttribute("Optional - Temporal Context Awareness"), DisplayName("Context Key"), Browsable(true), PropertyOrder(2), DescriptionAttribute("XPath for the Context Key")]
        public string ContextKey {
            get { return GetAttribute("contextCacheKeyXPath"); }
            set {
                //SetAttribute("maxMsgPerMinute", "");
                SetAttribute("contextCacheKeyXPath", value);
                if (ContextExpiry <= 0) {
                    ContextExpiry = 10;
                }
            }
        }

        [CategoryAttribute("Optional - Temporal Context Awareness"), DisplayName("Context Cache Expiry"), Browsable(true), PropertyOrder(3), DescriptionAttribute("How long items remain in the context cache which also determines the rate of messages meeting the key will be sent")]
        public int ContextExpiry {
            get { return GetIntAttribute("contextCacheExpiry"); }
            set {
                if (value <= 0) {
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
    }
}
