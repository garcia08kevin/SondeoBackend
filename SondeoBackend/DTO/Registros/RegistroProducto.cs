namespace SondeoBackend.DTO.Registros
{
    public class RegistroProducto
    {
        public string Nombre { get; set; }
        public IFormFile? Imagen { get; set; }
        public bool Activado { get; set; }
        public int CategoriaId { get; set; }
        public int MarcaId { get; set; }
        public int PropiedadesId { get; set; }
        public long? BarCode { get; set; }
    }
}
