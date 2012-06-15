using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;
using WatchNotify;
using System.IO;
using WatchNotify.EmailService;
using WatchNotifyService;

namespace WatchNotifyService.EmailService
{
    public class EmailService : IEmailService
    {
        const string smtpServer = "mail.apbfn.com";
        public EmailService()
        {
            
        }

        public System.Net.NetworkCredential Credentials { get; set; }
        public int Timeout { get; set; }
        public bool EnableDecompression { get; set; }
        public bool UnsafeAuthenticatedConnectionSharing { get; set; }
        public string Url { get; set; }

        public bool CheckConnection()
        {
            return true;
        }

        public void EmailWebService(string To, string From, string Subject, string Body)
        {
            SmtpClient smtp = new SmtpClient { Host = smtpServer };
            MailMessage message = GetMailMessage(To, From, Subject, Body);

            smtp.Send(message);
            smtp.Dispose();
        }

        private MailMessage GetMailMessage(string To, string From, string Subject, string Body)
        {
            MailAddress fromaddress = new MailAddress(From);
            MailMessage message = new MailMessage { IsBodyHtml = true, From = fromaddress };

            message.To.Add(To.Replace(";", ","));
            message.Subject = Subject;
            message.Body = Body;
            return message;
        }

        public void EmailWebServiceWithAttachment(string To, string From, string Subject, string Body, byte[] Attachment, string AttachmentName)
        {
            SmtpClient smtpclient = new SmtpClient { Host = smtpServer };
            MailMessage message = GetMailMessage(To, From, Subject, Body);

            TemporaryFile tempFile = new TemporaryFile("", AttachmentName);
            FileStream objfilestream = new FileStream(tempFile.FilePath, FileMode.Create, FileAccess.ReadWrite);
            using (BinaryWriter writer = new BinaryWriter(objfilestream))
            {
                writer.Write(Attachment);
            }

            Attachment a = new Attachment(tempFile.FilePath);
            message.Attachments.Add(a);
            smtpclient.Send(message);

            a.Dispose();
            tempFile.Dispose();
            smtpclient.Dispose();
        }

        public void Dispose()
        {            
        }
    }
}
