using Microsoft.AspNetCore.Identity;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SondeoBackend.Configuration
{
    public class CustomUserRole : IdentityUserRole<int> { }
    public class CustomUserClaim : IdentityUserClaim<int> { }
    public class CustomUserLogin : IdentityUserLogin<int> { }

    public class CustomRole : IdentityRole<int>
    {
        public CustomRole() { }
        public CustomRole(string name) { Name = name; }
    }
    public class CustomUser : IdentityUser<int>
    {
        [Required]
        [DefaultValue(true)]
        public bool CuentaActiva { get; set; }
        [Required]
        public string? Name { get; set; }
        public byte[]? Imagen { get; set; }
        [Required]
        public string? Lastname { get; set; }
        public string? Alias { get; set; }
    }
}
