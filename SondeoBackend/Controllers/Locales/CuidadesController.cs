using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SondeoBackend.Configuration;
using SondeoBackend.Context;
using SondeoBackend.DTO;
using SondeoBackend.DTO.Result;
using SondeoBackend.Models;

namespace SondeoBackend.Controllers.Locales
{
    [Route("api/[controller]")]
    [ApiController]
    public class CuidadesController : ControllerBase
    {
        private readonly DataContext _context;

        public CuidadesController(DataContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Ciudad>>> GetCiudad()
        {
            if (_context.Ciudades == null)
            {
                return NotFound();
            }
            return await _context.Ciudades.Include(e => e.Locales).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<IEnumerable<Ciudad>>> GetCiudadById(int id)
        {
            var cuidad = await _context.Ciudades.FindAsync(id);
            if (cuidad == null)
            {
                return BadRequest(error: new ObjectResult<Ciudad>()
                {
                    Result = false,
                    Respose = "No se encontro la cuidad"
                });
            }
            return Ok(cuidad);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutCiudad(int id, Ciudad cuidad)
        {
            var cuidadExist = await _context.Ciudades.FindAsync(id);
            if (cuidadExist == null)
            {
                return BadRequest(error: new UserResult()
                {
                    Result = false,
                    Respose = "No se encontro la cuidad"
                });
            }
            _context.Entry(cuidadExist).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok(new ObjectResult<Ciudad>()
            {
                Result = true,
                Respose = "Ciudad Actualizada",
                Object = cuidad
            });
        }

        [HttpPost]
        public async Task<ActionResult<Canal>> PostCiudad(Ciudad cuidad)
        {
            try
            {
                _context.Ciudades.Add(cuidad);
                await _context.SaveChangesAsync();
                return Ok(new ObjectResult<Ciudad>()
                {
                    Result = true,
                    Respose = "Ciudad Agregada",
                    Object = cuidad
                });
            }
            catch (Exception ex)
            {
                return BadRequest(error: new UserResult()
                {
                    Result = false,
                    Respose = $"No se pudo agregar la ciudad {ex.Message}"
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCiudad(int id)
        {
            var cuidadExist = await _context.Ciudades.FindAsync(id);
            if (cuidadExist == null)
            {
                return BadRequest(error: new ObjectResult<Ciudad>()
                {
                    Result = false,
                    Respose = "No se encontro la cuidad"
                });
            }
            _context.Ciudades.Remove(cuidadExist);
            await _context.SaveChangesAsync();
            return Ok(new ObjectResult<Ciudad>()
            {
                Result = true,
                Respose = "Ciudad Eliminad"
            });
        }
    }
}
