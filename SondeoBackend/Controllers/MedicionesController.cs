using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using SondeoBackend.Configuration;
using SondeoBackend.Context;
using SondeoBackend.DTO.Result;
using SondeoBackend.Models;
using System.Collections.Generic;
using System.Globalization;

namespace SondeoBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MedicionesController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly UserManager<CustomUser> _userManager;

        public MedicionesController(DataContext context, UserManager<CustomUser> userManager)
        {
            _userManager = userManager;
            _context = context;
        }

        [Route("HistoricoMediciones/{idCiudad}")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Medicion>>> GetHistoricoMediciones(int idCiudad)
        {
            return await _context.Mediciones
                .Include(e => e.Encuestas).ThenInclude(e => e.CustomUser)
                .Include(e => e.Encuestas).ThenInclude(e => e.Local).Where(e => e.CiudadId == idCiudad).OrderByDescending(m => m.Id).ToListAsync();
        }

        [Route("ExportarMedicion/{idMedicion}")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DescargarDatos>>> ExportarMedicion(int idMedicion)
        {
            List<DescargarDatos> datos = new List<DescargarDatos>();
            var medicion = await _context.Mediciones.Include(e=>e.Encuestas).FirstOrDefaultAsync(e => e.Id == idMedicion);
            foreach(Encuesta encuesta in medicion.Encuestas)
            {
                var encuestaData = await _context.Encuestas.Include(e => e.DetalleEncuestas).Include(e => e.Local).Include(e => e.CustomUser).FirstOrDefaultAsync(e=>e.Id == encuesta.Id);
                foreach (DetalleEncuesta detalle in encuestaData.DetalleEncuestas)
                {
                    var detalleData = await _context.DetalleEncuestas
                    .Include(e => e.Producto).ThenInclude(e => e.Marca)
                    .Include(e => e.Producto).ThenInclude(e => e.Propiedades)
                    .Include(e => e.Producto).ThenInclude(e => e.Categoria).FirstOrDefaultAsync(e=> e.Id == detalle.Id);
                    datos.Add(new DescargarDatos
                    {
                        NombreMedicion = medicion.nombreMedicion,
                        NombreEncuestador = $"{encuestaData.CustomUser.Name} {encuestaData.CustomUser.Lastname}",
                        LocalEncuestado = encuestaData.Local.Nombre,
                        FechaInicio = encuestaData.FechaInicio.ToString(),
                        FechaCierre = encuestaData.FechaCierre.ToString(),
                        DiasTrabajados = encuestaData.DiasTrabajados,
                        NombreProducto = detalleData.Producto.Nombre,
                        CodigoProducto  = detalleData.Producto.BarCode,
                        ProductoCategoria = detalleData.Producto.Categoria.NombreCategoria,
                        ProductoMarca = detalleData.Producto.Marca.NombreMarca,
                        ProductoPropiedad = detalleData.Producto.Propiedades.NombrePropiedades,
                        StockInicial = detalleData.StockInicial,
                        StockFinal = detalleData.StockFinal,
                        Compra = detalleData.Compra,
                        Pvd = detalleData.Pvd,
                        Pvp = detalleData.Pvp
                    });
                }
            }
            return datos;
        }

        [Route("MedicionesActivas")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Medicion>>> GetMedicionesActivas()
        {

            return await _context.Mediciones
                .Include(e => e.Encuestas).ThenInclude(e => e.CustomUser)
                .Include(e => e.Encuestas).ThenInclude(e => e.Local)
                .Include(e => e.Ciudad).Where(e => e.Activa).ToListAsync();
        }

        [Route("DetalleEncuesta/{id}")]
        [HttpGet]
        public async Task<ActionResult<Encuesta>> GetDetalleEncuesta(int id)
        {
            return await _context.Encuestas.Include(e => e.CustomUser).Include(e => e.Local).Include(e => e.Medicion).Include(e => e.DetalleEncuestas).ThenInclude(e => e.Producto).ThenInclude(e=> e.Propiedades)
                .Include(e => e.DetalleEncuestas).ThenInclude(e => e.Producto).ThenInclude(e => e.Marca).Include(e => e.DetalleEncuestas).ThenInclude(e => e.Producto).ThenInclude(e => e.Categoria)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        [Route("CrearMedicion")]
        [HttpPost]
        public async Task<ActionResult<Medicion>> CrearMedicion(int ciudadId)
        {
            try
            {
                var medicion = new Medicion();
                var mes = DateTime.Now.ToString("MMMM", CultureInfo.CreateSpecificCulture("es"));
                var ciudad = await _context.Ciudades.FindAsync(ciudadId);
                var ultimaMedicion = await _context.Mediciones.Include(e => e.Ciudad).Include(e => e.Encuestas).Where(e => e.CiudadId == ciudadId).OrderByDescending(m => m.Id).FirstOrDefaultAsync();
                if (ultimaMedicion == null)
                {
                    medicion = new Medicion
                    {
                        Activa = true,
                        nombreMedicion = $"Medicion {ciudad.NombreCiudad} {mes.ToUpper()} {DateTime.Now.Year}",
                        CiudadId = ciudadId
                    };
                    _context.Mediciones.Add(medicion);
                    await _context.SaveChangesAsync();
                    return Ok(new ObjectResult<Medicion>
                    {
                        Result = true,
                        Respose = "Medicion creada correctamente",
                        Object = medicion
                    });
                }
                if (ultimaMedicion.Activa)
                {
                    return BadRequest(error: new ObjectResult<Medicion>
                    {
                        Result = false,
                        Respose = "Ya hay una medicion activa para esta ciudad"
                    });
                }
                medicion = new Medicion
                {
                    Activa = true,
                    nombreMedicion = $"Medicion {ciudad.NombreCiudad} {mes.ToUpper()} {DateTime.Now.Year}",
                    CiudadId = ciudadId,
                };
                _context.Mediciones.Add(medicion);
                await _context.SaveChangesAsync();
                foreach (Encuesta encuesta in ultimaMedicion.Encuestas)
                {
                    var encuestaNueva = new Encuesta
                    {
                        FechaInicio = encuesta.FechaCierre,
                        FechaCierre = encuesta.FechaCierre,
                        DiasTrabajados = 0,
                        Visita = "INICIAL",
                        CustomUserId = encuesta.CustomUserId,
                        LocalId = encuesta.LocalId,
                        MedicionId = medicion.Id,
                    };
                    _context.Encuestas.Add(encuestaNueva);
                    await _context.SaveChangesAsync();
                    var encuestaDetalle = await _context.DetalleEncuestas.Where(e => e.EncuestaId == encuesta.Id).ToListAsync();
                    foreach (DetalleEncuesta detalle in encuestaDetalle)
                    {
                        var detalleNuevo = new DetalleEncuesta
                        {
                            StockInicial = detalle.StockFinal,
                            StockFinal = 0,
                            Compra = 0,
                            Pvd = 0,
                            Pvp = 0,
                            ProductoId = detalle.ProductoId,
                            EncuestaId = encuestaNueva.Id,
                        };
                        _context.DetalleEncuestas.Add(detalleNuevo);
                        await _context.SaveChangesAsync();
                    }
                }
                return Ok(new ObjectResult<Medicion>
                {
                    Result = true,
                    Respose = "Medicion creada correctamente",
                    Object = medicion
                });
            }
            catch (Exception ex)
            {
                return BadRequest(error: new ObjectResult<Medicion>
                {
                    Result = false,
                    Respose = $"No se pudo crear la medicion {ex.Message}"
                });
            }
        }

        [HttpPost("CerrarMedicion/{id}")]
        public async Task<ActionResult<Medicion>> CerrarMedicion(int id)
        {
            var medicion = await _context.Mediciones.Include(e => e.Encuestas).FirstOrDefaultAsync(i => i.Id == id);
            if (medicion == null)
            {
                return BadRequest(error: new ObjectResult<Encuesta>()
                {
                    Result = false,
                    Respose = "No se encontro la medicion"
                });
            }
            medicion.Activa = false;
            await _context.SaveChangesAsync();
            return Ok(new ObjectResult<Medicion>
            {
                Result = true,
                Respose = "Se cerro la medicion correctamente"
            });
        }
    }
}
