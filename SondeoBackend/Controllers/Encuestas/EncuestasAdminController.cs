using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using SondeoBackend.Configuration;
using SondeoBackend.Context;
using SondeoBackend.DTO.Result;
using SondeoBackend.Models;
using System.Globalization;

namespace SondeoBackend.Controllers.Encuestas
{
    [Route("api/[controller]")]
    [ApiController]
    public class EncuestasAdminController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly UserManager<CustomUser> _userManager;
        private readonly AssignId _assignId;

        public EncuestasAdminController(DataContext context, UserManager<CustomUser> userManager, AssignId assignId)
        {
            _assignId = assignId;
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
            return await _context.Encuestas.Include(e => e.CustomUser).Include(e => e.Medicion).Include(e => e.DetalleEncuestas).ThenInclude(e => e.Producto).FirstOrDefaultAsync(m => m.Id == id);
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
                var ultimaMedicion = await _context.Mediciones.Include(e => e.Ciudad).Include(e => e.Encuestas).Include(e => e.Encuestas).Where(e => e.CiudadId == ciudadId).OrderByDescending(m => m.Id).FirstOrDefaultAsync();
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
                var utlimaEncuesta = await _context.Encuestas.OrderByDescending(m => m.Id).FirstOrDefaultAsync();
                var ultimoDetalle = await _context.DetalleEncuestas.OrderByDescending(m => m.Id).FirstOrDefaultAsync();
                var idenEncu = utlimaEncuesta == null ? "0" : utlimaEncuesta.SyncId;
                var idenDeta = ultimoDetalle == null ? "0" : ultimoDetalle.SyncId;
                foreach (Encuesta encuesta in ultimaMedicion.Encuestas)
                {
                    var user = await _userManager.FindByIdAsync($"{encuesta.CustomUserId}");
                    var encuestaConDetalle = await _context.Encuestas.Include(e => e.DetalleEncuestas).FirstOrDefaultAsync(m => m.Id == encuesta.Id);
                    var encuestaNueva = new Encuesta
                    {
                        FechaInicio = encuesta.FechaCierre,
                        FechaCierre = null,
                        DiasTrabajados = 0,
                        CustomUserId = encuesta.CustomUserId,
                        LocalId = encuesta.LocalId,
                        MedicionId = medicion.Id,
                        SyncId = await _assignId.AssignSyncId(idenEncu, user.Email)
                    };
                    idenEncu = encuestaNueva.SyncId;
                    _context.Encuestas.Add(encuestaNueva);
                    await _context.SaveChangesAsync();
                    foreach (DetalleEncuesta detalle in encuesta.DetalleEncuestas)
                    {
                        var detalleNuevo = new DetalleEncuesta
                        {
                            StockInicial = detalle.StockFinal,
                            StockFinal = -1,
                            Compra = 0,
                            Pvd = 0,
                            Pvp = 0,
                            ProductoId = detalle.ProductoId,
                            EncuestaId = encuestaNueva.Id,
                            SyncId = await _assignId.AssignSyncId(idenDeta, user.Email)
                        };
                        idenDeta = detalleNuevo.SyncId;
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
            catch(Exception ex)
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
                Object = medicion
            });
        }
    }
}
