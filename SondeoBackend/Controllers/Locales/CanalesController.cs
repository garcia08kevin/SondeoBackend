using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SondeoBackend.Configuration;
using SondeoBackend.Context;
using SondeoBackend.DTO;
using SondeoBackend.Models;

namespace SondeoBackend.Controllers.Locales
{
    [Route("api/[controller]")]
    [ApiController]
    public class CanalesController : ControllerBase
    {
        private readonly DataContext _context;
        public CanalesController(DataContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Canal>>> GetCanales()
        {
            return await _context.Canales.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<IEnumerable<Canal>>> GetCanalById(int id)
        {
            var canal = await _context.Canales.FindAsync(id);
            if (canal == null)
            {
                return BadRequest(error: new ModelResult()
                {
                    Result = false,
                    Errors = new List<string>()
                        {
                            "No se encontro el canal"
                        }
                });
            }
            return Ok(canal);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutCanal(int id, Canal canal)
        {
            var canalExist = await _context.Canales.FindAsync(id);
            if (canalExist == null)
            {
                return BadRequest(error: new ModelResult()
                {
                    Result = false,
                    Errors = new List<string>()
                            {
                                "No se encontro el canal"
                            }
                });
            }
            _context.Entry(canal).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok(new ModelResult()
            {
                Result = true,
                Contenido = "Canal Actualizado"
            });
        }

        [HttpPost]
        public async Task<ActionResult<Canal>> PostCanal(Canal canal)
        {
            try
            {
                _context.Canales.Add(canal);
                await _context.SaveChangesAsync();
                return Ok(new ModelResult()
                {
                    Result = true,
                    Contenido = "Canal Agregado"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(error: new ModelResult()
                {
                    Contenido = "No se pudo agregar el Canal",
                    Result = false,
                    Errors = new List<string>()
                        {
                            ex.Message
                        }
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCanal(int id)
        {
            var canal = await _context.Canales.FindAsync(id);
            if (canal == null)
            {
                return BadRequest(error: new ModelResult()
                {
                    Result = false,
                    Errors = new List<string>()
                            {
                                "No se encontro el canal"
                            }
                });
            }
            _context.Canales.Remove(canal);
            await _context.SaveChangesAsync();
            return Ok(new ModelResult()
            {
                Result = true,
                Contenido = "Canal Eliminado"
            });
        }
    }
}
