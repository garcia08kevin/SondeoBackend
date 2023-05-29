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

namespace SondeoBackend.Controllers.Locales
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocalesController : ControllerBase
    {
        private readonly DataContext _context;

        public LocalesController(DataContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Local>>> GetLocales(int idCiudad = 0)
        {
            if (idCiudad == 0)
            {
                return await _context.Locales.ToListAsync();
            }
            return await _context.Locales.Where(e => e.CiudadId == idCiudad).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<IEnumerable<Local>>> GetLocalById(int id)
        {
            var local = await _context.Locales.FindAsync(id);
            if (local == null)
            {
                return BadRequest(error: new ModelResult()
                {
                    Result = false,
                    Errors = new List<string>()
                        {
                            "No se encontro el elemento"
                        }
                });
            }
            return Ok(local);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutLocal(int id, Local local)
        {
            var localExist = await _context.Locales.FindAsync(id);
            if (localExist == null)
            {
                return BadRequest(error: new ModelResult()
                {
                    Result = false,
                    Errors = new List<string>()
                        {
                            "No se encontro el local"
                        }
                });
            }
            _context.Entry(local).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok(new ModelResult()
            {
                Result = true,
                Contenido = "Local Actualizado"
            });
        }

        [HttpPost]
        public async Task<ActionResult<Local>> PostLocal(Local local)
        {
            try
            {
                _context.Locales.Add(local);
                await _context.SaveChangesAsync();
                return Ok(new ModelResult()
                {
                    Result = true,
                    Contenido = "Local Agregado"
                });
            }
            catch(Exception ex)
            {
                return BadRequest(error: new ModelResult()
                {
                    Contenido= "No se pudo agregar el local",
                    Result = false,
                    Errors = new List<string>()
                        {
                            ex.Message
                        }
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLocal(int id)
        {
            var local = await _context.Locales.FindAsync(id);
            if (local == null)
            {
                return BadRequest(error: new ModelResult()
                {
                    Result = false,
                    Errors = new List<string>()
                        {
                            "No se encontro el local"
                        }
                });
            }
            _context.Locales.Remove(local);
            await _context.SaveChangesAsync();
            return Ok(new ModelResult()
            {
                Result = true,
                Contenido = "Local Eliminado"
            });
        }        
    }
}
