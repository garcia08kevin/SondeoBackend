using System.ComponentModel.DataAnnotations;

namespace SondeoBackend.DTO.UserControl
{
    public class UserLogin
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
