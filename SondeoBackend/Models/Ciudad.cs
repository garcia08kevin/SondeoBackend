namespace SondeoBackend.Models
{
    public class Ciudad
    {
        public int? Id { get; set; }
        public string? NombreCiudad { get; set; }
        public ICollection<Local>? Locales { get; set; }
    }
}
