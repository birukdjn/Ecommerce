using Application.Interfaces;
using MailKit.Net.Smtp;
using MimeKit;
using Infrastructure.Options;
using MailKit.Security;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services;

public class EmailSender(IOptions<EmailOptions> settings) : IEmailSender
{
    private readonly EmailOptions _settings = settings.Value;

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.DisplayName, _settings.From));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;

        message.Body = new TextPart("html") { Text = body };

        using var client = new SmtpClient();
        try
        {
            await client.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_settings.Username, _settings.Password);
            await client.SendAsync(message);
        }
        finally
        {
            await client.DisconnectAsync(true);
            client.Dispose();
        }
    }
}