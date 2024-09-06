using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Spike.DataAccess.Entities;
using Spike.DataAccess.Interface;

namespace Spike.DataAccess.Implementation
{
    public class LoginRepository : ILoginRepository
    {
        private readonly SpikeDbContext _context;
        private readonly UserManager<SpikeUser> _userManager;
        private readonly SignInManager<SpikeUser> _signInManager;
        private readonly ILogger<LoginRepository> _logger;

        private const string EMAIL_NOT_CONFIRMED = "Email not confirmed";
        private const string INVALID_LOGIN_CREDENTIALS = "Invalid login credentials.";
        private const string ACCOUNT_LOCKED_OUT = "Your account is locked out";
        private const string SOME_ERROR_OCCURED = "Some error has occured";
        public const string SUCCESSFUL_LOGIN = "Successful Login";

        public LoginRepository(SpikeDbContext context, 
                               UserManager<SpikeUser> userManager, 
                               SignInManager<SpikeUser> signInManager, 
                               ILogger<LoginRepository> logger)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        public async Task<(SignInResult, string error)> AuthenticateUser(string email, string password)
        {
            var spikeUser = await _userManager.FindByEmailAsync(email);

            if (spikeUser == null)
            {
                return (new SignInResult(),string.Empty);
            }

            try
            {
                var signInResult = await _signInManager.PasswordSignInAsync(spikeUser, password, false, false);

                if (!signInResult.Succeeded)
                {
                    if (!spikeUser.EmailConfirmed)
                    {
                        _logger.LogError(EMAIL_NOT_CONFIRMED);
                        return (signInResult, EMAIL_NOT_CONFIRMED);
                    }
                    else if (spikeUser.PasswordHash != password || spikeUser.Email != email)
                    {
                        _logger.LogError(INVALID_LOGIN_CREDENTIALS);
                        return (signInResult, INVALID_LOGIN_CREDENTIALS);
                    }
                    else if (signInResult.IsLockedOut)
                    {
                        _logger.LogError(ACCOUNT_LOCKED_OUT);
                        return (signInResult, ACCOUNT_LOCKED_OUT);
                    }
                }

                return (signInResult, SUCCESSFUL_LOGIN);

            }
            catch (Exception ex) 
            {
                _logger.LogError($"Some error has occured :\n {ex.Message}");
                return (new SignInResult(),SOME_ERROR_OCCURED);
            }            
        }
    }
}
