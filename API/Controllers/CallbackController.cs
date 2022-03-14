using Data;
using Data.Entities;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CallbackController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public CallbackController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// This route is made specifically to handle the callback from twilio. 
        /// Any other sms-message provider should also make callbacks.
        /// </summary>
        /// <returns></returns>
        [HttpPost("twilio-callback")]
        public async Task<IActionResult> SmsCallback()
        {
            // Check that callback conatins message id.
            string? messageSid = Request.Form["MessageSid"];
            if (messageSid is null)
                return Unauthorized();

            // Check that the message id was expected.
            List<Sms> smsQueryResult = await _unitOfWork.SmsRepository.GetAllAsync(sms => sms.Sid == messageSid);
            Sms? sms = smsQueryResult.FirstOrDefault();
            if (sms is null)
                return Unauthorized();

            // Update the message.
            sms.Status = Request.Form["MessageStatus"];
            sms.ErrorCode = Request.Form["ErrorCode"];
            sms.FromNumber = Request.Form["From"];

            _unitOfWork.SmsRepository.Update(sms);

            return Ok();
        }
    }
}
