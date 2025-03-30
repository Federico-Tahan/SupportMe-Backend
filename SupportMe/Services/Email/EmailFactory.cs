using System.Net.Mail;
using System.Net;
using Razor.Templating.Core;
using SupportMe.Data;

namespace SupportMe.Services.Email
{
    public class EmailFactory
    {
        public static void SendEmail(Models.Email email, DataContext context)
        {

            var emailSender = context.EmailSenderConfig.FirstOrDefault();

            if (email == null)
            {
                return;
            }


            var message = new MailMessage();

            foreach (var address in email.ToAddresses)
            {
                message.To.Add(new MailAddress(address));
            }

            foreach (var address in email.ToBccAddresses)
            {
                message.Bcc.Add(new MailAddress(address));
            }

            message.Subject = email.Subject;
            message.Body = email.Message;
            message.IsBodyHtml = true;

            foreach (var item in email.Attachments)
            {
                message.Attachments.Add(item);
            }

            message.From = new MailAddress(emailSender.FromEmail, email.FromName);
            using (var smtp = new SmtpClient())
            {
                var credential = new NetworkCredential
                {
                    UserName = emailSender.UserName,
                    Password = emailSender.Password
                };
                smtp.Credentials = credential;
                smtp.Host = emailSender.Host;
                smtp.Port = emailSender.Port;
                smtp.EnableSsl = emailSender.EnableSsl;

                try
                {
                    smtp.Send(message);
                }
                catch (Exception e)
                {
                }
            }
        }
        public async static Task<string> RenderViewToStringAsync(string viewPath, object model = null)
        {
            string html = string.Empty;
            try
            {
                html = await RazorTemplateEngine.RenderAsync(viewPath, model);
            }
            catch (Exception ex)
            {
            }


            return html;
        }

    }

}
