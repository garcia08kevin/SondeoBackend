using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using SondeoBackend.Configuration;
using SondeoBackend.Context;
using SondeoBackend.DTO;
using SondeoBackend.DTO.Result;
using SondeoBackend.Models;
using System.Web.WebPages;

namespace SondeoBackend.Controllers.Productos.Administrador
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductosAdminController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly AssignId _assignId;

        public ProductosAdminController(DataContext context, AssignId assignId)
        {
            _assignId = assignId;
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
        [Route("NoActivados")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Producto>>> GetProductosNoctivados()
        {
            if (_context.Productos == null)
            {
                return NotFound();
            }
            return await _context.Productos.Where(e => !e.Activado).Include(e => e.Marca).Include(e => e.Categoria).Include(e => e.Propiedades).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Producto>> GetProducto(int id)
        {
            var producto = await _context.Productos.Include(e => e.Propiedades).Include(e => e.Marca).Include(e => e.Categoria).FirstOrDefaultAsync(i => i.Id == id);
            if (producto == null)
            {
                return BadRequest();
            }
            return producto;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutProducto(Producto producto, int id)
        {
            var producto_exist = await _context.Productos.FindAsync(id);
            if (producto_exist == null)
            {
                return BadRequest(error: new ObjectResult<Producto>()
                {
                    Result = false,
                    Respose = "No se encontro el producto"
                });
            }
            producto_exist.Activado = producto.Activado;
            producto_exist.Nombre = producto.Nombre;
            producto_exist.PropiedadesId = producto.PropiedadesId;
            producto_exist.MarcaId = producto.MarcaId;
            producto_exist.CategoriaId = producto.CategoriaId;
            await _context.SaveChangesAsync();
            return Ok(new ObjectResult<Producto>()
            {
                Result = true,
                Respose = "Producto modificado correctamente"
            });
        }

        [HttpPost]
        public async Task<ActionResult<Producto>> PostProducto([FromForm] RegistrarProductoAdmin producto)
        {
            byte[] bytes = null;
            if (producto.Imagen != null)
            {
                using (BinaryReader br = new BinaryReader(producto.Imagen.OpenReadStream()))
                {
                    bytes = br.ReadBytes((int)producto.Imagen.Length);
                }
            }
            var comprobarExistente = await _context.Productos.Where(e=> e.Nombre == producto.Nombre).Where(e => e.CategoriaId == producto.CategoriaId).Where(e => e.MarcaId == producto.MarcaId).Where(e => e.PropiedadesId == producto.PropiedadesId).FirstOrDefaultAsync();
            if (comprobarExistente != null)
            {
                return BadRequest(error: new ObjectResult<Producto>()
                {
                    Result = false,
                    Respose = "El producto con las caracteristicas que ingresaste ya esta en el sistema"
                });
            };
            var lastProduct = await _context.Productos.OrderByDescending(producto => producto.Id).FirstOrDefaultAsync();
            var identificador = await _assignId.AssignSyncId(lastProduct == null ? "0" : lastProduct.SyncId, producto.UserEmail);
            if (identificador.IsEmpty()){
                return BadRequest(error: new ObjectResult<Producto>()
                {
                    Result = false,
                    Respose = "El email que ingresaste no pertenece a ningun usuario"
                });
            };
            var productoAgregado = new Producto
            {
                Nombre = producto.Nombre,
                Imagen = bytes == null ? null : bytes,
                Activado = producto.Activado,
                CategoriaId = producto.CategoriaId,
                MarcaId = producto.MarcaId,
                PropiedadesId = producto.PropiedadesId,
                SyncId = identificador

            };
            _context.Productos.Add(productoAgregado);
            await _context.SaveChangesAsync();

            return Ok(new ObjectResult<Producto>()
            {
                Result = true,
                Respose = "Producto creado correctamente",
                Object = productoAgregado
            });
        }

        [Route("CambiarImagen")]
        [HttpPost]
        public async Task<ActionResult<Producto>> CambiarImagen([FromForm] int id, IFormFile Imagen)
        {
            var producto_exist = await _context.Productos.FindAsync(id);
            if (producto_exist == null)
            {
                return BadRequest(error: new ObjectResult<Producto>()
                {
                    Result = false,
                    Respose = "No se encontro el producto"
                });
            }
            byte[] bytes;
            using (BinaryReader br = new BinaryReader(Imagen.OpenReadStream()))
            {
                bytes = br.ReadBytes((int)Imagen.Length);
            }
            producto_exist.Imagen = bytes;
            await _context.SaveChangesAsync();
            return Ok(new ObjectResult<Producto>()
            {
                Result = true,
                Respose = "La imagen fue cambiada exitosamente"
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
                return Ok(new ObjectResult<Producto>()
                {
                    Result = true,
                    Respose = "Producto remplazado correctamente"
                });
            }
            return BadRequest(error: new ObjectResult<Producto>()
            {
                Result = false,
                Respose = "No se pudo remplazar el producto"
            });
        }

        [Route("ActivarProducto")]
        [HttpPost]
        public async Task<ActionResult> ActivarProducto(int id)
        {            
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
            {
                return BadRequest(new ObjectResult<Producto> { Result = false, Respose = "No se pudo encontrar el producto" });
            }
            if (producto.Activado)
            {
                producto.Activado = false;
                await _context.SaveChangesAsync();
                return Ok(new ObjectResult<Producto>
                {
                    Result = true,
                    Respose = "Se ha desactivado el producto correctamente"
                });
            }
            producto.Activado = true;
            await _context.SaveChangesAsync();
            return Ok(new ObjectResult<Producto>
            {
                Result = true,
                Respose = "Se ha activado el producto correctamente"
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProducto(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
            {
                return BadRequest(error: new ObjectResult<Producto>()
                {
                    Result = false,
                    Respose = "No se encontro el producto"
                });
            }
            _context.Productos.Remove(producto);
            await _context.SaveChangesAsync();

            return Ok(new ObjectResult<Producto>()
            {
                Result = true,
                Respose = "Producto eliminado correctamente"
            });
        }
    }
}
