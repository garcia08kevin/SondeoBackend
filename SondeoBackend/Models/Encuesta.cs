using Microsoft.Extensions.Hosting;
using System.ComponentModel.DataAnnotations.Schema;

namespace SondeoBackend.Models
{
    public class Encuesta
    {
        public int Id { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaCierre { get; set; }
        public int DiasTrabajados { get; set; }
        [ForeignKey("CustomUser")]
        public int CustomUserId { get; set; }
        public CustomUser CustomUser { get; set; }
        [ForeignKey("Encuesta")]
        public int LocalId { get; set; }
        public Local Local { get; set; }
    }
}
