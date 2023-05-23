using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SondeoBackend.Context;
using SondeoBackend.Models;
using SondeoBackend.Configuration;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Identity;
using SondeoBackend.DTO;

namespace SondeoBackend.Controllers.Productos.Encuestador
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductosController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IHubContext<Hubs> _hubs;
        private readonly UserManager<CustomUser> _userManager;

        public ProductosController(DataContext context, IHubContext<Hubs> hubs, UserManager<CustomUser> userManager)
        {
            _userManager = userManager;
            _hubs = hubs;
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Producto>>> GetProductos()
        {
            if (_context.Productos == null)
            {
                return NotFound();
            }
            return await _context.Productos.Where(e=> e.Activado == true).Include(e => e.Marca).Include(e => e.Categoria).Include(e => e.Propiedades).ToListAsync();
        }

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
            var categoria = await _context.Categorias.FindAsync(producto.CategoriaId);
            var marca = await _context.Marcas.FindAsync(producto.MarcaId);
            var propiedad = await _context.Propiedades.FindAsync(producto.PropiedadesId);

            var productoF = new Producto()
            {
                Id = producto.Id,
                Activado = producto.Activado,
                Nombre = producto.Nombre,
                Categoria = categoria,
                Marca = marca,
                Propiedades = propiedad,
            };
            return productoF;
        }

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

        [HttpPost]
        public async Task<ActionResult<Producto>> PostProducto(RegistroProducto registro)
        {
            var producto = new Producto()
            {
                Nombre = registro.Nombre,
                CategoriaId = registro.CategoriaId,
                MarcaId = registro.MarcaId,
                PropiedadesId = registro.PropiedadesId,
                Activado = false,
                CustomUserId = registro.CustomUserId
            };           
            var user = await _userManager.FindByIdAsync(Convert.ToString(registro.CustomUserId));
            var categoriaProducto = await _context.Categorias.FindAsync(producto.CategoriaId);
            if (user != null && categoriaProducto != null ) {
                _context.Productos.Add(producto);
                await _context.SaveChangesAsync();
                var mensajeNotificacion = $"El usuario {user.Name} {user.Lastname} ha registrado el producto {producto.Nombre} en la categoria {categoriaProducto.NombreCategoria}";
                var notificacion = new Notification()
                {
                    Tipo = 2,
                    Fecha = DateTime.Now,
                    Mensaje = mensajeNotificacion,
                    Identificacion = producto.Id
                };
                _context.Notifications.Add(notificacion);
                await _hubs.Clients.All.SendAsync("Notificacion", mensajeNotificacion);
                await _context.SaveChangesAsync();
                return Ok(new AuthResult()
                {
                    Result = true,
                    Contenido = "Producto creado correctamente"
                });
            }
            return BadRequest(error: new AuthResult()
            {
                Result = false,
                Errors = new List<string>()
                        {
                            "No se pudo crear el producto"
                        }
            });
        }

        private bool ProductoExists(int id)
        {
            return (_context.Productos?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
