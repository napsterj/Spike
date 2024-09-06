using Spike.Domain.Models;
using Spike.Domain.Models.Response;

namespace Spike.Services.Interface
{
    public interface IUserService
    {
        Task<AuthenticationResponse> AuthenticateUser(LoginDto login);
        Task<RegistrationResponse> RegisterNewUser(UserDto user);
        Task<List<UserInfoResponse>> ListExistingUsers();
        Task<IList<string>> GetUserRoles(string email);
    }
}
