namespace SondeoBackend.Models
{
    public class Medicion
    {
        public int Id { get; set; }
        public string NombreMedicion { get; set; }
        public int EncuestaId { get; set; }
        public Encuesta Encuesta { get; set; }
    }
}
