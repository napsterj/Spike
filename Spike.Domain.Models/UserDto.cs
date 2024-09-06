
using System.ComponentModel.DataAnnotations;

namespace Spike.Domain.Models
{
    public class UserDto
    {        
        public int Id { get; set; }

        [Required]
        public string FirstName { get; set; }

        public string MiddleName { get; set; }
        
        [Required]
        public string LastName { get; set; }

        [Required]
        public string Email { get; set; }                      

    }
}
