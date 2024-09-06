using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Spike.DataAccess.Entities;
using Spike.DataAccess.Interface;

namespace Spike.DataAccess.Implementation
{
    public class UserRepository : IUserRepository
    {
        private readonly SpikeDbContext _spikeDbContext;
        private readonly UserManager<SpikeUser> _spikeUser;        
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(SpikeDbContext spikeDbContext,
                              UserManager<SpikeUser> spikeUser,                               
                              ILogger<UserRepository> logger)
        {
            _spikeDbContext = spikeDbContext;
            _spikeUser = spikeUser;            
            _logger = logger;
        }

        public async Task<IList<SpikeUser>> GetExistingUsers()
        {
            return await _spikeUser.GetUsersInRoleAsync("User");

        }

        public async Task<IList<string>> GetUserRoles(string email)
        {
            var user = await _spikeUser.FindByEmailAsync(email);

            return await _spikeUser.GetRolesAsync(user);            
        }

        public async Task<(IdentityResult, string)> RegisterNewUser(SpikeUser user)
        {
            try
            {
                var existingUser = await _spikeUser.FindByEmailAsync(user.Email);
                if (existingUser != null)
                {
                    return (new IdentityResult(), "");
                }

                var identityResult = await _spikeUser.CreateAsync(user);

                if (identityResult != null && identityResult.Succeeded)
                {
                    //Assigning the role to the new user
                    await _spikeUser.AddToRoleAsync(user, "User");

                    var createdUser = await _spikeUser.FindByEmailAsync(user.Email);

                    return (identityResult, createdUser.Id);
                }

                return (new IdentityResult(), string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Some error has occured :\n {ex.Message}");
                return (new IdentityResult(), string.Empty);
            }            
        }
    }
}
