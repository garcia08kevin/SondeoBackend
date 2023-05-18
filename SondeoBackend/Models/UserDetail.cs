namespace SondeoBackend.Models
{
    public class UserDetail
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Lastname { get; set; }
        public string Role { get; set; }
        public string Email { get; set; }
        public bool Activado { get; set; }
        public bool CorreoActivado { get; set; }
    }
}
