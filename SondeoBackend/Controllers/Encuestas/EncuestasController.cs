using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using Microsoft.Win32;
using SondeoBackend.Configuration;
using SondeoBackend.Context;
using SondeoBackend.DTO;
using SondeoBackend.DTO.Encuestador.Registrar;
using SondeoBackend.DTO.Result;
using SondeoBackend.Models;
using System.Collections.ObjectModel;
using System.Web.Helpers;

namespace SondeoBackend.Controllers.Encuestas
{
    [Route("api/[controller]")]
    [ApiController]
    public class EncuestasController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly UserManager<CustomUser> _userManager;
        private readonly AssignId _assignId;

        public EncuestasController(DataContext context, UserManager<CustomUser> userManager,AssignId assignId)
        {
            _assignId = assignId;
            _userManager = userManager;
            _context = context;
        }
        
        [HttpGet("Medicion/{id}")]
        public async Task<ActionResult<Medicion>> GetMedicion(int id)
        {
            var medicion = await _context.Mediciones.Include(e => e.Encuestas).FirstOrDefaultAsync(i => i.Id == id);
            return Ok(new ObjectResult<Medicion>
            {
                Result = true,
                Object = medicion
            });
        }        

        [Route("CrearEncuesta")]
        [HttpPost]
        public async Task<IActionResult> CrearEncuesta(RegistroEncuesta registro)
        {            
            var user = await _userManager.FindByIdAsync($"{registro.UserId}");
            var local = await _context.Locales.FindAsync(registro.LocalId);
            if (local == null)
            {
                return BadRequest(error: new ObjectResult<Encuesta>()
                {
                    Result = false,
                    Respose = "No se encontro el local asignado"
                });
            }
            if (!local.Habilitado)
            {
                return BadRequest(error: new ObjectResult<Encuesta>()
                {
                    Result = false,
                    Respose = "Este local no esta habilitado"
                });
            }
            var ultimaEncuesta = await _context.Encuestas.Include(e => e.DetalleEncuestas).OrderByDescending(m => m.Id).FirstOrDefaultAsync();
            var identificador = await _assignId.AssignSyncId(ultimaEncuesta == null ? "0" : ultimaEncuesta.SyncId, user.Email);
            var comprobacion = await _context.Encuestas.Where(e => e.MedicionId == registro.MedicionId && e.LocalId == registro.LocalId).OrderByDescending(m => m.Id).FirstOrDefaultAsync();
            if (comprobacion != null)
            {
                return BadRequest(error: new ObjectResult<Encuesta>()
                {
                    Result = false,
                    Respose = "Ya hay una encuesta activa para este local"
                });
            }
            var encuesta = new Encuesta
            {
                FechaInicio = DateTime.Now,
                FechaCierre = null,
                DiasTrabajados = 0,
                CustomUserId = registro.UserId,
                LocalId = registro.LocalId,
                MedicionId = registro.MedicionId,
                SyncId = identificador
            };
            _context.Encuestas.Add(encuesta);
            await _context.SaveChangesAsync();
            return Ok(new ObjectResult<Encuesta>
            {
                Result = true,
                Respose = "Encuesta creada correctamente"
            });
        }

        [Route("AgregarProductoEncuesta")]
        [HttpPost]
        public async Task<IActionResult> AgregarProductoEncuesta(RegistroProductoEncuesta registro)
        {
            var encuesta = await _context.Encuestas.Include(e=>e.CustomUser).FirstOrDefaultAsync(i => i.Id == registro.EncuestaId);
            var lastDetalleEncuesta = await _context.DetalleEncuestas.OrderByDescending(m => m.Id).FirstOrDefaultAsync();
            var identificador = await _assignId.AssignSyncId(lastDetalleEncuesta == null ? "0" : lastDetalleEncuesta.SyncId, encuesta.CustomUser.Email);
            var detalleEncuesta = new DetalleEncuesta
            {
                StockInicial = registro.StockInicial,
                StockFinal = -1,
                Compra  = registro.Compra,
                Pvd = registro.Pvd,
                Pvp = registro.Pvp,
                EncuestaId = registro.EncuestaId,
                ProductoId = registro.ProductoId,
                SyncId = identificador
            };
            var productoEncuesta = await _context.DetalleEncuestas.Where(e=>e.ProductoId == registro.ProductoId && e.EncuestaId == registro.EncuestaId).FirstOrDefaultAsync();
            if (productoEncuesta == null)
            {
                _context.DetalleEncuestas.Add(detalleEncuesta);
                await _context.SaveChangesAsync();
                return Ok(new ObjectResult<Encuesta>
                {
                    Result = true,
                    Respose = "Producto agregado a la encuesta correctamente"
                });
            }
            return BadRequest(error: new ObjectResult<Encuesta>
            {
                Result = false,
                Respose = "Este producto ya esta asignado a esta encuesta"
            });
        }

        [Route("TerminarProducto/{syncId}")]
        [HttpPost]
        public async Task<IActionResult> TerminarProducto(int stockFinal, string syncId)
        {
            var detalle = await _context.DetalleEncuestas.FirstOrDefaultAsync(i => i.SyncId == syncId);
            if(detalle.StockFinal != -1) {
                return Ok(new ObjectResult<DetalleEncuesta>
                {
                    Result = true,
                    Respose = "El producto ya ha sido terminado"
                });
            }
            detalle.StockFinal = stockFinal;
            await _context.SaveChangesAsync();
            var cont = 0;
            var encuesta = await _context.Encuestas.FirstOrDefaultAsync(i => i.Id == detalle.EncuestaId);
            foreach(DetalleEncuesta detalles in encuesta.DetalleEncuestas)
            {
                if(detalle.StockFinal == -1)
                {
                    cont++;
                }
            }
            if(cont == 0)
            {
                encuesta.FechaCierre = DateTime.Now;
                encuesta.DiasTrabajados = (DateTime.Now - encuesta.FechaInicio).Value.Days;                
                await _context.SaveChangesAsync();
            }
            return Ok(new ObjectResult<DetalleEncuesta>
            {
                Result = true,
                Respose = "Producto registrado"
            });
        }

        [Route("TransferirEncuesta")]
        [HttpPost]
        public async Task<IActionResult> TransferirEncuesta(int idEncuesta, int idUsuario)
        {
            var encuesta = await _context.Encuestas.Include(e => e.Medicion).FirstOrDefaultAsync(i => i.Id == idEncuesta);                  
            encuesta.CustomUserId = idUsuario;
            await _context.SaveChangesAsync();
            return Ok(new ObjectResult<Encuesta>
            {
                Result = true,
                Respose = "Encuesta transferida correctamente"
            });
        }
    }
}
