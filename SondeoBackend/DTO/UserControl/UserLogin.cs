using System.ComponentModel.DataAnnotations;

namespace SondeoBackend.DTO.UserControl
{
    public class UserLogin
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
