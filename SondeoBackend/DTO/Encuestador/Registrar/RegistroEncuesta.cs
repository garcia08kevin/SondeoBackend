namespace SondeoBackend.DTO.Encuestador.Registrar
{
    public class RegistroEncuesta
    {
        public string Id { get; set; }
        public DateTime FechaInicio { get; set; }
        public int UserId { get; set; }
        public int LocalId { get; set; }
        public int MedicionId { get; set; }
    }
}
