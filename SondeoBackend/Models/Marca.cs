namespace SondeoBackend.Models
{
    public class Marca
    {
        public int Id { get; set; }
        public string Nombre_Marca { get; set; }
        public int ProductoID { get; set; }
        public Producto Producto { get; set; }
    }
}
