using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Models.ConfigurationModels;
using Services.Interfaces;

namespace Services.Services
{
    public class EmailService : IEmailService
    {
        private readonly SmtpSettingsModel _mailSettings;
        private readonly ApplicationIdentityModel _applicationIdentity;
        private readonly ILogger<EmailService> _logger;

        public EmailService(
            IOptions<SmtpSettingsModel> mailSettings,
            IOptions<ApplicationIdentityModel> applicationIdentity,
            ILogger<EmailService> logger)
        {
            _mailSettings = mailSettings.Value;
            _applicationIdentity = applicationIdentity.Value;
            _logger = logger;
        }

        public async Task SendAsync(MimeMessage message)
        {
            try
            {
                using var client = new SmtpClient();
                {
                    await client.ConnectAsync(_mailSettings.SmtpServer, _mailSettings.SmtpPort, _mailSettings.UseSSL);
                    await client.AuthenticateAsync(_mailSettings.Username, _mailSettings.Password);
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(eventId: 0200, exception: ex, message: "Failed to send an mail message.");
                throw;
            }
        }

        public async Task SendMessageAsync(string recipientName, string recipientMailAddress, MimeKit.Text.TextFormat format, string subject, string content)
        {
            MimeMessage message = new();
            message.To.Add(new MailboxAddress(recipientName, recipientMailAddress));
            message.From.Add(new MailboxAddress(_applicationIdentity.Name, _mailSettings.Username));
            message.Subject = subject;
            message.Body = new TextPart(format) { Text = content };
            await SendAsync(message);
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task<List<MimeMessage>> GetAsync() => throw new NotImplementedException();
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    }
}
