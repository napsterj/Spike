using Microsoft.AspNetCore.Identity;

namespace Spike.DataAccess.Entities
{
    public class SpikeUser: IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string MiddleName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;        

    }
}
