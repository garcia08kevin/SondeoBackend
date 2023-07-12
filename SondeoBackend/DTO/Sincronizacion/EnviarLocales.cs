using SondeoBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace SondeoBackend.DTO.Sincronizacion
{
    public class EnviarLocales
    {
        public List<EnviarLocalesDto> Locales{ get; set; }
    }

    public class EnviarLocalesDto
    {
        public int Id { get; set; }
        public string? Nombre { get; set; }
        public string? Direccion { get; set; }
        public float Latitud { get; set; }
        public float Longitud { get; set; }   
        public int CanalId { get; set; }
        public int CiudadId { get; set; }
        public bool Habilitado { get; set; }
    }
}
