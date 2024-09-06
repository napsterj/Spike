using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Spike.Domain.Models;
using Spike.Domain.Models.Response;
using Spike.Services;
using Spike.Services.Interface;
using SpikeApi.Common;
using System.Net;

namespace SpikeApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;
        private readonly IJwtTokenFactory _jwtTokenFactory;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IUserService userService, 
                                 IConfiguration configuration, 
                                 IJwtTokenFactory jwtTokenFactory, 
                                 ILogger<AccountController> logger)
        {
            _userService = userService;
            _configuration = configuration;
            _jwtTokenFactory = jwtTokenFactory;
            _logger = logger;
        }

        [Authorize(Policy = "CheckRole")]
        [HttpGet]
        [Route("get/users")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                string token = Request.Headers[HeaderNames.Authorization].ToString();

                if (string.IsNullOrWhiteSpace(token)) return Forbid(CommonConstants.INVALID_TOKEN);

                var result = HandleJwtTokenOperations(operationType: CommonConstants.TOKEN_VALIDATION, token: token);

                if (result != CommonConstants.SUCCESSFUL) { return GetSpecificError(result); }

                var existingUsers = await _userService.ListExistingUsers();

                if (existingUsers == null || existingUsers.Count == 0)
                {
                    return NotFound("No users found in the database.");
                }

                return Ok(existingUsers);
            }
            catch (Exception ex) 
            {
                _logger.LogError($"{CommonConstants.SOME_ERROR_OCCURRED} :\n {ex.Message}");
                return BadRequest();
            }
        }


        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto login)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(login.Email) || string.IsNullOrWhiteSpace(login.Password))
                {
                    return BadRequest($"Email or Password {CommonConstants.EMPTY}");
                }

                login.Password = PasswordHasher.HashUserPassword(login.Password, _configuration["PasswordSalt"]);

                var response = await _userService.AuthenticateUser(login);

                if (response.StatusMessage != CommonConstants.SUCCESSFUL_LOGIN)
                {
                    return Unauthorized(response);
                }

                var token = Request.Headers[HeaderNames.Authorization];

                var userRole = (await _userService.GetUserRoles(login.Email)).FirstOrDefault();

                response.jwtToken = HandleJwtTokenOperations(string.IsNullOrWhiteSpace(token) ? CommonConstants.TOKEN_CREATION
                                                                                              : CommonConstants.TOKEN_VALIDATION,
                                                             login.Email, userRole ?? "Admin");

                return Ok(response);
            }
            catch(Exception ex)
            {
                _logger.LogError($"{CommonConstants.SOME_ERROR_OCCURRED} :\n {ex.ToString()}");
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

        [Authorize(Policy = "CheckRole")]
        [HttpPost]
        [Route("register/new-user")]
        public async Task<IActionResult> RegisterUser([FromBody] UserDto userDto)
        {
            try
            {
                string token = Request.Headers[HeaderNames.Authorization].ToString();

                if (string.IsNullOrWhiteSpace(token)) return Forbid(CommonConstants.INVALID_TOKEN);

                var result = HandleJwtTokenOperations(operationType: CommonConstants.TOKEN_VALIDATION, token: token);

                if (result != CommonConstants.SUCCESSFUL) { return GetSpecificError(result); }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                                           .SelectMany(validations => validations.Errors)
                                           .Select(mserrors => mserrors.ErrorMessage)
                                           .ToArray();

                    return BadRequest(new RegistrationResponse { ModelStateErrors = errors });
                }
                
                var response = await _userService.RegisterNewUser(userDto);

                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    return BadRequest(response);
                }

                return Ok(response);
            }
            catch (Exception ex) 
            {
                _logger.LogError($"{CommonConstants.SOME_ERROR_OCCURRED} :\n {ex.Message}");
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

        private string HandleJwtTokenOperations(string operationType, 
                                                string email="", 
                                                string roleName = "", 
                                                string token="")
        {
            return operationType switch
            {
                CommonConstants.TOKEN_CREATION => _jwtTokenFactory.GenerateToken(email, roleName),
                CommonConstants.TOKEN_VALIDATION => _jwtTokenFactory.ValidateToken(token).ToString(),
                _ => string.Empty,
            };
        }

        private IActionResult GetSpecificError(string result)
        {
            if (result == CommonConstants.INVALID_JWT_TOKEN) { return Forbid(result); }

            if (result == CommonConstants.UNAUTHORIZED_USER) { return Unauthorized(result); }

            if (result == CommonConstants.SOME_ERROR_OCCURRED) { return BadRequest(result); }

            return Ok();
        }
    }
}
