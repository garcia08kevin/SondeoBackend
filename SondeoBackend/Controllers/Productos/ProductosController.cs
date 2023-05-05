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
using SondeoBackend.Configuration;
using Microsoft.AspNetCore.SignalR;

namespace SondeoBackend.Controllers.Productos
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductosController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly AuthenticationController _authentication;
        private readonly IHubContext<Hubs> _hubs;
        private readonly NotificationsController _notificationsController;

        public ProductosController(DataContext context, AuthenticationController authentication, IHubContext<Hubs> hubs, NotificationsController notificationsController)
        {
            _notificationsController= notificationsController;
            _hubs = hubs;
            _context = context;
            _authentication = authentication;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Producto>>> GetProductos()
        {
            if (_context.Productos == null)
            {
                return NotFound();
            }
            return await _context.Productos.ToListAsync();
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

            return producto;
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
            var mensajeNotificacion = $"El usuario {registro.Email} ha registrado el producto {productoEncu.Nombre} en la categoria {categoriaProducto.NombreCategoria}";
            var notificacion = new Notification()
            {
                tipo = 2,
                fecha = DateTime.Now,
                Mensaje = mensajeNotificacion
            };
            _context.Notifications.Add(notificacion);
            await _hubs.Clients.All.SendAsync("ReceiveMessage", mensajeNotificacion);
            await _hubs.Clients.All.SendAsync("nroNotificaciones", _notificationsController.NotificacionesNoLeidas());
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetProducto", new { id = productoEncu.Id }, productoEncu);
        }

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
