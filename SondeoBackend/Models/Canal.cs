namespace SondeoBackend.Models
{
    public class Canal
    {
        public int Id { get; set; }
        public string NombreCanal { get; set; }
        public int LocalId { get; set; }
        public Local Local { get; set; }
    }
}
