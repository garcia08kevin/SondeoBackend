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
        [ForeignKey("CiudadId")]
        public Ciudad? Ciudad { get; set; }
        public int? CiudadId { get; set; }
        public ICollection<Encuesta>? Encuestas { get; set; }
        public ICollection<Medicion>? Mediciones { get; set; }
    }
}
