namespace SondeoBackend.Models
{
    public class Categoria
    {
        public int Id { get; set; }
        public string Categoria_nombre { get; set; }
        public int ProductoID { get; set; }
        public Producto Producto { get; set; }
    }
}
