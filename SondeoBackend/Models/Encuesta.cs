﻿using Microsoft.Extensions.Hosting;
using SondeoBackend.Configuration;
using System.ComponentModel.DataAnnotations.Schema;

namespace SondeoBackend.Models
{
    public class Encuesta
    {
        public long Id { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaCierre { get; set; }
        public int DiasTrabajados { get; set; }
        public string? Visita { get; set; }
        [ForeignKey("CustomUserId")]
        public CustomUser? CustomUser { get; set; }
        public int CustomUserId { get; set; }

        [ForeignKey("LocalId")]
        public Local? Local { get; set; }
        public int LocalId { get; set; }
        [ForeignKey("MedicionId")]
        public Medicion? Medicion { get; set; }
        public int MedicionId { get; set; }
        public IEnumerable<DetalleEncuesta>? DetalleEncuestas { get; set; }
    }
}
