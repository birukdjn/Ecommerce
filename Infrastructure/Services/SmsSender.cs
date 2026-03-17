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

        public static string NormalizedPhone(string phone)
        {
            if (phone.StartsWith("0"))
                return "+251" + phone.Substring(1);

            if (!phone.StartsWith("+"))
                return "+" + phone;

            return phone;
        }

        public async Task<bool> SendSmsAsync(string to, string message)
        {
            to = NormalizedPhone(to);
            var client = httpClientFactory.CreateClient("AfroSms");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _settings.Token);

            var url = $"api/send?from={_settings.IdentifierId}&sender={_settings.SenderName}&to={to}&message={Uri.EscapeDataString(message)}";

            var response = await client.GetAsync(url);
            return response.IsSuccessStatusCode;
        }

        public async Task<Result<string>> SendOtpAsync(string Phone, string prefix = "Verification Code: ")
        {


            Phone = NormalizedPhone(Phone);
            var client = httpClientFactory.CreateClient("AfroSms");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _settings.Token);

            var url =
                $"api/challenge" +
                $"?from={_settings.IdentifierId}" +
                $"&sender={_settings.SenderName}" +
                $"&to={Phone}" +
                $"&pr={Uri.EscapeDataString(prefix)}" +
                $"&ttl=300&len=6&t=0";

            try
            {
                var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                    return Result<string>.Failure("AfroSms API returned an error.");

                var content = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(content);

                if (doc.RootElement.TryGetProperty("acknowledge", out var ackElement) &&
                    ackElement.GetString() == "success")
                {
                    return Result<string>.Success("OTP Sent Successfully");
                }

                return Result<string>.Failure("Failed to initiate SMS challenge.");
            }
            catch (Exception ex)
            {
                return Result<string>.Failure($"SMS Service Exception: {ex.Message}");
            }
        }

        public async Task<Result> VerifyOtpAsync(string Phone, string code)
        {
            Phone = NormalizedPhone(Phone);
            var client = httpClientFactory.CreateClient("AfroSms");
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _settings.Token);

            var url = $"api/verify?to={Phone}&code={code}";

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