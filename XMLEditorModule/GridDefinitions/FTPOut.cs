using QXEditorModule.Common;
using System.ComponentModel;
using System.Xml;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace QXEditorModule.GridDefinitions {
    [DisplayName("FTP - Send message to FTP destination")]
    public class FTPOut : MyNodePropertyGrid {


        public FTPOut(XmlNode dataModel, IView view) {
            this._node = dataModel;
            this.view = view;
            this.type = "FTP";
        }

        [CategoryAttribute("Required"), DisplayName("Node Type"), PropertyOrder(1), Browsable(true), DescriptionAttribute("Type of the endpoint node"), ItemsSource(typeof(NodeTypeListOut))]
        public string TypeData {
            get { return "FTP"; }
            set { SetType(value); }
        }

        [DisplayName("FTP URL"), ReadOnly(false), Browsable(true), PropertyOrder(31), DescriptionAttribute("The FTP Destination to send the message to, including filename.")]
        public string FTPURL {
            get { return GetAttribute("ftpURL"); }
            set { SetAttribute("ftpURL", value); }
        }

        [DisplayName("Use FTPS"), ReadOnly(false), Browsable(true), PropertyOrder(32), DescriptionAttribute("Use SSL connection. (Probably needs to be set)")]
        public bool SMTPSSL {
            get { return GetBoolDefaultFalseAttribute(_node, "useFTPS"); }
            set { SetAttribute("useFTPS", value); }
        }

        [DisplayName("Directory"), ReadOnly(false), Browsable(true), PropertyOrder(34), DescriptionAttribute("Directory relative to the root of the FTP Server")]
        public string Directory {
            get { return GetAttribute("directory"); }
            set { SetAttribute("directory", value); }
        }

        //[DisplayName("FTP Pass"), ReadOnly(false), Browsable(true), PropertyOrder(35), DescriptionAttribute("The FTP Password")]
        //public string FTPPass {
        //    get { return GetAttribute("ftpPass"); }
        //    set { SetAttribute("ftpPass", value); }
        //}

        //[CategoryAttribute("Destination Configuration"), DisplayName("SMTP Host"), ReadOnly(false), Browsable(true), PropertyOrder(17), DescriptionAttribute("The SMTP Host")]
        //public string SMTPHost {
        //    get { return GetAttribute("smtphost"); }
        //    set { SetAttribute("smtphost", value); }
        //}
        //[CategoryAttribute("Destination Configuration"), DisplayName("SMTP Host Port"), ReadOnly(false), Browsable(true), PropertyOrder(17), DescriptionAttribute("The port on the SMTP Host to connect to")]
        //public string SMTPPort {
        //    get { return GetAttribute("smtpport"); }
        //    set { SetAttribute("smtpport", value); }
        //}
        //[CategoryAttribute("Destination Configuration"), DisplayName("SMTP User"), ReadOnly(false), Browsable(true), PropertyOrder(1), DescriptionAttribute("The user name if authentication is required by the SMTP Host")]
        //public string SMTPUser {
        //    get { return GetAttribute("smtpuser"); }
        //    set { SetAttribute("smtpuser", value); }
        //}
        //[CategoryAttribute("Destination Configuration"), DisplayName("SMTP Password"), ReadOnly(false), Browsable(true), PropertyOrder(2), DescriptionAttribute("The user password if authentication is required by the SMTP Host")]
        //public string SMTPPass {
        //    get { return GetAttribute("smtppass"); }
        //    set { SetAttribute("smtppass", value); }
        //}
        //[CategoryAttribute("Destination Configuration"), DisplayName("SMTP Use SSL"), ReadOnly(false), Browsable(true), PropertyOrder(4), DescriptionAttribute("Use SSL connection. (Probably needs to be set)")]
        //public bool SMTPSSL {
        //    get { return GetBoolDefaultFalseAttribute(_node, "smtpuseSSL"); }
        //    set { SetAttribute("smtpuseSSL", value); }
        //}
        //[CategoryAttribute("Destination Configuration"), DisplayName("Send As Attachment"), ReadOnly(false), Browsable(true), PropertyOrder(3), DescriptionAttribute("Send the payload as an attachment. By default it is sent as the body of the message")]
        //public bool SMTPAttachment {
        //    get { return GetBoolDefaultFalseAttribute(_node, "smtpAttachment"); }
        //    set { SetAttribute("smtpAttachment", value); }
        //}

        //[CategoryAttribute("Destination Configuration"), DisplayName("SMTP From User"), ReadOnly(false), Browsable(true), PropertyOrder(18), DescriptionAttribute("The name of the user sending the email")]
        //public string SMTPFromUser {
        //    get { return GetAttribute("smtpfromUser"); }
        //    set { SetAttribute("smtpfromUser", value); }
        //}
        //[CategoryAttribute("Destination Configuration"), DisplayName("SMTP From Email"), ReadOnly(false), Browsable(true), PropertyOrder(19), DescriptionAttribute("The email address of the sending user (May be required, for example, the Yahoo SMTP Server requires this to send)")]
        //public string SMTPFromEmail {
        //    get { return GetAttribute("smtpfromEmail"); }
        //    set { SetAttribute("smtpfromEmail", value); }
        //}
        //[CategoryAttribute("Destination Configuration"), DisplayName("SMTP To Email"), ReadOnly(false), Browsable(true), PropertyOrder(17), DescriptionAttribute("The Email address to send the message to")]
        //public string SMTPToEmail {
        //    get { return GetAttribute("smtptoEmail"); }
        //    set { SetAttribute("smtptoEmail", value); }
        //}


        //[CategoryAttribute("SMTP - Optional"), DisplayName("SMTP Subject"), ReadOnly(false), Browsable(true), PropertyOrder(1), DescriptionAttribute("The Subject of the Email.")]
        //public string SMTPSubj {
        //    get { return GetAttribute("smtpsubject"); }
        //    set { SetAttribute("smtpsubject", value); }
        //}
        //[CategoryAttribute("SMTP - Optional"), DisplayName("Attachment Name"), ReadOnly(false), Browsable(true), PropertyOrder(2), DescriptionAttribute("The filename of the attachment.")]
        //public string SMTPAttachmentName {
        //    get { return GetAttribute("smtpattachname"); }
        //    set { SetAttribute("smtpattachname", value); }
        //}
        //[CategoryAttribute("SMTP - Optional"), DisplayName("SMTP To User"), ReadOnly(false), Browsable(true), PropertyOrder(4), DescriptionAttribute("The name of the 'To:' recipient")]
        //public string SMTPToUser {
        //    get { return GetAttribute("smtptouser"); }
        //    set { SetAttribute("smtptouser", value); }
        //}


    }
}