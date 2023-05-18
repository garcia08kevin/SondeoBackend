using System.ComponentModel.DataAnnotations.Schema;

namespace SondeoBackend.Models
{
    public class RegistroProducto
    {
        public string Nombre { get; set; }
        public int CategoriaId { get; set; }
        public int MarcaId { get; set; }
        public int PropiedadesId { get; set; }
        public int CustomUserId { get; set; }
    }
}
