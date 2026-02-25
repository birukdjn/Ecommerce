
using Domain.Common;

namespace Application.Interfaces
{
    public interface ISmsSender
    {
        Task<bool> SendSmsAsync(string to, string message);

        Task<Result<string>> SendSmsChallengeAsync(string to, string prefix = "Your code is: ");

        Task<Result> VerifyCodeAsync(string to, string code, string verificationId);
    }
}
