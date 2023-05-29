using System.ComponentModel.DataAnnotations.Schema;

namespace SondeoBackend.Models
{
    public class Medicion
    {
        public int? Id { get; set; }
        public bool Finalizada { get; set; }
        public DateTime FechaRealizada { get; set; }
        [ForeignKey("LocalesId")]
        public Local? Local { get; set; }
        public int? LocalesId { get; set; }
    }
}
