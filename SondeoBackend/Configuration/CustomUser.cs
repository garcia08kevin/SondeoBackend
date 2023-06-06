using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace SondeoBackend.Configuration
{
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
