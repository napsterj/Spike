
using System.Net;

namespace Spike.Domain.Models.Response
{
    public class AuthenticationResponse : BaseResponse
    {        
        public string jwtToken { get; set; } = string.Empty;
        public bool IsAuthenticated { get; set; }        
        
    }
}
