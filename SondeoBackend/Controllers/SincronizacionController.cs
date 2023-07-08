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
using SondeoBackend.DTO.Result;
using SondeoBackend.DTO.Registros;

namespace SondeoBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SincronizacionController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IHubContext<Hubs> _hubs;
        private readonly UserManager<CustomUser> _userManager;
        private readonly AssignId _assignId;

        public SincronizacionController(DataContext context, IHubContext<Hubs> hubs, UserManager<CustomUser> userManager, AssignId assignId)
        {
            _userManager = userManager;
            _hubs = hubs;
            _context = context;
            _assignId = assignId;
        }

        [HttpGet("Medicion/{id}")]
        public async Task<ActionResult<Medicion>> GetMedicion(int id)
        {
            var medicion = await _context.Mediciones.Include(e => e.Encuestas).FirstOrDefaultAsync(i => i.Id == id);
            return Ok(new ObjectResult<Medicion>
            {
                Result = true,
                Object = medicion
            });
        }

        #region Sincronizar Encuesta
        [Route("CrearEncuesta")]
        [HttpPost]
        public async Task<IActionResult> CrearEncuesta(RegistroEncuesta registro)
        {
            var user = await _userManager.FindByIdAsync($"{registro.UserId}");
            var local = await _context.Locales.FindAsync(registro.LocalId);
            if (local == null)
            {
                return BadRequest(error: new ObjectResult<Encuesta>()
                {
                    Result = false,
                    Respose = "No se encontro el local asignado"
                });
            }
            if (!local.Habilitado)
            {
                return BadRequest(error: new ObjectResult<Encuesta>()
                {
                    Result = false,
                    Respose = "Este local no esta habilitado"
                });
            }
            var ultimaEncuesta = await _context.Encuestas.Include(e => e.DetalleEncuestas).OrderByDescending(m => m.Id).FirstOrDefaultAsync();
            var comprobacion = await _context.Encuestas.Where(e => e.MedicionId == registro.MedicionId && e.LocalId == registro.LocalId).OrderByDescending(m => m.Id).FirstOrDefaultAsync();
            if (comprobacion != null)
            {
                return BadRequest(error: new ObjectResult<Encuesta>()
                {
                    Result = false,
                    Respose = "Ya hay una encuesta activa para este local"
                });
            }
            var encuesta = new Encuesta
            {
                FechaInicio = DateTime.Now,
                FechaCierre = null,
                DiasTrabajados = 0,
                CustomUserId = registro.UserId,
                LocalId = registro.LocalId,
                MedicionId = registro.MedicionId,
            };
            _context.Encuestas.Add(encuesta);
            await _context.SaveChangesAsync();
            return Ok(new ObjectResult<Encuesta>
            {
                Result = true,
                Respose = "Encuesta creada correctamente"
            });
        }

        [Route("AgregarProductoEncuesta")]
        [HttpPost]
        public async Task<IActionResult> AgregarProductoEncuesta(RegistroDetalle registro)
        {
            var encuesta = await _context.Encuestas.Include(e => e.CustomUser).FirstOrDefaultAsync(i => i.Id == registro.EncuestaId);
            var lastDetalleEncuesta = await _context.DetalleEncuestas.OrderByDescending(m => m.Id).FirstOrDefaultAsync();
            var detalleEncuesta = new DetalleEncuesta
            {
                StockInicial = registro.StockInicial,
                StockFinal = -1,
                Compra = registro.Compra,
                Pvd = registro.Pvd,
                Pvp = registro.Pvp,
                EncuestaId = registro.EncuestaId,
                ProductoId = registro.ProductoId,
            };
            var productoEncuesta = await _context.DetalleEncuestas.Where(e => e.ProductoId == registro.ProductoId && e.EncuestaId == registro.EncuestaId).FirstOrDefaultAsync();
            if (productoEncuesta == null)
            {
                _context.DetalleEncuestas.Add(detalleEncuesta);
                await _context.SaveChangesAsync();
                return Ok(new ObjectResult<Encuesta>
                {
                    Result = true,
                    Respose = "Producto agregado a la encuesta correctamente"
                });
            }
            return BadRequest(error: new ObjectResult<Encuesta>
            {
                Result = false,
                Respose = "Este producto ya esta asignado a esta encuesta"
            });
        }

        [Route("TerminarProducto/{id}")]
        [HttpPost]
        public async Task<IActionResult> TerminarProducto(int stockFinal, string id)
        {
            var detalle = await _context.DetalleEncuestas.FindAsync(id);
            if (detalle.StockFinal != -1)
            {
                return Ok(new ObjectResult<DetalleEncuesta>
                {
                    Result = true,
                    Respose = "El producto ya ha sido terminado"
                });
            }
            detalle.StockFinal = stockFinal;
            await _context.SaveChangesAsync();
            var cont = 0;
            var encuesta = await _context.Encuestas.FirstOrDefaultAsync(i => i.Id == detalle.EncuestaId);
            foreach (DetalleEncuesta detalles in encuesta.DetalleEncuestas)
            {
                if (detalle.StockFinal == -1)
                {
                    cont++;
                }
            }
            if (cont == 0)
            {
                encuesta.FechaCierre = DateTime.Now;
                encuesta.DiasTrabajados = (DateTime.Now - encuesta.FechaInicio).Value.Days;
                await _context.SaveChangesAsync();
            }
            return Ok(new ObjectResult<DetalleEncuesta>
            {
                Result = true,
                Respose = "Producto registrado"
            });
        }

        [Route("TransferirEncuesta")]
        [HttpPost]
        public async Task<IActionResult> TransferirEncuesta(int idEncuesta, int idUsuario)
        {
            var encuesta = await _context.Encuestas.Include(e => e.Medicion).FirstOrDefaultAsync(i => i.Id == idEncuesta);
            encuesta.CustomUserId = idUsuario;
            await _context.SaveChangesAsync();
            return Ok(new ObjectResult<Encuesta>
            {
                Result = true,
                Respose = "Encuesta transferida correctamente"
            });
        }
        #endregion

        #region Productos
        [Route("Productos")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Producto>>> GetProductos()
        {
            return await _context.Productos.Where(e => e.Activado == true).Include(e => e.Marca).Include(e => e.Categoria).Include(e => e.Propiedades).ToListAsync();
        }

        [HttpGet("Productos/{id}")]
        public async Task<ActionResult<Producto>> GetProducto(int id)
        {
            return await _context.Productos.Include(e => e.Propiedades).Include(e => e.Marca).Include(e => e.Categoria).FirstOrDefaultAsync(i => i.BarCode == id);
        }

        [HttpPut("Productos/{id}")]
        public async Task<IActionResult> PutProducto(int id, RegistroProducto registro)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
            {
                return BadRequest(error: new ObjectResult<Producto>()
                {
                    Result = false,
                    Respose = "El producto no existe"
                });
            }
            _context.Entry(producto).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok(new ObjectResult<Producto>()
            {
                Result = true,
                Respose = "Producto modificado correctamente",
                Object = producto
            });
        }

        [Route("Productos")]
        [HttpPost]
        public async Task<ActionResult<Producto>> PostProducto([FromForm] RegistroProducto registro)
        {
            try
            {
                byte[] bytes = null;
                if (registro.Imagen != null)
                {
                    using (BinaryReader br = new BinaryReader(registro.Imagen.OpenReadStream()))
                    {
                        bytes = br.ReadBytes((int)registro.Imagen.Length);
                    }
                }
                var productoConfirmacion = await _context.Productos.Where(p => p.Nombre.Equals(registro.Nombre)).FirstOrDefaultAsync();
                if (productoConfirmacion != null)
                {
                    return BadRequest(error: new ObjectResult<Producto>()
                    {
                        Result = false,
                        Respose = "Ya hay un producto con el mismo nombre"
                    });
                }
                var producto = new Producto()
                {
                    Nombre = registro.Nombre,
                    Imagen = bytes == null ? null : bytes,
                    CategoriaId = registro.CategoriaId,
                    MarcaId = registro.MarcaId,
                    PropiedadesId = registro.PropiedadesId,
                    Activado = false,
                };
                _context.Productos.Add(producto);
                await _context.SaveChangesAsync();
                return Ok(new ObjectResult<Producto>()
                {
                    Result = true,
                    Respose = "Producto creado correctamente"
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

        [HttpDelete("Productos/{id}")]
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
        #endregion

        
    }
}
