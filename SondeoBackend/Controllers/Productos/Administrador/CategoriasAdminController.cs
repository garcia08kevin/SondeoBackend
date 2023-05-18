using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SondeoBackend.Context;
using SondeoBackend.Models;

namespace SondeoBackend.Controllers.Productos.Administrador
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriasAdminController : ControllerBase
    {
        private readonly DataContext _context;

        public CategoriasAdminController(DataContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Categoria>>> GetCategorias()
        {
            if (_context.Categorias == null)
            {
                return NotFound();
            }
            return await _context.Categorias.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Categoria>> GetCategoria(int id)
        {
            if (_context.Categorias == null)
            {
                return NotFound();
            }
            var categoria = await _context.Categorias.FindAsync(id);

            if (categoria == null)
            {
                return NotFound();
            }

            return categoria;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutCategoria(int id, Categoria categoria)
        {
            if (id != categoria.Id)
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
            _context.Entry(categoria).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(new AuthResult()
            {
                Result = true,
                Contenido = "Elemento modificado correctamente"
            });
        }

        [HttpPost]
        public async Task<ActionResult<Categoria>> PostCategoria(Categoria categoria)
        {
            try
            {
                _context.Categorias.Add(categoria);
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
        public async Task<IActionResult> DeleteCategoria(int id)
        {
            if (_context.Marcas == null)
            {
                return NotFound();
            }
            var categoria = await _context.Categorias.FindAsync(id);
            if (id == 1)
            {
                return BadRequest(error: new AuthResult()
                {
                    Result = false,
                    Contenido = "No puedes eliminar este elemento"
                });
            }
            if (categoria == null)
            {
                return BadRequest(error: new AuthResult()
                {
                    Result = false,
                    Contenido = "No se ha encontrado elemento"
                });
            }

            _context.Categorias.Remove(categoria);
            await _context.SaveChangesAsync();

            return Ok(new AuthResult()
            {
                Result = true,
                Contenido = "Elemento eliminado correctamente"
            });
        }

        private bool CategoriaExists(int id)
        {
            return (_context.Categorias?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
