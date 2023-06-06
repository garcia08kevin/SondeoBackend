using System.ComponentModel.DataAnnotations.Schema;

namespace SondeoBackend.Models
{
    public class Medicion
    {
        public int Id { get; set; }
        public string? nombreMedicion { get; set; }
        [ForeignKey("CiudadId")]
        public Ciudad? Ciudad { get; set; }
        public int? CiudadId { get; set; }
        public bool Activa { get; set; }
        public IEnumerable<Encuesta>? Encuestas { get; set; }
    }
}
