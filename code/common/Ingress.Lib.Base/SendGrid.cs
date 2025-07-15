using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Ingress.Lib.Base
{
    public static class SendGrid
    {
        private static string EncodeBinary(string filename)
        {
            byte[] fileBytes = File.ReadAllBytes(filename);
            return Convert.ToBase64String(fileBytes);
        }

        private static string EncodeContent(string filename)
        {
            var attachmentContent = File.ReadAllText(filename);
            var plainTextBytes = Encoding.UTF8.GetBytes(attachmentContent);
            string encodedContent = Convert.ToBase64String(plainTextBytes);
            return encodedContent;
        }

        private static string GetMimeTypeFromExtension(string extension)
        {
            if (extension == ".zip")
            {
                return "application/zip";
            }

            return "text/csv";
        }

        public static void SendEmail(string apiKey,
                                     string subject,
                                     string mainMessage,
                                     IEnumerable<string> attachmentFilenames = null)
        {
            try
            {
                //only below sender has been authorized to send email - any other email will fail to send 
                var from = new EmailAddress("thirdpartydata@acmewidgets.com", "Ingress Communications");
                var to = new EmailAddress("4c9da0d6.acmewidgets.onmicrosoft.com@amer.teams.ms", "Ingress - Engineering");
                var client = new SendGridClient(apiKey);

                var body = GetEmailBody(mainMessage);

                var msg = MailHelper.CreateSingleEmail(from, to, subject, string.Empty, body);

                if (attachmentFilenames != null && attachmentFilenames.Any())
                {
                    var attachments = attachmentFilenames.Select(e =>
                    {
                        var mimeType = GetMimeTypeFromExtension(Path.GetExtension(e));
                        return new Attachment
                        {
                            Content = EncodeBinary(e),
                            //ContentId
                            Disposition = "attachment",
                            Filename = Path.GetFileName(e),
                            Type = mimeType,
                        };
                    });
                    msg.Attachments = attachments.ToList();
                }

                var response = client.SendEmailAsync(msg).GetAwaiter().GetResult();
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("Failed to send email.Message:" + response.Body.ReadAsStringAsync().GetAwaiter().GetResult());
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static string GetEmailBody(string mainMessage)
        {
            var emailBody = new StringBuilder();

            emailBody.AppendLine("<html lang='en'><body>");
            emailBody.AppendLine("<br />");
            emailBody.AppendLine("Hi,<br /><br />");

            emailBody.AppendLine($"{mainMessage}.<br /> " +
                                 $"Please find the attached report for more details. " +
                                 $"A copy has also been uploaded to IngressDataValidation Storage Account.");

            emailBody.AppendLine($"<br /><br />");

            emailBody.AppendLine("Thank you,<br />");
            emailBody.AppendLine("Ingress");
            emailBody.AppendLine("</body>");

            return emailBody.ToString();
        }
    }
}
