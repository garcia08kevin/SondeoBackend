using System.ComponentModel.DataAnnotations.Schema;

namespace SondeoBackend.Models
{
    public class Medicion
    {
        public int? Id { get; set; }
        public bool Activada { get; set; }
        public int? Month { get; set; }
        public int? Year { get; set; }
        [ForeignKey("EncuestaId")]
        public Encuesta? Encuesta { get; set; }
        public int? EncuestaId { get; set; }
    }
}
