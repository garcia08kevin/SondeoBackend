namespace SondeoBackend.DTO
{
    public class RegistroProductoEncuesta
    {
        public int StockInicial { get; set; }
        public float Compra { get; set; }
        public float Pvd { get; set; }
        public float Pvp { get; set; }
        public int EncuestaId { get; set; }
        public int ProductoId { get; set; }
    }
}
