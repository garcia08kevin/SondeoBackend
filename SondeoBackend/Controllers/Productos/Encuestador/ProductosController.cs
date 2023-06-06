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
using SondeoBackend.DTO.Encuestador.Registrar;
using SondeoBackend.DTO.Result;
using SondeoBackend.DTO.Encuestador.Mostrar;
using SondeoBackend.DTO.UserControl;
using System.Web.WebPages;

namespace SondeoBackend.Controllers.Productos.Encuestador
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductosController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IHubContext<Hubs> _hubs;
        private readonly UserManager<CustomUser> _userManager;
        private readonly AssignId _assignId;

        public ProductosController(DataContext context, IHubContext<Hubs> hubs, UserManager<CustomUser> userManager, AssignId assignId)
        {
            _userManager = userManager;
            _hubs = hubs;
            _context = context;
            _assignId = assignId;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Producto>>> GetProductos(string email)
        {
            var alias = await _assignId.UserAlias(email);
            return await _context.Productos.Where(e=> e.Activado == true || e.SyncId.Contains(alias)).Include(e => e.Marca).Include(e => e.Categoria).Include(e => e.Propiedades).ToListAsync();
        }

        [HttpGet("{syncId}")]
        public async Task<ActionResult<Producto>> GetProducto(string syncId)
        {
            return await _context.Productos.Include(e => e.Propiedades).Include(e => e.Marca).Include(e => e.Categoria).FirstOrDefaultAsync(i => i.SyncId.Equals(syncId));
        }

        [HttpPut("{syncId}")]
        public async Task<IActionResult> PutProducto(string syncId, RegistroProducto registro)
        {
            if (syncId.Contains("ADMIN"))
            {
                return BadRequest(error: new ObjectResult<Producto>()
                {
                    Result = false,
                    Respose = "No puedes modifica este elemento"
                });
            }
            var producto = await _context.Productos.FirstOrDefaultAsync(i => i.SyncId == syncId);
            var marca = await _context.Marcas.FirstOrDefaultAsync(i => i.SyncId == registro.MarcaSyncId);
            var propiedades = await _context.Propiedades.FirstOrDefaultAsync(i => i.SyncId == registro.PropiedadesSyncId);
            producto.CategoriaId = registro.CategoriaSyncId;
            producto.MarcaId = marca.Id;
            producto.PropiedadesId = propiedades.Id;
            await _context.SaveChangesAsync();
            return Ok(new ObjectResult<Producto>()
            {
                Result = true,
                Respose = "Producto modificado correctamente",
                Object = producto
            });
        }

        [HttpPost]
        public async Task<ActionResult<Producto>> PostProducto([FromForm] RegistroProducto registro, string email, IFormFile? imagen)
        {
            try
            {
                byte[] bytes;
                using (BinaryReader br = new BinaryReader(imagen.OpenReadStream()))
                {
                    bytes = br.ReadBytes((int)imagen.Length);
                }
                var lastProduct = await _context.Productos.OrderByDescending(producto => producto.Id).FirstOrDefaultAsync();
                var identificador = await _assignId.AssignSyncId(lastProduct == null ? "0" : lastProduct.SyncId, email);
                if (identificador.IsEmpty())
                {
                    return BadRequest(error: new ObjectResult<Producto>()
                    {
                        Result = false,
                        Respose = "El email que ingresaste no pertenece a ningun usuario"
                    });
                };

                var productoConfirmacion = await _context.Productos.Where(p => p.Nombre.Equals(registro.Nombre)).FirstOrDefaultAsync();
                if(productoConfirmacion != null)
                {
                    return BadRequest(error: new ObjectResult<Producto>()
                    {
                        Result = false,
                        Respose = "Ya hay un producto con el mismo nombre"
                    });
                }
                if (lastProduct == null)
                {
                    identificador = await _assignId.AssignSyncId("0", email);
                }
                else
                {
                    identificador = await _assignId.AssignSyncId(lastProduct.SyncId, email);
                }
                var marca = await _context.Marcas.FirstOrDefaultAsync(i => i.SyncId == registro.MarcaSyncId);
                var propiedad = await _context.Propiedades.FirstOrDefaultAsync(i => i.SyncId == registro.PropiedadesSyncId);
                var producto = new Producto()
                {
                    Nombre = registro.Nombre,
                    Imagen = bytes,
                    CategoriaId = registro.CategoriaSyncId,
                    MarcaId = marca.Id,
                    PropiedadesId = propiedad.Id,
                    Activado = false,
                    SyncId = identificador
                };
                _context.Productos.Add(producto);
                await _context.SaveChangesAsync();
                return Ok(new ObjectResult<MostrarProducto>()
                {
                    Result = true,
                    Respose = "Producto creado correctamente",
                    Object = new MostrarProducto
                    {
                        SyncId = propiedad.SyncId,
                        Nombre = registro.Nombre,
                        CategoriaId = registro.CategoriaSyncId,
                        MarcaSyncId = marca.SyncId,
                        PropiedadSyncId = propiedad.SyncId
                }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(error: new ObjectResult<Producto>()
                {
                    Result = false,
                    Respose = $"No se pudo crear el producto {ex.Message}"
                });
            }            
        }

        [HttpDelete("{syncId}")]
        public async Task<IActionResult> DeleteProducto(string syncId)
        {
            var producto = await _context.Productos.FirstOrDefaultAsync(i => i.SyncId == syncId);
            if (producto == null)
            {
                return BadRequest(error: new ObjectResult<Producto>()
                {
                    Result = false,
                    Respose = "No se encontro el producto"
                });
            }
            if (syncId.Contains("ADMIN"))
            {
                return BadRequest(error: new ObjectResult<Producto>()
                {
                    Result = false,
                    Respose = "No puedes eliminar este producto"
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
