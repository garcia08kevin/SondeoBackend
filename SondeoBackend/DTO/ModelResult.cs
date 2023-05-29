using System.Collections;

namespace SondeoBackend.DTO
{
    public class ModelResult
    {
        public string Token { get; set; }
        public bool Result { get; set; }
        public List<string> Errors { get; set; }
        public string Contenido { get; set; }
    }
}
