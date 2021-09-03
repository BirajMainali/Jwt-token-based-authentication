using System.ComponentModel.DataAnnotations;

namespace WebApplication.Dto.Request
{
    public class UserLoginRequest
    {
        [Required]
        [EmailAddress] 
        public string Email { get; set; }
        [Required] 
        public string Password { get; set; }
    }
}