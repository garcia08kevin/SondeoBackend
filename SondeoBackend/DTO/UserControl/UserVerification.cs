using System.ComponentModel.DataAnnotations;

namespace SondeoBackend.DTO.UserControl
{
    public class UserVerification
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string OldPassword { get; set; }
        [Required]
        public string Password { get; set; }
        [Required, Compare("Password")]
        public string ConfirmPassword { get; set; }
    }
}
