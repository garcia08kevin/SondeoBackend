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
using Microsoft.Win32;

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
            var producto = await _context.Productos.Include(e => e.Propiedades).Include(e => e.Marca).Include(e => e.Categoria).FirstOrDefaultAsync(i => i.Id == id);
            if (producto== null)
            {
                return BadRequest();
            }
            return producto;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutProducto(int id, Producto producto)
        {
            if (id != producto.Id)
            {
                return BadRequest(error: new ModelResult()
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
            return Ok(new ModelResult()
            {
                Result = true,
                Contenido = "Producto modificado correctamente"
            });
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
                return Ok(new ModelResult()
                {
                    Result = true,
                    Contenido = "Producto creado correctamente"
                });
            }
            return BadRequest(error: new ModelResult()
            {
                Result = false,
                Errors = new List<string>()
                        {
                            "No se pudo crear el producto"
                        }
            });
        }
    }
}
