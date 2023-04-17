namespace SondeoBackend.Models
{
    public class Encuesta
    {
        public int Id { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaCierre { get; set; }
        public int DiasTrabajados { get; set; }
        public Medicion Medicion { get; set; }
        public Local Local { get; set; }
        public DetalleEncuesta DetalleEncuesta { get; set; }
        public int CustomUserId { get; set; }
        public CustomUser CustomUser { get; set; }
    }
}
