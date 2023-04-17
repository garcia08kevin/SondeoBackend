namespace SondeoBackend.Models
{
    public class Propiedades
    {
        public int Id { get; set; }
        public string Propiedades_nombre { get; set; }
        public int ProductoID { get; set; }
        public Producto Producto { get; set; }
    }
}
