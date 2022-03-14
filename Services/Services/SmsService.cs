
using Data;
using Data.Entities;
using Data.Entities.Identity;
using Microsoft.Extensions.Options;
using Models.ConfigurationModels;
using Services.Interfaces;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace Services.Services
{
    public class SmsService : ISmsService
    {
        private readonly SmsSettingsModel _smsServiceSettings;
        private readonly IUnitOfWork _unitOfWork;

        public SmsService(
            IOptions<SmsSettingsModel> smsServiceSettings,
            IUnitOfWork unitOfWork)
        {
            _smsServiceSettings = smsServiceSettings.Value;
            _unitOfWork = unitOfWork;
        }

        public async Task SendSms(ApplicationUser user, string contentStr)
        {
            TwilioClient.Init(_smsServiceSettings.User, _smsServiceSettings.Pass);

            // Create SMS Message.
            var messageOptions = new CreateMessageOptions(new PhoneNumber(user.PhoneNumber))
            {
                MessagingServiceSid = _smsServiceSettings.Msid,
                Body = contentStr,
            };

            // Add callback url.
            if (_smsServiceSettings.CallbackUrl is not null)
                messageOptions.StatusCallback = new Uri(_smsServiceSettings.CallbackUrl);

            // Send SMS Message.
            MessageResource message = await MessageResource.CreateAsync(messageOptions);

            // Create and save Sms entity in database.
            await _unitOfWork.SmsRepository.InsertAsync(new Sms()
            {
                Sid = message.Sid,
                Status = message.Status.ToString(),
                ToNumber = message.To,
                Price = message.Price,
                PriceUnit = message.PriceUnit,
                ErrorCode = message.ErrorCode.ToString(),
                ErrorMessage = message.ErrorMessage,
                MessagingServiceSid = message.MessagingServiceSid,
                AccountSid = message.AccountSid,
                ApplicationUserId = user.Id,
                ApplicationUser = user,
            });
        }
    }
}
