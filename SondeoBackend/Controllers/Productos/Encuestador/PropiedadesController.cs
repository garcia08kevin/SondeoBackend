using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SondeoBackend.Configuration;
using SondeoBackend.Context;
using SondeoBackend.DTO.Result;
using SondeoBackend.Models;

namespace SondeoBackend.Controllers.Productos.Encuestador
{
    [Route("api/[controller]")]
    [ApiController]
    public class PropiedadesController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly AssignId _assignId;

        public PropiedadesController(DataContext context, AssignId assignId)
        {
            _assignId = assignId;
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Propiedades>>> GetPropiedades(string email)
        {
            var alias = await _assignId.UserAlias(email);
            return await _context.Propiedades.Where(e => e.SyncId.Contains("ADMIN") || e.SyncId.Contains(alias)).ToListAsync();
        }

        [HttpGet("{syncId}")]
        public async Task<ActionResult<Propiedades>> GetPropiedad(string syncId)
        {
            var propiedades = await _context.Propiedades.FirstOrDefaultAsync(i => i.SyncId.Equals(syncId));

            if (propiedades == null)
            {
                return BadRequest(error: new ObjectResult<Marca>()
                {
                    Result = false,
                    Respose = "No se ha encontrado elemento"
                });
            }
            return propiedades;
        }

        [HttpPut("{syncId}")]
        public async Task<IActionResult> PutPropiedades(string syncId, Propiedades propiedades)
        {
            if (syncId.Equals(propiedades.SyncId))
            {
                return BadRequest(error: new ObjectResult<Propiedades>()
                {
                    Result = false,
                    Respose = "El elemento no coincide"
                });
            }
            if (syncId.Contains("ADMIN"))
            {
                return BadRequest(error: new ObjectResult<Marca>()
                {
                    Result = false,
                    Respose = "No puedes modificar este elemento"
                });
            }
            _context.Entry(propiedades).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(new ObjectResult<Propiedades>()
            {
                Result = true,
                Respose = "Elemento modificado correctamente"
            });
        }

        [HttpPost]
        public async Task<ActionResult<Propiedades>> PostPropiedades(Propiedades propiedades, string email)
        {
            try
            {
                var identificador = "";
                _context.Propiedades.Add(propiedades);
                var lastPropiedad = await _context.Propiedades.OrderByDescending(m => m.Id).FirstOrDefaultAsync();
                var propiedadConfirmacion = await _context.Propiedades.Where(p => p.NombrePropiedades.Equals(propiedades.NombrePropiedades)).FirstOrDefaultAsync();
                if (propiedadConfirmacion != null)
                {
                    return BadRequest(error: new ObjectResult<Producto>()
                    {
                        Result = false,
                        Respose = "Ya hay un propiedad con el mismo nombre"
                    });
                }
                if (lastPropiedad == null)
                {
                    identificador = await _assignId.AssignSyncId("0", email);
                }
                else
                {
                    identificador = await _assignId.AssignSyncId(lastPropiedad.SyncId, email);
                }
                propiedades.SyncId = identificador;
                await _context.SaveChangesAsync();
                return Ok(new ObjectResult<Propiedades>()
                {
                    Result = true,
                    Respose = "Elemento agregado correctamente"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(error: new ObjectResult<Propiedades>()
                {
                    Result = false,
                    Respose = $"No se pudo agregar el elemento {ex.Message}"
                });
            }

        }

        [HttpDelete("{syncId}")]
        public async Task<IActionResult> DeletePropiedades(string syncId)
        {
            var propiedades = await _context.Propiedades.FirstOrDefaultAsync(i => i.SyncId.Equals(syncId));
            if (syncId.Contains("ADMIN"))
            {
                return BadRequest(error: new ObjectResult<Propiedades>() { Result = false, Respose = "No puedes eliminar este elemento" });
            }
            if (propiedades == null)
            {
                return BadRequest(error: new ObjectResult<Propiedades>()
                {
                    Result = false,
                    Respose = "No se ha encontrado el elemento"
                });
            }
            _context.Propiedades.Remove(propiedades);
            await _context.SaveChangesAsync();

            return Ok(new ObjectResult<Propiedades>()
            {
                Result = true,
                Respose = "Elemento eliminado correctamente"
            });
        }
    }
}
