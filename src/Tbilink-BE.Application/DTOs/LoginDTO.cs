using System.ComponentModel.DataAnnotations;

namespace Tbilink_BE.Application.DTOs
{
    public class LoginDTO
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
