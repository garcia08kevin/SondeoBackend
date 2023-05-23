using System.ComponentModel.DataAnnotations.Schema;

namespace SondeoBackend.Models
{
    public class Local
    {
        public int? Id { get; set; }
        public string? Nombre { get; set; }
        public string? Direccion { get; set; }
        public float? Latitud { get; set;}
        public float? Longitud { get; set; }
        [ForeignKey("CanalId")]        
        public Canal? Canal { get; set; }
        public int? CanalId { get; set; }
        public Ciudad? Ciudad { get; set; }
    }
}
