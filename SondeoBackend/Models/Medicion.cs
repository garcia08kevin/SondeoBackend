using System.ComponentModel.DataAnnotations.Schema;

namespace SondeoBackend.Models
{
    public class Medicion
    {
        public int Id { get; set; }
        [ForeignKey("MesId")]
        public Mes? Meses { get; set; }
        public int MesId { get; set; }
        [ForeignKey("EncuestaId")]
        public Encuesta Encuesta { get; set; }
        public int EncuestaId { get; set; }
    }
}
