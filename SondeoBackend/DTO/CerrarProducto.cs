namespace SondeoBackend.DTO
{
    public class CerrarProducto
    {
        public int Id { get; set; }
        public int? StockInicial { get; set; }
        public int? StockFinal { get; set; }
        public float? Compra { get; set; }
        public float? Pvd { get; set; }
        public float? Pvp { get; set; }
    }
}
