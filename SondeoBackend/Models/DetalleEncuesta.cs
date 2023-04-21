using System.ComponentModel.DataAnnotations.Schema;

namespace SondeoBackend.Models
{
    public class DetalleEncuesta
    {
        public int Id { get; set; }
        public int StockInicial { get; set; }
        public int StockFinal { get; set; }
        public float Compra { get; set; }
        public float Pvd { get; set; }
        public float Pvp { get; set; }
        [ForeignKey("Encuesta")]

        public int EncuestaId { get; set; }
        public Encuesta? Canal { get; set; }
        [ForeignKey("Producto")]
        public int ProductoId { get; set; }
        public Producto Producto { get; set; }
    }
}
