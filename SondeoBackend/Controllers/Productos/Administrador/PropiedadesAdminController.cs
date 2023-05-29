using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SondeoBackend.Context;
using SondeoBackend.DTO;
using SondeoBackend.Models;

namespace SondeoBackend.Controllers.Productos.Administrador
{
    
    [Route("api/[controller]")]
    [ApiController]
    public class PropiedadesAdminController : ControllerBase
    {
        private readonly DataContext _context;

        public PropiedadesAdminController(DataContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Propiedades>>> GetPropiedades()
        {
            if (_context.Propiedades == null)
            {
                return NotFound();
            }
            return await _context.Propiedades.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Propiedades>> GetPropiedades(int id)
        {
            if (_context.Propiedades == null)
            {
                return NotFound();
            }
            var propiedades = await _context.Propiedades.FindAsync(id);

            if (propiedades == null)
            {
                return NotFound();
            }

            return propiedades;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutPropiedades(int id, Propiedades propiedades)
        {
            if (id != propiedades.Id)
            {
                return BadRequest(error: new ModelResult()
                {
                    Result = false,
                    Contenido = "El elemento no coincide"
                });
            }
            if (id == 1)
            {
                return BadRequest(error: new ModelResult()
                {
                    Result = false,
                    Contenido = "No puedes modificar este elemento"
                });
            }
            _context.Entry(propiedades).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(new ModelResult()
            {
                Result = true,
                Contenido = "Elemento modificado correctamente"
            });
        }

        [HttpPost]
        public async Task<ActionResult<Propiedades>> PostPropiedades(Propiedades propiedades)
        {
            try
            {
                _context.Propiedades.Add(propiedades);
                await _context.SaveChangesAsync();
                return Ok(new ModelResult()
                {
                    Result = true,
                    Contenido = "Elemento agregado correctamente"
                });
            }
            catch (DbUpdateConcurrencyException)
            {
                return BadRequest(error: new ModelResult()
                {
                    Result = false,
                    Contenido = "No puedes agregar el elemento"
                });
            }
            
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePropiedades(int id)
        {
            var propiedades = await _context.Propiedades.FindAsync(id);
            if (id == 1)
            {
                return BadRequest(error: new ModelResult()
                {
                    Result = false,
                    Contenido = "No puedes eliminar este elemento"
                });
            }
            if (propiedades == null)
            {
                return BadRequest(error: new ModelResult()
                {
                    Result = false,
                    Contenido = "No se ha encontrado elemento"
                });
            }

            _context.Propiedades.Remove(propiedades);
            await _context.SaveChangesAsync();

            return Ok(new ModelResult()
            {
                Result = true,
                Contenido = "Elemento eliminado correctamente"
            });
        }
    }
}
