namespace SondeoBackend.DTO.Registros
{
    public class RegistroDetalle
    {
        public int StockInicial { get; set; }
        public float Compra { get; set; }
        public float Pvd { get; set; }
        public float Pvp { get; set; }
        public int EncuestaId { get; set; }
        public long ProductoId { get; set; }
    }
}
