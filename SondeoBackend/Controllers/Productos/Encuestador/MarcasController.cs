using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using SondeoBackend.Configuration;
using SondeoBackend.Context;
using SondeoBackend.DTO.Result;
using SondeoBackend.Models;
using System.Web.Helpers;

namespace SondeoBackend.Controllers.Productos.Encuestador
{
    [Route("api/[controller]")]
    [ApiController]
    public class MarcasController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly AssignId _assignId;

        public MarcasController(DataContext context, AssignId assignId)
        {
            _assignId = assignId;
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Marca>>> GetMarcas(string email)
        {
            var alias = await _assignId.UserAlias(email);
            return await _context.Marcas.Where(e => e.SyncId.Contains("ADMIN") || e.SyncId.Contains(alias)).ToListAsync();
        }

        [HttpGet("{syncId}")]
        public async Task<ActionResult<Marca>> GetMarca(string syncId)
        {
            var marca = await _context.Marcas.FirstOrDefaultAsync(i => i.SyncId.Equals(syncId));
            if (marca == null)
            {
                return BadRequest(error: new ObjectResult<Marca>()
                {
                    Result = false,
                    Respose = "No se ha encontrado elemento"
                });
            }
            return marca;
        }

        [HttpPut("{syncId}")]
        public async Task<IActionResult> PutMarca(string syncId, Marca marca)
        {
            if (syncId.Equals(marca.SyncId))
            {
                return BadRequest(error: new ObjectResult<Marca>()
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
            _context.Entry(marca).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(new ObjectResult<Marca>()
            {
                Result = true,
                Respose = "Elemento modificado correctamente"
            });
        }

        [HttpPost]
        public async Task<ActionResult<Marca>> PostMarca(Marca marca, string email)
        {
            try
            {
                var identificador = "";
                _context.Marcas.Add(marca);
                var lastMarca = await _context.Marcas.OrderByDescending(m => m.Id).FirstOrDefaultAsync();
                var marcaConfirmacion = await _context.Marcas.Where(p => p.NombreMarca.Equals(marca.NombreMarca)).FirstOrDefaultAsync();
                if (marcaConfirmacion != null)
                {
                    return BadRequest(error: new ObjectResult<Producto>()
                    {
                        Result = false,
                        Respose = "Ya hay una marca con el mismo nombre"
                    });
                }
                if (lastMarca == null)
                {
                    identificador = await _assignId.AssignSyncId("0", email);
                }
                else
                {
                    identificador = await _assignId.AssignSyncId(lastMarca.SyncId, email);
                }
                marca.SyncId = identificador;
                await _context.SaveChangesAsync();
                return Ok(new ObjectResult<Marca>()
                {
                    Result = true,
                    Respose = "Elemento agregado correctamente"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(error: new ObjectResult<Marca>() { Result = false, Respose = $"No puedes agregar el elemento {ex.Message}" });
            }
        }

        [HttpDelete("{syncId}")]
        public async Task<IActionResult> DeleteMarca(string syncId)
        {
            var marca = await _context.Marcas.FirstOrDefaultAsync(i => i.SyncId.Equals(syncId));
            if (syncId.Contains("ADMIN"))
            {
                return BadRequest(error: new ObjectResult<Marca>() { Result = false, Respose = "No puedes eliminar este elemento" });
            }
            if (marca == null)
            {
                return BadRequest(error: new ObjectResult<Marca>() { Result = false, Respose = "No se ha encontrado el elemento" });
            }
            _context.Marcas.Remove(marca);
            await _context.SaveChangesAsync();

            return Ok(new ObjectResult<Marca>() { Result = true, Respose = "Elemento eliminado correctamente" });
        }
    }
}
