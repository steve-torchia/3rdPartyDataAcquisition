using System.Threading.Tasks;

namespace DP.Base.Contracts.EmailSupport
{
    public interface IEmailService
    {
        Task Send(EmailDetails emailDetails);
    }
}
