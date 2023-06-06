using Microsoft.Extensions.Hosting;
using SondeoBackend.Configuration;
using System.ComponentModel.DataAnnotations.Schema;

namespace SondeoBackend.Models
{
    public class Encuesta
    {
        public int Id { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaCierre { get; set; }
        public int? DiasTrabajados { get; set; }
        [ForeignKey("CustomUserId")]
        public CustomUser? CustomUser { get; set; }
        public int? CustomUserId { get; set; }

        [ForeignKey("LocalId")]
        public Local? Local { get; set; }
        public int? LocalId { get; set; }
        [ForeignKey("MedicionId")]
        public Medicion? Medicion { get; set; }
        public int? MedicionId { get; set; }
        public string SyncId { get; set; }
        public IEnumerable<DetalleEncuesta>? DetalleEncuestas { get; set; }
    }
}
