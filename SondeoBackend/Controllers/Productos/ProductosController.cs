using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SondeoBackend.Context;
using SondeoBackend.Models;
using SondeoBackend.Controllers;

namespace SondeoBackend.Controllers.Productos
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductosController : ControllerBase
    {        
        private readonly DataContext _context;
        private readonly AuthenticationController _authentication;

        public ProductosController(DataContext context, AuthenticationController authentication)
        {
            _context = context;
            _authentication = authentication;
        }

        // GET: api/Productos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Producto>>> GetProductos()
        {
          if (_context.Productos == null)
          {
              return NotFound();
          }
            return await _context.Productos.ToListAsync();
        }

        // GET: api/Productos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Producto>> GetProducto(int id)
        {
          if (_context.Productos == null)
          {
              return NotFound();
          }
            var producto = await _context.Productos.FindAsync(id);

            if (producto == null)
            {
                return NotFound();
            }

            return producto;
        }

        // PUT: api/Productos/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProducto(int id, Producto producto)
        {
            if (id != producto.Id)
            {
                return BadRequest();
            }

            _context.Entry(producto).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductoExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Productos
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Producto>> PostProducto(RegistroProducto registro)
        {
          if (_context.Productos == null)
          {
              return Problem("Entity set 'DataContext.Productos'  is null.");
          }
            var productoEncu = new Producto()
            {
                Nombre = registro.Nombre,
                CategoriaId = registro.CategoriaId,
                MarcaId = registro.MarcaId,
                PropiedadesId = registro.PropiedadesId,
                Activado = false
            };
            _context.Productos.Add(productoEncu);
            await _context.SaveChangesAsync();
            var categoriaProducto = await _context.Categorias.FindAsync(productoEncu.CategoriaId);
            var notificacion = new Notification()
            {
                tipo = 2,
                fecha = DateTime.Now,
                Mensaje = $"El usuario {registro.Email} ha registrado el producto {productoEncu.Nombre} en la categoria {categoriaProducto.NombreCategoria}"
            };
            _context.Notifications.Add(notificacion);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetProducto", new { id = productoEncu.Id }, productoEncu);
        }

        // DELETE: api/Productos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProducto(int id)
        {
            if (_context.Productos == null)
            {
                return NotFound();
            }
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
            {
                return NotFound();
            }

            _context.Productos.Remove(producto);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProductoExists(int id)
        {
            return (_context.Productos?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
