using SondeoBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace SondeoBackend.DTO.Result
{
    public class ObjectResult<T>
    {
        public bool Result { get; set; }
        public string Respose { get; set; }
        public T Object { get; set; }
        public List<T> ListObject { get; set; }
    }
    public class LocalDto
    {
        public int Id { get; set; }
        public string? Nombre { get; set; }
        public string? Direccion { get; set; }
        public float Latitud { get; set; }
        public float Longitud { get; set; }
        public string? Canal { get; set; }
        public string? Ciudad { get; set; }
        public bool Habilitado { get; set; }
    }
}
