using System.ComponentModel.DataAnnotations.Schema;
using System.IO;

namespace SondeoBackend.DTO.Encuestador.Registrar
{
    public class RegistroProducto
    {
        public string Nombre { get; set; }
        public int CategoriaSyncId { get; set; }
        public string MarcaSyncId { get; set; }
        public string PropiedadesSyncId { get; set; }
        public IFormFile Image { get; set; }
    }
}
