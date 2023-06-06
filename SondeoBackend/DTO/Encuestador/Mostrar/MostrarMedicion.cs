namespace SondeoBackend.DTO.Encuestador.Mostrar
{
    public class MostrarMedicion
    {
        public int Id { get; set; }
        public string NombreMedicion { get; set; }
        public bool Activa { get; set; }
        public int CiudadId { get; set; }
    }
}
