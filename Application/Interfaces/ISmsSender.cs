
using Domain.Common;

namespace Application.Interfaces
{
    public interface ISmsSender
    {
        Task<bool> SendSmsAsync(string phoneNumber, string message);

        Task<Result<string>> SendOtpAsync(string phoneNumber, string prefix = "Your code is: ");

        Task<Result> VerifyOtpAsync(string phoneNumber, string code);

    }
}
