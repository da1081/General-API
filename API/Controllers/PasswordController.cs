using Data;
using Data.Entities.Identity;
using Data.TokenProviders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models.ResponseModels;
using Models.SingleDataModels;
using Services.Interfaces;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PasswordController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPasswordService _passwordService;
        private readonly IUnitOfWork _unitOfWork;

        public PasswordController(
            UserManager<ApplicationUser> userManager,
            IPasswordService passwordService,
            IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _passwordService = passwordService;
            _unitOfWork = unitOfWork;
        }

        [AllowAnonymous]
        [HttpPost("temporary-email")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel))]
        public async Task<IActionResult> RequestTemporaryPasswordMail([FromBody] EmailModel model)
        {
            // Validate Model.
            if (!ModelState.IsValid)
                return ValidationProblem();

            // Same response used no matter what.
            var response = Ok(new ResponseModel()
            {
                Status = Status.Success,
                Message = "If user exists and has confirmed email, a temporary password has been send"
            });

            // Check if user can be found by email.
            ApplicationUser user = await _userManager.FindByEmailAsync(model.Email);
            if (user is null)
                return response;

            // Confirmed mail required.
            if (!user.EmailConfirmed)
                return response;

            // Generate PIN code.
            string token = await _userManager.GenerateUserTokenAsync(
                user, SecurityPinProvider.TotpProvider, "Time-based one-time password by mail");

            // Send PIN by mail.
            await _passwordService.CreateEmailTemporaryPasswordPinAsync(user, token);

            return response;
        }

        [AllowAnonymous]
        [HttpPost("temporary-phone")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel))]
        public async Task<IActionResult> RequestTemporaryPasswordPhone([FromBody] PhoneNumberModel model)
        {
            // Validate Model.
            if (!ModelState.IsValid)
                return ValidationProblem();

            // Same response used no matter what.
            var response = Ok(new ResponseModel()
            {
                Status = Status.Success,
                Message = "If user exists and has confirmed email, a temporary password has been send"
            });

            // Check if user can be found by phone.
            var userQuery = await _unitOfWork.ApplicationUserRepository.GetAllAsync(
                filter: user => user.PhoneNumberConfirmed && user.PhoneNumber == model.PhoneNumber);
            ApplicationUser? user = userQuery.FirstOrDefault();
            if (user is null)
                return response;

            // Generate PIN code.
            string token = await _userManager.GenerateUserTokenAsync(
                user, SecurityPinProvider.TotpProvider, "Time-based one-time password by phone");

            // Send PIN by mail.
            _passwordService.CreatePhoneTemporaryPasswordPinAsync(user, token);

            return response;
        }
    }
}
