﻿using SondeoBackend.Models;

namespace SondeoBackend.DTO.Sincronizacion
{
    public class EnviarDatos
    {
        public class Transferir
        {
            public int Id_encuestador { get; set; }
            public int Id_Encuesta { get; set; }
        }
        public class PeticionEncuestas
        {
            public int MedicionId { get; set; }
            public int UsuarioId { get; set; }
        }
        public class PeticionLocales
        {
            public int Id_encuestador { get; set; }
        }
        public class LocalesByUser
        {
            public List<Local> locales { get; set; }
        }
        public class SendSyncDto
        {
            public List<EnviarLocalesDto>? Locales { get; set; }
            public List<EnviarEncuestasDto>? Encuestas { get; set; }
            public List<ProductoDto>? Productos { get; set; }
            public List<DetalleEncuestaDto>? DetalleEncuestas { get; set; }
        }
        public class EnviarLocalesDto
        {
            public int Id { get; set; }
            public int Id_canal { get; set; }
            public string? Nombre { get; set; }
            public string? Direccion { get; set; }
            public double Latitud { get; set; }
            public double Longitud { get; set; }            
            public bool Habilitado { get; set; }
        }

        public class EnviarLocalPrueba
        {
            public List<EnviarLocalesDto>? Locales { get; set; }
        }

        public class EnviarEncuestasDto
        {
            public long Id { get; set; }
            public int Id_encuestador { get; set; }
            public int Id_local { get; set; }
            public int Id_medicion { get; set; }
            public string? Fecha_init { get; set; }
            public string? Fecha_cierre { get; set; }
            public int Dias_trabajados { get; set; }
            public string? Visita { get; set; }
            public bool Habilitado { get; set; }
            public IEnumerable<DetalleEncuesta>? DetalleEncuestas { get; set; }
        }

        public class PropiedadDto
        {
            public long Id { get; set; }
            public string? PropiedadesNombre { get; set; }
        }

        public class ProductoDto
        {
            public long Id { get; set; }
            public int Id_categoria { get; set; }
            public int Id_marca { get; set; }
            public long id_propiedades { get; set; }
            public string? propiedad { get; set; }
            public string? Foto { get; set; }
        }

        public class DetalleEncuestaDto
        {
            public long Id { get; set; }
            public int Id_encuesta { get; set; }
            public long Id_producto { get; set; }
            public int Stock_init { get; set; }
            public int Compra { get; set; }
            public int Stock_fin { get; set; }
            public float Pvd { get; set; }
            public float Pvp { get; set; }
        }
    }
}
