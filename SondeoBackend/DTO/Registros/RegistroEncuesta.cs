﻿namespace SondeoBackend.DTO.Registros
{
    public class RegistroEncuesta
    {
        public int Id { get; set; }
        public DateTime FechaInicio { get; set; }
        public int UserId { get; set; }
        public int LocalId { get; set; }
        public int MedicionId { get; set; }
    }
}
