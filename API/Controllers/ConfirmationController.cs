using Data.Entities.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models.ResponseModels;
using Services.Interfaces;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ConfirmationController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfirmationService _confirmationService;

        public ConfirmationController(
            UserManager<ApplicationUser> userManager,
            IConfirmationService confirmationService)
        {
            _userManager = userManager;
            _confirmationService = confirmationService;
        }

        [Authorize]
        [HttpGet("email")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResponseModel))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResponseModel))]
        public async Task<IActionResult> CreateEmailConfirmation()
        {
            // Get current ApplicationUser.
            ApplicationUser user = await _userManager.GetUserAsync(User);
            if (user == null)
                return BadRequest(new ResponseModel() { Status = Status.InvalidRequest, Message = $"Invalid request." });

            // Confirm that the email is not already confirmed.
            if (user.EmailConfirmed)
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ResponseModel() { Status = Status.AlreadyConfirmedError, Message = $"This email has already been confirmed." });

            // Generate the confirmation token.
            string confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            // Create and send the email confirmation token(pin).
            await _confirmationService.CreateEmailConfirmationPinAsync(user, confirmationToken);

            return Ok(new ResponseModel() { Status = Status.Success, Message = $"The email, {user.Email} is now pending confirmation." });
        }

        [Authorize]
        [HttpPost("email-pin")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResponseModel))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResponseModel))]
        public async Task<IActionResult> ConfirmEmail(string token)
        {
            // Get current user.
            ApplicationUser user = await _userManager.GetUserAsync(User);
            if (user == null)
                return BadRequest(new ResponseModel() { Status = Status.InvalidRequest, Message = $"Invalid request." });

            // Confirm that the email is not already confirmed.
            if (user.EmailConfirmed)
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ResponseModel() { Status = Status.AlreadyConfirmedError, Message = $"This email has already been confirmed." });

            // Decode url encoded token and confirm email with it.
            IdentityResult result = await _userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
            {
                await _confirmationService.ConfirmationFailedAsync(user);
                if (result.Errors.Any())
                    return StatusCode(StatusCodes.Status500InternalServerError, new ResponseModel
                    {
                        Status = Status.PasswordResetError,
                        Message = $"Failed. {user.ConfirmationFailedCount} out of 5 attempts to confirm used. Error(s): {string.Join(". ", result.Errors!.Select(x => $"{x.Code} - {x.Description}"))}"
                    });
                else
                    return StatusCode(StatusCodes.Status500InternalServerError, new ResponseModel
                    {
                        Status = Status.PasswordResetError,
                        Message = $"Failed. {user.ConfirmationFailedCount} out of 5 attempts to confirm used. Error unknown"
                    });
            }
            return Ok(new ResponseModel() { Status = Status.Success, Message = "Email has been confirmed." });
        }

        [Authorize]
        [HttpGet("phone")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResponseModel))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResponseModel))]
        public async Task<IActionResult> CreatePhoneConfirmation()
        {
            // Get current ApplicationUser.
            ApplicationUser user = await _userManager.GetUserAsync(User);
            if (user == null)
                return BadRequest(new ResponseModel() { Status = Status.InvalidRequest, Message = $"Invalid request." });

            // Confirm that the phone number is not already confirmed.
            if (user.PhoneNumberConfirmed)
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ResponseModel() { Status = Status.AlreadyConfirmedError, Message = $"This phone number has already been confirmed." });

            // Generate the confirmation token.
            string confirmationToken = await _userManager.GenerateChangePhoneNumberTokenAsync(user, user.PhoneNumber);

            // Create and send the phone confirmation token(pin).
            _confirmationService.CreatePhoneConfirmationPinAsync(user, confirmationToken);

            return Ok(new ResponseModel() { Status = Status.Success, Message = $"The phone number, {user.PhoneNumber} is now pending confirmation." });
        }

        [Authorize]
        [HttpPost("phone-pin")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResponseModel))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResponseModel))]
        public async Task<IActionResult> ConfirmPhone(string token)
        {
            // Get current user.
            ApplicationUser user = await _userManager.GetUserAsync(User);
            if (user == null)
                return BadRequest(new ResponseModel() { Status = Status.InvalidRequest, Message = $"Invalid request." });

            // Confirm that the email is not already confirmed.
            if (user.PhoneNumberConfirmed)
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ResponseModel() { Status = Status.AlreadyConfirmedError, Message = $"This phone number has already been confirmed." });

            // Decode url encoded token and confirm email with it.
            bool isValide = await _userManager.VerifyChangePhoneNumberTokenAsync(user, token, user.PhoneNumber);
            if (!isValide)
            {
                bool tokenReset = await _confirmationService.ConfirmationFailedAsync(user);
                string message;
                if (tokenReset)
                    message = "Failed confirmation attempts exeeced, request a new token.";
                else
                    message = "Invalid token try again.";

                return StatusCode(StatusCodes.Status500InternalServerError, new ResponseModel
                {
                    Status = Status.PhoneConfirmationFailed,
                    Message = message
                });
            }

            user.PhoneNumberConfirmed = true;
            await _userManager.UpdateAsync(user);

            return Ok(new ResponseModel() { Status = Status.Success, Message = "Phone number has been confirmed." });
        }

        [Authorize]
        [HttpGet("reset-password-mail")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResponseModel))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResponseModel))]
        public async Task<IActionResult> CreatePasswordResetMail()
        {
            // Get current ApplicationUser.
            ApplicationUser user = await _userManager.GetUserAsync(User);
            if (user == null)
                return BadRequest(new ResponseModel() { Status = Status.InvalidRequest, Message = $"Invalid request." });

            // Insure confirmed mail.
            if (!user.EmailConfirmed)
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ResponseModel() { Status = Status.EmailNotConfirmed, Message = $"Confirmed email is required." });

            // Generate the reset pin.
            string passwordResetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

            // Send pin by mail.
            await _confirmationService.CreatePasswordResetMailAsync(user, passwordResetToken);

            return Ok(new ResponseModel() { Status = Status.Success, Message = $"Password reset pin is now send to your confirmed mail {user.Email}." });
        }

        [Authorize]
        [HttpGet("reset-password-phone")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResponseModel))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResponseModel))]
        public async Task<IActionResult> CreatePasswordResetPhone()
        {
            // Get current ApplicationUser.
            ApplicationUser user = await _userManager.GetUserAsync(User);
            if (user == null)
                return BadRequest(new ResponseModel() { Status = Status.InvalidRequest, Message = $"Invalid request." });

            // Insure confirmed phone.
            if (!user.PhoneNumberConfirmed)
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ResponseModel() { Status = Status.EmailNotConfirmed, Message = $"Confirmed phone number is required." });

            // Generate the reset pin.
            string passwordResetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

            // Send pin by mail.
            _confirmationService.CreatePasswordResetSmsAsync(user, passwordResetToken);

            return Ok(new ResponseModel() { Status = Status.Success, Message = $"Password reset pin is now send to your confirmed phone number {user.PhoneNumber}." });
        }

        [Authorize]
        [HttpPost("reset-password")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResponseModel))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResponseModel))]
        public async Task<IActionResult> ConfirmPasswordReset(string token, string newPassword)
        {
            // Get current ApplicationUser.
            ApplicationUser user = await _userManager.GetUserAsync(User);
            if (user == null)
                return BadRequest(new ResponseModel() { Status = Status.InvalidRequest, Message = $"Invalid request." });

            // Insure confirmed mail.
            if (!user.EmailConfirmed)
                return BadRequest(new ResponseModel() { Status = Status.EmailNotConfirmed, Message = $"Confirmed email is required." });

            // Reset password.
            IdentityResult result = await _userManager.ResetPasswordAsync(user, token, newPassword);
            if (!result.Succeeded)
            {
                bool tokenReset = await _confirmationService.ConfirmationFailedAsync(user);
                string message;

                if (!tokenReset)
                    if (result.Errors.Any())
                        message = $"Failed. {user.ConfirmationFailedCount} out of 5 attempts to confirm used. Error(s): {string.Join(". ", result.Errors!.Select(x => $"{x.Code} - {x.Description}"))}";
                    else
                        message = $"Failed. {user.ConfirmationFailedCount} out of 5 attempts to confirm used. Error unknown";
                else
                    message = "You have exceeded the allowed confirmation attempts. Request a new token.";

                return StatusCode(StatusCodes.Status500InternalServerError, new ResponseModel
                {
                    Status = Status.PasswordResetError,
                    Message = message
                });
            }
            return Ok(new ResponseModel() { Status = Status.Success, Message = $"Your password has been reset." });
        }
    }
}
