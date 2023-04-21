using System.ComponentModel.DataAnnotations.Schema;

namespace SondeoBackend.Models
{
    public class Medicion
    {
        public int Id { get; set; }
        public string NombreMedicion { get; set; }
        [ForeignKey("Encuesta")]
        public int EncuestaId { get; set; }
        public Encuesta Encuesta { get; set; }

    }
}
