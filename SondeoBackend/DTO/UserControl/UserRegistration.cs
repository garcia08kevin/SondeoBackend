using System.ComponentModel.DataAnnotations;

namespace SondeoBackend.DTO.UserControl
{
    public class UserRegistration
    {
        [Required]
        public string? Name { get; set; }
        [Required]
        public string? Lastname { get; set; }
        [Required]
        public string? UserName { get; set; }
        public string? Email { get; set; }
        [Required]
        public string? Role { get; set; }
    }
}
