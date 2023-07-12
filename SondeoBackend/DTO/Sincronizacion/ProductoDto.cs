﻿using SondeoBackend.Models;

namespace SondeoBackend.DTO.Sincronizacion
{
    public class ProductoDto
    {
        public long? BarCode { get; set; }
        public string? Nombre { get; set; }
        public bool Activado { get; set; }
        public int CategoriaId { get; set; }
        public int MarcaId { get; set; }
        public int PropiedadesId { get; set; }
    }
}
