namespace SondeoBackend.Models
{
    public class Local
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Direccion { get; set; }
        public float Latitud { get; set; }
        public float Longitud { get; set; }
        public Canal Canal { get; set; }
        public Ciudad Ciudad { get; set; }
        public int EncuestaId { get; set; }
        public Encuesta Encuesta { get; set; }
    }
}
