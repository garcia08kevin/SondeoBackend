using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using SondeoBackend.Configuration;
using SondeoBackend.Context;
using SondeoBackend.Models;

namespace SondeoBackend.Controllers.Productos.Administrador
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductosAdminController : ControllerBase
    {
        private readonly DataContext _context;

        public ProductosAdminController(DataContext context)
        {
            _context = context;
        }        

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Producto>>> GetProductos()
        {
            if (_context.Productos == null)
            {
                return NotFound();
            }
            return await _context.Productos.Include(e => e.Marca).Include(e => e.Categoria).Include(e => e.Propiedades).ToListAsync();
        }
        [Route("GetProductosByEncuestador/{id}")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Producto>>> GetProductosByEncuestador(int id, bool todo)
        {
            if (_context.Productos == null)
            {
                return NotFound();
            }
            if(todo)
            {
                return await _context.Productos.Where(e => e.Activado == false).Include(e => e.Marca).Include(e => e.User).Include(e => e.Categoria).Include(e => e.Propiedades).ToListAsync();
            }
            return await _context.Productos.Where(e=> e.CustomUserId == id).Include(e=> e.User).Include(e => e.Marca).Include(e => e.Categoria).Include(e => e.Propiedades).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Producto>> GetProducto(int id)
        {
            var producto = await _context.Productos.FindAsync(id);

            if (producto == null)
            {
                return BadRequest(error: new AuthResult()
                {
                    Result = false,
                    Errors = new List<string>()
                        {
                            "No se encontro el producto"
                        }
                });
            }
            var categoria = await _context.Categorias.FindAsync(producto.CategoriaId);
            var marca = await _context.Marcas.FindAsync(producto.MarcaId);
            var propiedad = await _context.Propiedades.FindAsync(producto.PropiedadesId);

            return Ok(new Producto()
            {
                Id = producto.Id,
                CategoriaId = producto.CategoriaId,
                MarcaId= producto.MarcaId,
                PropiedadesId = producto.PropiedadesId,
                Activado = producto.Activado,
                Nombre = producto.Nombre,
                Categoria = categoria,
                Marca = marca,
                Propiedades = propiedad,
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutProducto(Producto producto, int id)
        {
            if (id != producto.Id)
            {
                return BadRequest(error: new AuthResult()
                {
                    Result = false,
                    Errors = new List<string>()
                        {
                            "No se encontro el producto"
                        }
                });
            }
            _context.Entry(producto).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok(new AuthResult()
            {
                Result = true,
                Contenido = "Producto modificado correctamente"
            });
        }

        [HttpPost]
        public async Task<ActionResult<Producto>> PostProducto(Producto producto)
        {
            _context.Productos.Add(producto);
            await _context.SaveChangesAsync();

            return Ok(new AuthResult()
            {
                Result = true,
                Contenido = "Producto creado correctamente"
            });
        }

        [Route("AsignarExistente")]
        [HttpPost]
        public async Task<ActionResult<Producto>> AsignarExistente(int prodSeleccionado ,int prodResplazo)
        {
            var detallesEncuestas = await _context.DetalleEncuestas.ToListAsync();
            foreach(DetalleEncuesta detalle in detallesEncuestas)
            {
                if (detalle.ProductoId == prodSeleccionado)
                {
                    detalle.ProductoId = prodResplazo;
                    await _context.SaveChangesAsync();
                }
            }
            var productoRemplazado = await _context.Productos.FindAsync(prodSeleccionado);
            if(productoRemplazado != null)
            {
                _context.Productos.Remove(productoRemplazado);
                await _context.SaveChangesAsync();
                return Ok(new AuthResult()
                {
                    Result = true,
                    Contenido = "Producto remplazado correctamente"
                });
            }
            return BadRequest(error: new AuthResult()
            {
                Result = false,
                Errors = new List<string>()
                        {
                            "No se pudo remplazar el producto"
                        }
            });
        }

        [Route("ActivarProducto")]
        [HttpPost]
        public async Task<ActionResult> ActivarProducto(int id)
        {
            if (_context.Productos == null)
            {
                return BadRequest(new AuthResult { Result = false, Contenido = "No se pudo encontrar el producto" });
            }
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
            {
                return BadRequest(new AuthResult { Result = false, Contenido = "No se pudo encontrar el producto" });
            }
            if (producto.Activado)
            {
                producto.Activado = false;
                await _context.SaveChangesAsync();
                return Ok(new AuthResult
                {
                    Result = true,
                    Contenido = "Se ha desactivado el producto correctamente"
                });
            }
            producto.Activado = true;
            await _context.SaveChangesAsync();
            return Ok(new AuthResult
            {
                Result = true,
                Contenido = "Se ha activado el producto correctamente"
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProducto(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
            {
                return BadRequest(error: new AuthResult()
                {
                    Result = false,
                    Errors = new List<string>()
                        {
                            "No se encontro el producto"
                        }
                });
            }
            _context.Productos.Remove(producto);
            await _context.SaveChangesAsync();

            return Ok(new AuthResult()
            {
                Result = true,
                Contenido = "Producto eliminado correctamente"
            });
        }
    }
}
