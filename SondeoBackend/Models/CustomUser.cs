using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace SondeoBackend.Models
{
    public class CustomUser : IdentityUser<int>
    {
        [Required]
        [DefaultValue(true)]
        public bool CuentaActiva { get; set; }
        [Required]
        public string? Name { get; set; }
        [Required]
        public string? Lastname { get; set; }
    }
}
