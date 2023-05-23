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
using SondeoBackend.DTO;
using SondeoBackend.Models;

namespace SondeoBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocalesControlController : ControllerBase
    {
        private readonly DataContext _context;
        private GenericRepository<Local> _localesRepository;
        private GenericRepository<Canal> _canalesRepository;
        private GenericRepository<Ciudad> _ciudadesRepository;

        public LocalesControlController(DataContext context, GenericRepository<Local> localesRepository, GenericRepository<Canal> canalesRepository, GenericRepository<Ciudad> ciudadesRepository)
        {
            _localesRepository = localesRepository;
            _canalesRepository = canalesRepository;
            _ciudadesRepository = ciudadesRepository;
            _context = context;
        }

        [HttpGet("Ciudades")]
        public ActionResult GetCiudad()
        {
            var cuidad = _ciudadesRepository.Get();
            return Ok(cuidad);
        }

        [HttpGet("Ciudades/{id}")]
        public ActionResult GetCiudadById(int id)
        {
            var cuidad = _ciudadesRepository.GetByID(id);
            if (cuidad == null)
            {
                return BadRequest(error: new AuthResult()
                {
                    Result = false,
                    Errors = new List<string>()
                            {
                                "No se encontro la cuidad"
                            }
                });
            }
            return Ok(cuidad);
        }

        [HttpPut("Ciudades/{id}")]
        public async Task<IActionResult> PutCiudad(int id, Ciudad cuidad)
        {
            var cuidadExist = _ciudadesRepository.GetByID(id);
            if (cuidadExist == null)
            {
                return BadRequest(error: new AuthResult()
                {
                    Result = false,
                    Errors = new List<string>()
                            {
                                "No se encontro la cuidad"
                            }
                });
            }
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
            var cuidadExist = _ciudadesRepository.GetByID(id);
            if (cuidadExist == null)
            {
                return BadRequest(error: new AuthResult()
                {
                    Result = false,
                    Errors = new List<string>()
                            {
                                "No se encontro la cuidad"
                            }
                });
            }
            _ciudadesRepository.Delete(id);
            await _context.SaveChangesAsync();
            return Ok(new AuthResult()
            {
                Result = true,
                Contenido = "Canal Eliminado"
            });
        }

        //Locales
        [HttpGet("Locales")]
        public async Task<ActionResult<IEnumerable<Local>>> GetLocales()
        {
            if (_context.Locales == null)
            {
                return NotFound();
            }
            return await _context.Locales.Include(e => e.Canal).ToListAsync();
        }

        [HttpGet("Locales/{id}")]
        public ActionResult<Local> GetLocalById(int id)
        {
            var local = _localesRepository.GetByID(id);
            if (local == null)
            {
                return BadRequest(error: new AuthResult()
                {
                    Result = false,
                    Errors = new List<string>()
                        {
                            "No se encontro el elemento"
                        }
                });
            }
            return local;
        }

        [HttpPut("Locales")]
        public async Task<IActionResult> PutLocal(int id, Local local)
        {
            var localExist = _localesRepository.GetByID(id);
            if (localExist == null)
            {
                return BadRequest(error: new AuthResult()
                {
                    Result = false,
                    Errors = new List<string>()
                        {
                            "No se encontro el local"
                        }
                });
            }
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
            var local = _localesRepository.GetByID(id);
            if (local == null)
            {
                return BadRequest(error: new AuthResult()
                {
                    Result = false,
                    Errors = new List<string>()
                        {
                            "No se encontro el local"
                        }
                });
            }
            _localesRepository.Delete(id);
            await _context.SaveChangesAsync();
            return Ok(new AuthResult()
            {
                Result = true,
                Contenido = "Local Eliminado"
            });
        }

        //Canales
        [HttpGet("Canales")]
        public ActionResult GetCanales()
        {
            var canal = _canalesRepository.Get();
            return Ok(canal);
        }

        [HttpGet("Canales/{id}")]
        public ActionResult GetCanalById(int id)
        {
            var canal = _canalesRepository.GetByID(id);
            if(canal == null)
            {
                return BadRequest(error: new AuthResult()
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

        [HttpPut("Canales/{id}")]
        public async Task<IActionResult> PutCanal(int id, Canal canal)
        {
            var canalExist = _canalesRepository.GetByID(id);
            if (canalExist == null)
            {
                return BadRequest(error: new AuthResult()
                {
                    Result = false,
                    Errors = new List<string>()
                            {
                                "No se encontro el canal"
                            }
                });
            }
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
            var canal = _canalesRepository.GetByID(id);
            if (canal == null)
            {
                return BadRequest(error: new AuthResult()
                {
                    Result = false,
                    Errors = new List<string>()
                            {
                                "No se encontro el canal"
                            }
                });
            }
            _canalesRepository.Delete(id);
            await _context.SaveChangesAsync();
            return Ok(new AuthResult()
            {
                Result = true,
                Contenido = "Canal Eliminado"
            });
        }                
    }
}
