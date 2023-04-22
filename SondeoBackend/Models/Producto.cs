using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace SondeoBackend.Models
{
    public class Producto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        [DefaultValue(false)]
        public bool Activado { get; set; }
        [ForeignKey("Categoria")]
        public int CategoriaId { get; set; }
        public Categoria? Categoria { get; set; }
        [ForeignKey("Marca")]

        public int MarcaId { get; set; }
        public Marca? Marca { get; set; }
        [ForeignKey("Propiedades")]

        public int PropiedadesId { get; set; }
        public Propiedades? Propiedades { get; set; }
    }
}
