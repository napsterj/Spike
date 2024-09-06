using Microsoft.AspNetCore.Identity;

namespace Spike.DataAccess.Interface
{
    public interface ILoginRepository
    {
        Task<(SignInResult, string error)> AuthenticateUser(string email, string password);
    }
}
