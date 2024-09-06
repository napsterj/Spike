using Spike.DataAccess.Entities;
using Spike.DataAccess.Interface;
using Spike.Domain.Models;
using Spike.Domain.Models.Response;
using Spike.Services.Interface;
using System.Net;

namespace Spike.Services.Implementation
{
    public class UserService : IUserService
    {
        private readonly ILoginRepository _loginRepository;
        private readonly IUserRepository _userRepository;

        public UserService(ILoginRepository loginRepository, IUserRepository userRepository)
        {
            _loginRepository = loginRepository;
            _userRepository = userRepository;
        }

        public async Task<AuthenticationResponse> AuthenticateUser(LoginDto login)
        {
           var (result,error) =  await _loginRepository.AuthenticateUser(login.Email, login.Password);

            var isSuccessful = result != null && result.Succeeded && string.IsNullOrWhiteSpace(error);            

            return new AuthenticationResponse
            {
                StatusMessage = isSuccessful ? CommonConstants.SUCCESSFUL_LOGIN
                                                                   : error,

                IsAuthenticated = isSuccessful,

                StatusCode = isSuccessful ? HttpStatusCode.OK : HttpStatusCode.Unauthorized,
            };

        }

        public Task<IList<string>> GetUserRoles(string email)
        {            
            return _userRepository.GetUserRoles(email);
        }

        public async Task<List<UserInfoResponse>> ListExistingUsers()
        {
            var users = await _userRepository.GetExistingUsers();

            if(users != null && users.Any()) 
            { 
                return users.Select(user => new UserInfoResponse
                {
                    FullName = $"{user.FirstName} {user.MiddleName} {user.LastName}",
                    Email = user.Email,

                }).ToList();
            }

            return [];
        }

        public async Task<RegistrationResponse> RegisterNewUser(UserDto user)
        {
            var spikeUser = new SpikeUser
            {
                Email = user.Email,
                EmailConfirmed = true,                
                UserName = user.Email,
                FirstName = user.FirstName,
                MiddleName = user.MiddleName,
                LastName = user.LastName
            };
            
            var (identityResult,id) = await _userRepository.RegisterNewUser(spikeUser);

            if((identityResult == null || !identityResult.Succeeded) && string.IsNullOrWhiteSpace(id))
            {
                return new RegistrationResponse { StatusCode = HttpStatusCode.BadRequest, StatusMessage=CommonConstants.USER_ALREADY_REGISTERED, UserId = ""};
            }

            return new RegistrationResponse
            {
                StatusCode = HttpStatusCode.OK,
                StatusMessage = CommonConstants.USER_CREATED_SUCCESSFULLY,
                UserId = id
            };

        }
    }
}
