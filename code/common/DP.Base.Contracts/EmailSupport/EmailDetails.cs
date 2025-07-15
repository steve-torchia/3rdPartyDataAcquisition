using System.Collections.Generic;

namespace DP.Base.Contracts.EmailSupport
{
    public class EmailDetails
    {
        public List<string> RecipientAddressList { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }

        public EmailDetails(List<string> recipientAddressList, string subject, string body)
        {
            RecipientAddressList = recipientAddressList;
            Subject = subject;
            Body = body;
        }
    }
}
