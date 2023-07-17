using SondeoBackend.Models;

namespace SondeoBackend.DTO.Sincronizacion
{
    public class ProductoDtoResponse
    {
        public long? BarCode { get; set; }
        public string? Nombre { get; set; }
        public bool Activado { get; set; }
        public int CategoriaId { get; set; }
        public int MarcaId { get; set; }
        public long PropiedadesId { get; set; }
    }
}
