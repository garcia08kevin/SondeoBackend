using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using SondeoBackend.Configuration;

namespace SondeoBackend.Models
{
    public class Producto
    {
        public int Id { get; set; }
        public string? Nombre { get; set; }
        [DefaultValue(false)]
        public bool Activado { get; set; }
        [ForeignKey("CategoriaId")]        
        public Categoria? Categoria { get; set; }
        public int CategoriaId { get; set; }
        [ForeignKey("MarcaId")]
        public Marca? Marca { get; set; }
        public int MarcaId { get; set; }

        [ForeignKey("PropiedadesId")]
        public Propiedades? Propiedades { get; set; }
        public int PropiedadesId { get; set; }

        [ForeignKey("CustomUserId")]
        public CustomUser? User { get; set; }
        public int CustomUserId { get; set; }        
    }
}
