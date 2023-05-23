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
    public class MarcasAdminController : ControllerBase
    {
        private readonly DataContext _context;

        public MarcasAdminController(DataContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Marca>>> GetMarcas()
        {
            if (_context.Marcas == null)
            {
                return NotFound();
            }
            return await _context.Marcas.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Marca>> GetMarca(int id)
        {
            if (_context.Marcas == null)
            {
                return BadRequest(error: new AuthResult()
                {
                    Result = false,
                    Contenido = "No se ha encontrado elemento"
                });            
            }
            var marca = await _context.Marcas.FindAsync(id);

            if (marca == null)
            {
                return BadRequest(error: new AuthResult()
                {
                    Result = false,
                    Contenido = "No se ha encontrado elemento"
                });            
            }
            return marca;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutMarca(int id, Marca marca)
        {
            if (id != marca.Id)
            {
                return BadRequest(error: new AuthResult()
                {
                    Result = false,
                    Contenido = "El elemento no coincide"
                });
            }
            if (id == 1)
            {
                return BadRequest(error: new AuthResult()
                {
                    Result = false,
                    Contenido = "No puedes modificar este elemento"
                });
            }
            _context.Entry(marca).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(new AuthResult()
            {
                Result = true,
                Contenido = "Elemento modificado correctamente"
            });
        }

        [HttpPost]
        public async Task<ActionResult<Marca>> PostMarca(Marca marca)
        {
            try
            {
                _context.Marcas.Add(marca);
                await _context.SaveChangesAsync();
                return Ok(new AuthResult()
                {
                    Result = true,
                    Contenido = "Elemento agregado correctamente"
                });
            }
            catch (DbUpdateConcurrencyException)
            {
                return BadRequest(error: new AuthResult()
                {
                    Result = false,
                    Contenido = "No puedes agregar el elemento"
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMarca(int id)
        {
            if (_context.Marcas == null)
            {
                return NotFound();
            }
            var marca = await _context.Marcas.FindAsync(id);
            if (id == 1)
            {
                return BadRequest(error: new AuthResult()
                {
                    Result = false,
                    Contenido = "No puedes eliminar este elemento"
                });
            }
            if (marca == null)
            {
                return BadRequest(error: new AuthResult()
                {
                    Result = false,
                    Contenido = "No se ha encontrado elemento"
                });
            }

            _context.Marcas.Remove(marca);
            await _context.SaveChangesAsync();

            return Ok(new AuthResult()
            {
                Result = true,
                Contenido = "Elemento eliminado correctamente"
            });
        }
    }
}
