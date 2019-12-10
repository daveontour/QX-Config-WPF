using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

using System.Windows.Input;
using QXEditorModule.Common;
using System.ComponentModel;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using System.Globalization;
using System.Reflection;


namespace QXEditorModule.GridDefinitions {
    [DisplayName("IBM MQ Output Node")]
    public class MQOUT : MQIN {


        public MQOUT(XmlNode dataModel, IView view) : base(dataModel, view) {
            PropertyDescriptor descriptor = TypeDescriptor.GetProperties(this.GetType())["Priority"];
            BrowsableAttribute theDescriptorBrowsableAttribute = (BrowsableAttribute)descriptor.Attributes[typeof(BrowsableAttribute)];
            FieldInfo isBrowsable = theDescriptorBrowsableAttribute.GetType().GetField("Browsable", BindingFlags.IgnoreCase | BindingFlags.NonPublic | BindingFlags.Instance);

            // Set the Descriptor's "Browsable" Attribute
            isBrowsable.SetValue(theDescriptorBrowsableAttribute, false);
        }

        [CategoryAttribute("Optional"), DisplayName("Maximum Messages"), PropertyOrder(3), ReadOnly(false), Browsable(true), DescriptionAttribute("Maximum Number of Messages Allowed in the Queue. (Older messages will be replaced)")]
        public int MaxMessages {
            get { return GetIntAttribute("maxMessages"); }
            set {
                if (value <= -1) {
                    value = -1;
                }
                SetAttribute("maxMessages", value);
            }
        }

        [CategoryAttribute("Optional"), DisplayName("Undeliverable Queue"), PropertyOrder(4), ReadOnly(false), Browsable(true), DescriptionAttribute("Local MSMQ queue to send messages if they cannot be delivered")]
        public string Undeliverable {
            get { return GetAttribute("undeliverableQueue"); }
            set {SetAttribute("undeliverableQueue", value);
            }
        }

        
    }
}
