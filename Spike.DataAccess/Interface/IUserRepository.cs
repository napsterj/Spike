using Microsoft.AspNetCore.Identity;
using Spike.DataAccess.Entities;

namespace Spike.DataAccess.Interface
{
    public interface IUserRepository
    {
        Task<(IdentityResult,string)> RegisterNewUser(SpikeUser user);
        Task<IList<SpikeUser>> GetExistingUsers();
        Task<IList<string>> GetUserRoles(string email);
    }
}
