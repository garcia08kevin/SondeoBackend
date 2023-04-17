namespace SondeoBackend.Models
{
    public class DetalleEncuesta
    {
        public int Id { get; set; }
        public int StockInicial { get; set; }
        public int StockFinal { get; set; }
        public decimal Pdv { get; set; }
        public decimal Pvp { get; set; }
        public decimal Compra { get; set; }
        public ICollection<Producto> Productos { get; set; }
        public int EncuestaId { get; set; }
        public Encuesta Encuesta { get; set; }
    }
}
