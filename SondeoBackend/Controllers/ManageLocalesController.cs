using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using SondeoBackend.Configuration;
using SondeoBackend.Context;
using SondeoBackend.DTO;
using SondeoBackend.DTO.Result;
using SondeoBackend.Models;

namespace SondeoBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ManageLocalesController : ControllerBase
    {
        private readonly DataContext _context;

        public ManageLocalesController(DataContext context)
        {
            _context = context;
        }

        #region Locales
        [Route("Locales")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LocalDto>>> GetLocales()
        {
            var locales = await _context.Locales.Include(e => e.Canal).Include(e => e.Encuestas).ThenInclude(e => e.Medicion).ThenInclude(e => e.Ciudad).ToListAsync();
            return locales.Select(e => new LocalDto
            {
                Id = e.Id,
                Nombre = e.Nombre,
                Canal = e.Canal.NombreCanal,
                Direccion = e.Direccion,
                Ciudad = e.Encuestas.Count == 0 ? "Sin ciudad ni encuesta Asignada" : e.Encuestas.FirstOrDefault(e=>e.Medicion.Activa).Medicion.Ciudad.NombreCiudad,
                Habilitado = e.Habilitado
                
            }).ToList();
        }

        [HttpGet("Locales/{id}")]
        public async Task<ActionResult<IEnumerable<Local>>> GetLocalById(int id)
        {
            var locales = await _context.Locales.Include(e => e.Canal).Include(e => e.Encuestas).ThenInclude(e => e.Medicion).ThenInclude(e => e.Ciudad).ToListAsync();
            var local = locales.Select(e => new LocalDto
            {
                Id = e.Id,
                Nombre = e.Nombre,
                Canal = e.Canal.NombreCanal,
                Direccion = e.Direccion,
                Ciudad = e.Encuestas.Count == 0 ? "Sin ciudad ni encuesta Asignada" : e.Encuestas.FirstOrDefault(e => e.Medicion.Activa).Medicion.Ciudad.NombreCiudad,
                Habilitado = e.Habilitado

            }).FirstOrDefault(i => i.Id == id);
            if (local == null)
            {
                return BadRequest(error: new ObjectResult<Local>()
                {
                    Result = false,
                    Respose = "No se encontro el elemento"
                });
            }
            return Ok(local);
        }

        [HttpPut("Locales/{id}")]
        public async Task<IActionResult> PutLocal(int id, Local local)
        {
            var localExist = await _context.Locales.FindAsync(id);
            if (localExist == null)
            {
                return BadRequest(error: new ObjectResult<Local>()
                {
                    Result = false,
                    Respose = "No se encontro el local"
                });
            }
            _context.Entry(local).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok(new ObjectResult<Local>()
            {
                Result = true,
                Respose = "Local Actualizado"
            });
        }

        [HttpPost]
        [Route("HabilitarLocal")]
        public async Task<IActionResult> ActivarUsuario(int id, bool eleccion)
        {
            try
            {
                var local = await _context.Locales.FindAsync(id);
                if (local == null)
                {
                    return BadRequest(error: new UserResult()
                    {
                        Result = false,
                        Respose = "El local no fue encontrado"
                    });
                }
                local.Habilitado = eleccion ? true : false;
                await _context.SaveChangesAsync();
                return Ok(new ObjectResult<CustomUser>()
                {
                    Result = true,
                    Respose = "Se ha actualizado el estado del local correctamente"
                });

            }
            catch (Exception ex)
            {
                return BadRequest(error: new UserResult()
                {
                    Result = false,
                    Respose = $"No se pudo activar el usuario {ex.Message}"
                });
            }
        }

        [Route("Locales")]
        [HttpPost]
        public async Task<ActionResult<Local>> PostLocal(Local local)
        {
            try
            {
                _context.Locales.Add(local);
                await _context.SaveChangesAsync();
                return Ok(new ObjectResult<Local>()
                {
                    Result = true,
                    Respose = "Local Agregado"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(error: new ObjectResult<Local>()
                {
                    Result = false,
                    Respose = $"No se pudo agregar el local {ex.Message}"
                });
            }
        }

        [HttpDelete("Locales/{id}")]
        public async Task<IActionResult> DeleteLocal(int id)
        {
            var local = await _context.Locales.FindAsync(id);
            if (local == null)
            {
                return BadRequest(error: new ObjectResult<Local>()
                {
                    Result = false,
                    Respose = "No se encontro el local"
                });
            }
            _context.Locales.Remove(local);
            await _context.SaveChangesAsync();
            return Ok(new ObjectResult<Local>()
            {
                Result = true,
                Respose = "Local Eliminado"
            });
        }
        #endregion

        #region Canales
        [Route("Canales")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Canal>>> GetCanales()
        {
            return await _context.Canales.ToListAsync();
        }

        [HttpGet("Canales/{id}")]
        public async Task<ActionResult<IEnumerable<Canal>>> GetCanalById(int id)
        {
            var canal = await _context.Canales.FindAsync(id);
            if (canal == null)
            {
                return BadRequest(error: new ObjectResult<Canal>()
                {
                    Result = false,
                    Respose = "No se encontro el canal"
                });
            }
            return Ok(canal);
        }

        [HttpPut("Canales/{id}")]
        public async Task<IActionResult> PutCanal(int id, Canal canal)
        {
            var canalExist = await _context.Canales.FindAsync(id);
            if (canalExist == null)
            {
                return BadRequest(error: new ObjectResult<Canal>()
                {
                    Result = false,
                    Respose = "No se encontro el canal"
                });
            }
            _context.Entry(canal).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok(new ObjectResult<Canal>()
            {
                Result = true,
                Respose = "Canal Actualizado"
            });
        }

        [Route("Canales")]
        [HttpPost]
        public async Task<ActionResult<Canal>> PostCanal(Canal canal)
        {
            try
            {
                _context.Canales.Add(canal);
                await _context.SaveChangesAsync();
                return Ok(new ObjectResult<Canal>()
                {
                    Result = true,
                    Respose = "Canal Agregado"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(error: new ObjectResult<Canal>()
                {
                    Result = false,
                    Respose = $"No se pudo agregar el Canal {ex.Message}"
                });
            }
        }

        [HttpDelete("Canales/{id}")]
        public async Task<IActionResult> DeleteCanal(int id)
        {
            var canal = await _context.Canales.FindAsync(id);
            if (canal == null)
            {
                return BadRequest(error: new ObjectResult<Canal>()
                {
                    Result = false,
                    Respose = "No se encontro el canal"
                });
            }
            _context.Canales.Remove(canal);
            await _context.SaveChangesAsync();
            return Ok(new ObjectResult<Canal>()
            {
                Result = true,
                Respose = "Canal Eliminado"
            });
        }
        #endregion

        #region Ciudades
        [Route("Ciudades")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Ciudad>>> GetCiudad()
        {
            if (_context.Ciudades == null)
            {
                return NotFound();
            }
            return await _context.Ciudades.ToListAsync();
        }

        [HttpGet("Ciudades/{id}")]
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

        [HttpPut("Ciudades/{id}")]
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

        [Route("Ciudades")]
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

        [HttpDelete("Ciudades/{id}")]
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
                Respose = "Ciudad Eliminado"
            });
        }
        #endregion
    }
}
