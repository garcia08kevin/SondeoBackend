using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using SondeoBackend.Configuration;
using SondeoBackend.Context;
using SondeoBackend.Models;

namespace SondeoBackend.Controllers.Locales
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocalesControlController : ControllerBase
    {
        private readonly DataContext _context;
        private GenericRepository<Local> _localesRepository;
        private GenericRepository<Canal> _canalesRepository;
        private GenericRepository<Ciudad> _ciudadesRepository;

        public LocalesControlController(DataContext context,GenericRepository<Local> localesRepository, GenericRepository<Canal> canalesRepository, GenericRepository<Ciudad> ciudadesRepository)
        {
            _localesRepository = localesRepository;
            _canalesRepository = canalesRepository;
            _ciudadesRepository = ciudadesRepository;
            _context = context;
        }

        [HttpGet("Locales")]
        public async Task<ActionResult<IEnumerable<Local>>> GetLocales()
        {
          if (_context.Locales == null)
          {
              return NotFound();
          }
            return await _context.Locales.Include(e=> e.Canal).Include(e => e.Ciudad).ToListAsync();
        }

        [HttpGet("Locales/{id}")]
        public async Task<ActionResult<Local>> GetLocalById(int id)
        {
            if (LocalExists(id))
            {
                var local = _localesRepository.GetByID(id);
                return Ok(local);
            }
            return BadRequest(error: new AuthResult()
            {
                Result = false,
                Errors = new List<string>()
                        {
                            "No se encontro el local"
                        }
            });            
        }

        [HttpPut("Locales")]
        public async Task<IActionResult> PutLocal(Local local)
        {
            _localesRepository.Update(local);
            await _context.SaveChangesAsync();
            return Ok(local);
        }

        [HttpPost("Locales")]
        public async Task<ActionResult<Local>> PostLocal(Local local)
        {
            _localesRepository.Insert(local);
            await _context.SaveChangesAsync();
            return Ok(local);
        }

        [HttpDelete("Locales/{id}")]
        public async Task<IActionResult> DeleteLocal(int id)
        {
            if (LocalExists(id))
            {
                _localesRepository.Delete(id);
                await _context.SaveChangesAsync();
                return Ok(new AuthResult()
                {
                    Result = true,
                    Contenido = "Local Eliminado"
                });
            }
            return BadRequest(error: new AuthResult()
            {
                Result = false,
                Errors = new List<string>()
                        {
                            "No se encontro la cuidad"
                        }
            });            
        }

        //Canales

        [HttpGet("Canales")]
        public async Task<ActionResult> GetCanales()
        {
            var canal =_canalesRepository.Get();
            return Ok(canal);
        }

        [HttpGet("Canales/{id}")]
        public async Task<ActionResult> GetCanalById(int id)
        {
            
            if (CanalExists(id))
            {
                var canal = _canalesRepository.GetByID(id);
                return Ok(canal);

            }
            return BadRequest(error: new AuthResult()
            {
                Result = false,
                Errors = new List<string>()
                        {
                            "No se encontro el canal"
                        }
            });
        }

        [HttpPut("Canales/{id}")]
        public async Task<IActionResult> PutCanal(Canal canal)
        {
            _canalesRepository.Update(canal);
            await _context.SaveChangesAsync();
            return Ok(canal);
        }

        [HttpPost("Canales")]
        public async Task<ActionResult<Canal>> PostCanal(Canal canal)
        {
            _canalesRepository.Insert(canal);
            await _context.SaveChangesAsync();
            return Ok(canal);
        }

        [HttpDelete("Canales/{id}")]
        public async Task<IActionResult> DeleteCanal(int id)
        {
            if (CanalExists(id))
            {
                _canalesRepository.Delete(id);
                await _context.SaveChangesAsync();
                return Ok(new AuthResult()
                {
                    Result = true,
                    Contenido = "Canal Eliminado"
                });

            }
            return BadRequest(error: new AuthResult()
            {
                Result = false,
                Errors = new List<string>()
                        {
                            "No se encontro el canal"
                        }
            });            
        }

        //Ciudades

        [HttpGet("Ciudades")]
        public async Task<ActionResult> GetCiudad()
        {
            var cuidad = _ciudadesRepository.Get();
            return Ok(cuidad);
        }

        [HttpGet("Ciudades/{id}")]
        public async Task<ActionResult> GetCiudadById(int id)
        {
            if (CuidadExists(id))
            {
                var canal = _ciudadesRepository.GetByID(id);
                return Ok(canal);

            }
            return BadRequest(error: new AuthResult()
            {
                Result = false,
                Errors = new List<string>()
                        {
                            "No se encontro la cuidad"
                        }
            });            
        }

        [HttpPut("Ciudades/{id}")]
        public async Task<IActionResult> PutCiudad(Ciudad cuidad)
        {
            _ciudadesRepository.Update(cuidad);
            await _context.SaveChangesAsync();
            return Ok(cuidad);
        }

        [HttpPost("Ciudades")]
        public async Task<ActionResult<Canal>> PostCiudad(Ciudad cuidad)
        {
            _ciudadesRepository.Insert(cuidad);
            await _context.SaveChangesAsync();
            return Ok(cuidad);
        }

        [HttpDelete("Ciudades/{id}")]
        public async Task<IActionResult> DeleteCiudad(int id)
        {
            if (CuidadExists(id))
            {
                _ciudadesRepository.Delete(id);
                await _context.SaveChangesAsync();
                return Ok(new AuthResult()
                {
                    Result = true,
                    Contenido = "Canal Eliminado"
                });
            }
            return BadRequest(error: new AuthResult()
            {
                Result = false,
                Errors = new List<string>()
                        {
                            "No se encontro la cuidad"
                        }
            });
        }

        private bool LocalExists(int id)
        {
            return (_context.Locales?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        private bool CanalExists(int id)
        {
            return (_context.Canales?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        private bool CuidadExists(int id)
        {
            return (_context.Ciudades?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
