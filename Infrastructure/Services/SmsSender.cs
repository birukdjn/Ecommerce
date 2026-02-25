using Application.Interfaces;
using Infrastructure.Options;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Domain.Common;

namespace Infrastructure.Services
{
    public class SmsSender(IHttpClientFactory httpClientFactory, IOptions<AfroSmsOptions> options) : ISmsSender
    {
        private readonly AfroSmsOptions _settings = options.Value;

        public static string NormalizePhone(string phone)
        {
            if (phone.StartsWith("0"))
                return "+251" + phone.Substring(1);

            if (!phone.StartsWith("+"))
                return "+" + phone;

            return phone;
        }

        public async Task<bool> SendSmsAsync(string to, string message)
        {
            to = NormalizePhone(to);
            var client = httpClientFactory.CreateClient("AfroSms");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _settings.Token);

            var url = $"api/send?from={_settings.IdentifierId}&sender={_settings.SenderName}&to={to}&message={Uri.EscapeDataString(message)}";

            var response = await client.GetAsync(url);
            return response.IsSuccessStatusCode;
        }

        public async Task<Result<string>> SendSmsChallengeAsync(string to, string prefix = "Verification Code: ")
        {


            to = NormalizePhone(to);
            var client = httpClientFactory.CreateClient("AfroSms");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _settings.Token);

            // ttl=300 (5 minutes), len=4 (code length)
            var url = $"api/challenge?from={_settings.IdentifierId}&sender={_settings.SenderName}&to={to}&pr={Uri.EscapeDataString(prefix)}&ttl=300&len=4&t=0";
            try
            {
                var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                    return Result<string>.Failure("AfroSms API returned an error.");

                var content = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(content);
                var ack = doc.RootElement.GetProperty("acknowledge").GetString();

                if (ack == "success")
                {
                    var vId = doc.RootElement.GetProperty("response").GetProperty("verificationId").GetString();
                    return Result<string>.Success(vId!);
                }
                return Result<string>.Failure("Failed to initiate SMS challenge.");
            }
            catch (Exception ex)
            {
                return Result<string>.Failure($"SMS Service Exception: {ex.Message}");
            }
        }

        public async Task<Result> VerifyCodeAsync(string to, string code, string verificationId)
        {
            to = NormalizePhone(to);
            var client = httpClientFactory.CreateClient("AfroSms");
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _settings.Token);

            var url = $"api/verify?to={to}&vc={verificationId}&code={code}";

            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode) return Result.Failure("Network error during verification.");

            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);

            return doc.RootElement.GetProperty("acknowledge").GetString() == "success"
                ? Result.Success()
                : Result.Failure("Invalid or expired code.");
        }

       
    }
}