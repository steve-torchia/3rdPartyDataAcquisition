using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DP.Base.Contracts.EmailSupport;

namespace DP.Base.FluentEmail
{

    public static class FluentEmailExtensions
    {
        public static void AddFluentEmail(this IServiceCollection services, IConfiguration configuration)
        {
            var emailSettings = configuration.GetSection(EmailHelpers.EmailConfigSectionName);

            var defaultFromEmail = emailSettings[EmailHelpers.DefaultFromEmailConfigKey];
            var host = emailSettings[EmailHelpers.HostConfigKey];
            var port = int.Parse(emailSettings[EmailHelpers.SMTPSettingPortConfigKey]);
            var userName = emailSettings[EmailHelpers.SMTPSettingUserNameConfigKey];
            var password = emailSettings[EmailHelpers.SMTPSettingPasswordConfigKey];

            services.AddFluentEmail(defaultFromEmail)
                .AddSmtpSender(host, port, userName, password);
        }
    }
}
