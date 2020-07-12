using MailKit.Net.Smtp;
using MimeKit;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace QueueExchange {
    class QESMTP : QueueAbstract {


        private readonly string smtphost;
        private readonly int smtpport;
        private readonly string smtpuser;
        private readonly string smtppass;
        private readonly bool smtpuseSSL = false;
        private readonly bool smtpAttachment = false;
        private readonly string smtpsubject;
        private readonly string smtpfromUser;
        private readonly string smtpfromEmail;
        private readonly string smtptoUser;
        private readonly string smtptoEmail;
        private readonly string smtpattachname;
        private readonly bool OK_TO_SEND = true;



        public QESMTP(XElement defn, IProgress<QueueMonitorMessage> monitorMessageProgress) : base(defn, monitorMessageProgress) {

            try {
                smtphost = definition.Attribute("smtphost").Value;
            }
            catch (Exception) {
                logger.Error($"SMTP not defined for {definition.Attribute("name").Value}");
                OK_TO_SEND = false;
            }

            try {
                smtpport = int.Parse(definition.Attribute("smtpport").Value);
            }
            catch (Exception) {
                logger.Error($"No SMTP Port correctly defined for {definition.Attribute("name").Value}");
                OK_TO_SEND = false;
            }

            try {
                smtpuser = definition.Attribute("smtpuser").Value;
            }
            catch (Exception) {
                smtpuser = null;
            }

            try {
                smtpattachname = definition.Attribute("smtpattachname").Value;
            }
            catch (Exception) {
                smtpattachname = "LoadInjectorMsg";
            }

            try {
                smtppass = definition.Attribute("smtppass").Value;
            }
            catch (Exception) {
                smtppass = null;
            }

            try {
                smtpsubject = definition.Attribute("smtpsubject").Value;
            }
            catch (Exception) {
                smtpsubject = "Load Injector Message";
            }

            try {
                smtpfromUser = definition.Attribute("smtpfromUser").Value;
            }
            catch (Exception) {
                smtpfromUser = "Load Injector";
            }

            try {
                smtpfromEmail = definition.Attribute("smtpfromEmail").Value;
            }
            catch (Exception) {
                smtpfromEmail = "";
            }

            try {
                smtptoUser = definition.Attribute("smtptoUser").Value;
            }
            catch (Exception) {
                smtptoUser = "Load Injector Recipient";
            }

            try {
                smtptoEmail = definition.Attribute("smtptoEmail").Value;
            }
            catch (Exception) {
                logger.Error($"No SMTP Destination Email defined for {definition.Attribute("name").Value}");
                OK_TO_SEND = false;
            }

            try {
                smtpuseSSL = bool.Parse(definition.Attribute("smtpuseSSL").Value);
            }
            catch (Exception) {
                smtpuseSSL = false;
            }

            try {
                smtpAttachment = bool.Parse(definition.Attribute("smtpAttachment").Value);
            }
            catch (Exception) {
                smtpAttachment = false; ;
            }


        }

        public override async Task StartListener() {
            await Task.Run(() =>
            {
            });
        }


        public override async Task<ExchangeMessage> SendToOutputAsync(ExchangeMessage message) {


            await Task.Run(() =>
            {
                {

                    if (!OK_TO_SEND) {
                        logger.Warn("SMTP Interface not configured correctly");
                        return;
                    }
                    string subject = smtpsubject;

                    try {
                        var mailmessage = new MimeMessage();
                        mailmessage.From.Add(new MailboxAddress(smtpfromUser, smtpfromEmail));
                        mailmessage.To.Add(new MailboxAddress(smtptoUser, smtptoEmail));
                        mailmessage.Subject = subject;

                        if (smtpAttachment) {
                            var body = new TextPart("plain") {
                                Text = "Message is in attachment"
                            };
                            var attachment = new MimePart("text", "plain") {
                                ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                                FileName = Path.GetFileName(smtpattachname)
                            };

                            byte[] byteArray = Encoding.ASCII.GetBytes(message.payload);
                            MemoryStream stream = new MemoryStream(byteArray);
                            attachment.Content = new MimeContent(stream, ContentEncoding.Default);

                            var multipart = new Multipart("mixed") {
                                body,
                                attachment
                            };

                            // now set the multipart/mixed as the message body
                            mailmessage.Body = multipart;
                        }
                        else {
                            mailmessage.Body = new TextPart("plain") {
                                Text = message.payload
                            };
                        }


                        using (var client = new SmtpClient()) {
                            // For demo-purposes, accept all SSL certificates (in case the server supports STARTTLS)
                            client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                            client.Connect(smtphost, smtpport, smtpuseSSL);
                            //client.Connect("smtp.mail.yahoo.com", 465, true);


                            // Note: only needed if the SMTP server requires authentication
                            if (smtpuser != null) {
                                client.Authenticate(smtpuser, smtppass);
                            }
                            //                  client.Authenticate("dave_on_tour@yahoo.com", "!@aiw2dihsf!@");

                            client.Send(mailmessage);
                            client.Disconnect(true);
                        }
                    }
                    catch (Exception e) {
                        logger.Error($"SMTP Client Send Exception: {e.Message}");
                    }
                }
            });
            return null;
        }

        public override bool SetUp() {
            return true;
        }
    }
}
