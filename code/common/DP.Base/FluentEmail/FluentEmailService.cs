using FluentEmail.Core;
using FluentEmail.Core.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using DP.Base.Contracts.EmailSupport;

namespace DP.Base.FluentEmail
{
    public class FluentEmailService : IEmailService
    {
        private readonly IFluentEmail _fluentEmail;

        public FluentEmailService(IFluentEmail fluentEmail)
        {
            _fluentEmail = fluentEmail
                ?? throw new ArgumentNullException(nameof(fluentEmail));
        }

        public async Task Send(EmailDetails emailDetails)
        {
            var recipientAddressList = emailDetails.RecipientAddressList.Select(e => new Address(e));
            await _fluentEmail.To(recipientAddressList)
                .Subject(emailDetails.Subject)
                .Body(emailDetails.Body, true)
                .SendAsync();
        }
    }


}
