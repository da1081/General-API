
using Data;
using Data.Entities.Identity;
using Data.TokenProviders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models.AuthenticationModels;
using Models.ResponseModels;
using Models.UserModels;
using Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly ILogger<AuthenticationController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IAuthenticationService _authenticationService;
        private readonly IUnitOfWork _unitOfWork;

        public AuthenticationController(
            ILogger<AuthenticationController> logger,
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration,
            IAuthenticationService authenticationService,
            IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _userManager = userManager;
            _configuration = configuration;
            _authenticationService = authenticationService;
            _unitOfWork = unitOfWork;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PrivateUserModel))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ActionResult))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResponseModel))]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            // Validate Model.
            if (!ModelState.IsValid)
                return ValidationProblem();

            // Validate Username.
            var userByName = await _userManager.FindByNameAsync(model.Username);
            if (userByName is not null)
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ResponseModel { Status = Status.UsernameNotAvailableError, Message = "A user with this username already exists." });

            // Validate Email.
            var userByMail = await _userManager.FindByEmailAsync(model.Email);
            if (userByMail is not null)
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ResponseModel { Status = Status.EmailNotAvailableError, Message = "A user with this email already exists." });

            // Create new User.
            var result = await _userManager.CreateAsync(new ApplicationUser { UserName = model.Username, Email = model.Email }, model.Password);
            if (!result.Succeeded)
            {
                _logger.LogError(
                    eventId: 0001,
                    exception: new Exception(string.Format("Error/s: {0}", result.Errors)),
                    message: "Unknown error/s has prevented a new user to be created.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ResponseModel { Status = Status.UnknownError, Message = "An unknown error has caused the user creation to fail." });
            }

            // Get new user.
            ApplicationUser user = await _userManager.FindByNameAsync(model.Username);

            // New user is created respond accordingly.
            return Ok(new PrivateUserModel(user));
        }

        [AllowAnonymous]
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TokenResponseModel))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ActionResult))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResponseModel))]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            // Validate Model.
            if (!ModelState.IsValid)
                return ValidationProblem();

            // Get user.
            ApplicationUser user = await _userManager.FindByNameAsync(model.Username);
            if (user != null)
            {
                // Apply locked out user feature.
                if (await _authenticationService.IsUserLockedoutAsync(user))
                    return StatusCode(StatusCodes.Status401Unauthorized, new ResponseModel
                    {
                        Status = Status.LockedOutError,
                        Message = $"Too many failed attempts to login. Try again in {DateTimeOffset.UtcNow - user.LockoutEnd}"
                    });

                if (await _userManager.CheckPasswordAsync(user, model.Password))
                {
                    // Create Json Web Token instance.
                    JwtSecurityToken rawToken = await _authenticationService
                        .CreateJwtSecurityTokenAsync(user,
                            _configuration["AppSettings:ValidIssuer"],
                            _configuration["AppSettings:ValidAudience"],
                            _configuration["AppSettings:Secret"]);

                    // Write JwtSecurityToken to string.
                    string token = new JwtSecurityTokenHandler().WriteToken(rawToken);

                    // Reset login attempts.
                    await _userManager.ResetAccessFailedCountAsync(user);

                    // Return token to user.
                    return Ok(new TokenResponseModel() { Token = token, Expiration = rawToken.ValidTo });
                }

                // Handle failed login attempt.
                await _userManager.AccessFailedAsync(user);
            }
            return Unauthorized();
        }

        [AllowAnonymous]
        [HttpPost("totp-mail")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TokenResponseModel))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ActionResult))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResponseModel))]
        public async Task<IActionResult> TimeBasedOneTimeLoginMail([FromBody] TotpLoginMailModel model)
        {
            // Validate Model.
            if (!ModelState.IsValid)
                return ValidationProblem();

            // Get user.
            ApplicationUser user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                // Apply locked out user feature.
                if (await _authenticationService.IsUserLockedoutAsync(user))
                    return StatusCode(StatusCodes.Status401Unauthorized, new ResponseModel
                    {
                        Status = Status.LockedOutError,
                        Message = $"Too many failed attempts to login. Try again in {DateTimeOffset.UtcNow - user.LockoutEnd}"
                    });

                // Validate Time-based one-time password with the totpmailvalidation.
                bool isValid = await _userManager.VerifyUserTokenAsync(user, SecurityPinProvider.TotpProvider, "Time-based one-time password by mail", model.Pin);
                if (isValid)
                {
                    // Create Json Web Token instance.
                    JwtSecurityToken rawToken = await _authenticationService
                        .CreateJwtSecurityTokenAsync(user,
                            _configuration["AppSettings:ValidIssuer"],
                            _configuration["AppSettings:ValidAudience"],
                            _configuration["AppSettings:Secret"]);

                    // Write JwtSecurityToken to string.
                    string token = new JwtSecurityTokenHandler().WriteToken(rawToken);

                    // Reset login attempts.
                    await _userManager.ResetAccessFailedCountAsync(user);

                    // Return token to user.
                    return Ok(new TokenResponseModel() { Token = token, Expiration = rawToken.ValidTo });
                }

                // Handle failed login attempt.
                await _userManager.AccessFailedAsync(user);
            }
            return Unauthorized();
        }

        [AllowAnonymous]
        [HttpPost("totp-phone")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TokenResponseModel))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ActionResult))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResponseModel))]
        public async Task<IActionResult> TimeBasedOneTimeLoginPhone([FromBody] TotpLoginPhoneModel model)
        {
            // Validate Model.
            if (!ModelState.IsValid)
                return ValidationProblem();

            // Find user by phone-number.
            var userQuery = await _unitOfWork.ApplicationUserRepository.GetAllAsync(filter: user => user.PhoneNumberConfirmed && user.PhoneNumber == model.PhoneNumber);
            ApplicationUser? user = userQuery.FirstOrDefault();
            if (user != null)
            {
                // Apply locked out user feature.
                if (await _authenticationService.IsUserLockedoutAsync(user))
                    return StatusCode(StatusCodes.Status401Unauthorized, new ResponseModel
                    {
                        Status = Status.LockedOutError,
                        Message = $"Too many failed attempts to login. Try again in {DateTimeOffset.UtcNow - user.LockoutEnd}"
                    });

                // Validate Time-based one-time password with the totpphonevalidation.
                bool isValid = await _userManager.VerifyUserTokenAsync(user, SecurityPinProvider.TotpProvider, "Time-based one-time password by phone", model.Pin);
                if (isValid)
                {
                    // Create Json Web Token instance.
                    JwtSecurityToken rawToken = await _authenticationService
                        .CreateJwtSecurityTokenAsync(user,
                            _configuration["AppSettings:ValidIssuer"],
                            _configuration["AppSettings:ValidAudience"],
                            _configuration["AppSettings:Secret"]);

                    // Write JwtSecurityToken to string.
                    string token = new JwtSecurityTokenHandler().WriteToken(rawToken);

                    // Reset login attempts.
                    await _userManager.ResetAccessFailedCountAsync(user);

                    // Return token to user.
                    return Ok(new TokenResponseModel() { Token = token, Expiration = rawToken.ValidTo });
                }

                // Handle failed login attempt.
                await _userManager.AccessFailedAsync(user);
            }
            return Unauthorized();
        }

        [Authorize]
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Logout()
        {
            // Best way to handle a logout is to remove the JWT token form the client.
            // Alternative could be handy in other usecases where access has to be removed from an unexpired JWT token.
            //     Use Redis/other-cache to store blacklisted JWT tokens until they expire.
            return Ok();
        }
    }
}
