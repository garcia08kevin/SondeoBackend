using System.ComponentModel.DataAnnotations;

namespace SondeoBackend.DTO
{
    public class UserRegistration
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Lastname { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Role { get; set; }
    }
}
