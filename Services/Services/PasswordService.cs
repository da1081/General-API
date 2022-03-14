using Data.Entities.Identity;
using Hangfire;
using MailTemplate;
using MailTemplates.Models;
using Microsoft.Extensions.Options;
using MimeKit.Text;
using Models.ConfigurationModels;
using Services.Interfaces;

namespace Services.Services
{
    public class PasswordService : IPasswordService
    {
        private readonly ApplicationIdentityModel _applicationIdentityModel;
        private readonly ITemplateRenderService _templateRenderService;

        public PasswordService(
            IOptions<ApplicationIdentityModel> applicationIdentityModel,
            ITemplateRenderService templateRenderService)
        {
            _applicationIdentityModel = applicationIdentityModel.Value;
            _templateRenderService = templateRenderService;
        }

        public async Task<string> CreateEmailTemporaryPasswordPinAsync(ApplicationUser user, string token)
        {
            // Render mail template.
            string templateString = await _templateRenderService.GenericPinTemplateAsync(
                new GenericPinTemplateModel()
                {
                    AppIdentity = _applicationIdentityModel,
                    ExpiresIn = "3 Minutes.",
                    PinType = "Login.",
                    Pin = token
                });

            // Queue email send-task with hangfire.
            string jobId = BackgroundJob.Enqueue<EmailService>(emailService => emailService.SendMessageAsync(
                user.UserName, user.Email, TextFormat.Html, "Time-based one-time email login", templateString));

            return jobId;
        }

        public string CreatePhoneTemporaryPasswordPinAsync(ApplicationUser user, string token)
        {
            // Sms template. No magic..
            string templateString = $"Your PIN: {token}";

            // Queue sms send-task with hangfire.
            string jobId = BackgroundJob.Enqueue<SmsService>(smsService => smsService.SendSms(user, templateString));

            return jobId;
        }
    }
}
