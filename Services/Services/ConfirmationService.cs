using Data.Entities.Identity;
using Hangfire;
using MailTemplate;
using MailTemplates.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit.Text;
using Models.ConfigurationModels;
using Services.Interfaces;

namespace Services.Services
{
    public class ConfirmationService : IConfirmationService
    {
        private readonly ILogger<ConfirmationService> _logger;
        private readonly ITemplateRenderService _templateRenderService;
        private readonly ApplicationIdentityModel _applicationIdentityModel;
        private readonly UserManager<ApplicationUser> _userManager;

        public ConfirmationService(
            ILogger<ConfirmationService> logger,
            ITemplateRenderService templateRenderService,
            IOptions<ApplicationIdentityModel> applicationIdentityModel,
            UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _templateRenderService = templateRenderService;
            _applicationIdentityModel = applicationIdentityModel.Value;
            _userManager = userManager;
        }

        public async Task<string> CreateEmailConfirmationPinAsync(ApplicationUser user, string token)
        {
            // Render email template.
            string templateString = await _templateRenderService.GenericPinTemplateAsync(
                new GenericPinTemplateModel()
                {
                    AppIdentity = _applicationIdentityModel,
                    ExpiresIn = "3 Minutes.",
                    PinType = "Email Confirmation.",
                    Pin = token
                });

            // Queue email send-task with hangfire.
            string jobId = BackgroundJob.Enqueue<EmailService>(emailService => emailService.SendMessageAsync(
                user.UserName, user.Email, TextFormat.Html, "Email Confirmation", templateString));

            return jobId;
        }

        public string CreatePhoneConfirmationPinAsync(ApplicationUser user, string token)
        {
            // Sms template. No magic..
            string templateString = $"Your PIN: {token}";

            // Queue sms send-task with hangfire.
            string jobId = BackgroundJob.Enqueue<SmsService>(smsService => smsService.SendSms(user, templateString));

            return jobId;
        }

        public async Task<string> CreatePasswordResetMailAsync(ApplicationUser user, string token)
        {
            // Render mail template.
            string templateString = await _templateRenderService.GenericPinTemplateAsync(
                new GenericPinTemplateModel()
                {
                    AppIdentity = _applicationIdentityModel,
                    ExpiresIn = "3 Minutes.",
                    PinType = "Password Reset.",
                    Pin = token
                });

            // Queue email send-task with hangfire.
            string jobId = BackgroundJob.Enqueue<EmailService>(emailService => emailService.SendMessageAsync(
                user.UserName, user.Email, TextFormat.Html, "Password reset", templateString));

            return jobId;
        }

        public string CreatePasswordResetSmsAsync(ApplicationUser user, string token)
        {
            // Sms template. No magic..
            string templateString = $"Your PIN: {token}";

            // Queue sms send-task with hangfire.
            string jobId = BackgroundJob.Enqueue<SmsService>(smsService => smsService.SendSms(user, templateString));

            return jobId;
        }

        public async Task<bool> ConfirmationFailedAsync(ApplicationUser user)
        {
            bool res = false;
            user.ConfirmationFailedCount++;
            if (user.ConfirmationFailedCount > 5)
            {
                user.ConfirmationFailedCount = 0;
                await _userManager.UpdateSecurityStampAsync(user);
                res = true;
            }

            IdentityResult result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                _logger.LogWarning(string.Format(
                    "ConfirmationFailedAsync Error on user update. Errors: {0}",
                    string.Join(". ", result.Errors!.Select(x => $"{x.Code} - {x.Description} "))));

            return res;
        }
    }
}
