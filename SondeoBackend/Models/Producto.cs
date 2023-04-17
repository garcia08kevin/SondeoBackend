namespace SondeoBackend.Models
{
    public class Producto
    {
        public int Id { get; set; }
        public Categoria Categoria { get; set; }
        public Marca Marca { get; set; }
        public Propiedades Propiedades { get; set; }
        public int DetalleEncuestaId { get; set; }
        public DetalleEncuesta DetalleEncuesta { get; set; }
    }
}
