using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace SondeoBackend.Models
{
    public class CustomUser : IdentityUser
    {
        [Required]
        [DefaultValue(false)]
        public bool CuentaActiva { get; set; }
        public ICollection<Encuesta> Encuestas { get; set; }
    }
}
