using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SondeoBackend.Configuration;

namespace SondeoBackend.Models
{
    public class Producto
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)] 
        public long? BarCode { get; set; }
        public string? Nombre { get; set; }
        public byte[]? Imagen { get; set; }
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
    }
}
