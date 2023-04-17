namespace SondeoBackend.Models
{
    public class Ciudad
    {
        public int Id { get; set; }
        public string NombreCuidad { get; set; }
        public int LocalId { get; set; }
        public Local Local { get; set; }    }
}
