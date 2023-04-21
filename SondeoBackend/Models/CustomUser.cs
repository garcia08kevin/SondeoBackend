using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace SondeoBackend.Models
{
    public class CustomUser : IdentityUser<int>
    {
        [Required]
        [DefaultValue(false)]
        public bool CuentaActiva { get; set; }
        [Required]
        [DefaultValue("Sin Asignar")]
        public string Name { get; set; }
        [Required]
        [DefaultValue("Sin Asignar")]
        public string Lastname { get; set; }
        //public ICollection<Encuesta> Encuestas { get; set; }
    }
}
