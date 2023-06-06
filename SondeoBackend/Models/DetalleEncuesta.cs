using System.ComponentModel.DataAnnotations.Schema;

namespace SondeoBackend.Models
{
    public class DetalleEncuesta
    {
        public int Id { get; set; }
        public int? StockInicial { get; set; }
        public int? StockFinal { get; set; }
        public float? Compra { get; set; }
        public float? Pvd { get; set; }
        public float? Pvp { get; set; }
        public string SyncId { get; set; }
        [ForeignKey("EncuestaId")]
        public Encuesta? Encuesta { get; set; }
        public int? EncuestaId { get; set; }
        [ForeignKey("ProductoId")]
        public Producto? Producto { get; set; }
        public int ProductoId { get; set; }
    }
}
