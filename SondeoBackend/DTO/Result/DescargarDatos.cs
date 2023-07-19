namespace SondeoBackend.DTO.Result
{
    public class DescargarDatos
    {
        public string NombreMedicion { get; set; }
        public string NombreEncuestador { get; set; }
        public string LocalEncuestado { get; set; }
        public string FechaInicio { get; set; }
        public string FechaCierre { get; set; }
        public int DiasTrabajados { get; set; }
        public string NombreProducto { get; set; }
        public long? CodigoProducto { get; set; }
        public string ProductoCategoria { get; set; }
        public string ProductoMarca { get; set; }
        public string ProductoPropiedad { get; set; }
        public int? StockInicial { get; set; }
        public int? StockFinal { get; set; }
        public float? Compra { get; set; }
        public float? Pvd { get; set; }
        public float? Pvp { get; set; }
    }
}
