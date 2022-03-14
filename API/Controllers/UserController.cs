using Data.Entities.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models.ResponseModels;
using Models.SingleDataModels;
using Models.UserModels;
using System.Security.Claims;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        public UserController(
            ILogger<UserController> logger,
            UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _userManager = userManager;
        }

        [Authorize]
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PublicUserModel))]
        public async Task<IActionResult> Get(Guid id)
        {
            ApplicationUser user = await _userManager.FindByIdAsync(id.ToString());
            if (user is null)
            {
                _logger.LogWarning(
                    eventId: 0101,
                    exception: new Exception(string.Format("User with id; {0} was requsted but not found.", id.ToString())),
                    message: "User requested but not found.");
                return NotFound();
            }

            if (User.FindFirstValue(ClaimTypes.NameIdentifier) == user.Id.ToString())
                return Ok(new PrivateUserModel(user));
            else
                return Ok(new PublicUserModel(user));
        }

        [Authorize]
        [HttpPost("phone")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResponseModel))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResponseModel))]
        public async Task<IActionResult> SetPhone([FromBody] PhoneNumberModel model)
        {
            // Validate Model.
            if (!ModelState.IsValid)
                return ValidationProblem();

            // Get current ApplicationUser.
            ApplicationUser user = await _userManager.GetUserAsync(User);
            if (user == null)
                return BadRequest(new ResponseModel() { Status = Status.InvalidRequest, Message = $"Invalid request." });

            // No change needed.
            if (user.PhoneNumber == model.PhoneNumber)
                return Ok(new ResponseModel() { Status = Status.Success, Message = $"Phone number updated." });

            // Set values and update user.
            user.PhoneNumber = model.PhoneNumber;
            user.PhoneNumberConfirmed = false;
            IdentityResult result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                // TODO : Come back around to this later..
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new ResponseModel() { Status = Status.UnknownError, Message = $"Internal error." });
            }

            // Return success 
            return Ok(new ResponseModel() { Status = Status.Success, Message = $"Phone number updated." });
        }
    }
}
