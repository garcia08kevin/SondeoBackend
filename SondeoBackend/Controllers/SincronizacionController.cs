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
using SondeoBackend.DTO;
using SondeoBackend.DTO.Sincronizacion;
using static SondeoBackend.DTO.Sincronizacion.EnviarDatos;
using System.Text;
using System.Web.WebPages;

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
        private readonly ManageProductosController _manageProductos;
        private readonly ManageLocalesController _manageLocales;

        public SincronizacionController(DataContext context, ManageLocalesController manageLocales, ManageProductosController manageProductos, IHubContext<Hubs> hubs, UserManager<CustomUser> userManager, AssignId assignId)
        {
            _userManager = userManager;
            _manageProductos = manageProductos;
            _manageLocales = manageLocales;
            _hubs = hubs;
            _context = context;
            _assignId = assignId;
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
                FechaInicio = registro.FechaInicio,
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
                encuesta.DiasTrabajados = DateTime.Now.Day - encuesta.FechaInicio.Value.Day;
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

        #region Sincronizar Datos
        [Route("SincronizacionCompleta")]
        [HttpPost]
        public async Task<ActionResult<Local>> SendSync(SendSyncDto data)
        {
            try
            {                
                await PostLocales(data.Locales);
                await PostEncuestas(data.Encuestas);
                await PostProducto(data.Productos);
                await PostDetalleEncuesta(data.DetalleEncuestas);
                return Ok(new ObjectResult<SendSyncDto>()
                {
                    Result = true,
                    Respose = "Soncronizacion realizada correctamente"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(error: new ObjectResult<Local>()
                {
                    Result = false,
                    Respose = $"No se pudo agregar el local {ex.Message}"
                });
            }
        }

        [Route("Productos")]
        [HttpPost]
        public async Task<ActionResult<Local>> PostProducto(List<ProductoDto> enviarProducto)
        {
            foreach (ProductoDto producto in enviarProducto)
            {
                var propiedad = new Propiedades
                {
                    Id = producto.id_propiedades,
                    NombrePropiedades = producto.propiedad
                };
                var propiedadConfirmacion = await _context.Propiedades.Where(p => p.NombrePropiedades.Equals(producto.propiedad)).FirstOrDefaultAsync();
                if (propiedadConfirmacion == null)
                {
                    _context.Propiedades.Add(propiedad);
                    await _context.SaveChangesAsync();
                }                
                var comprobarCodigo = await _context.Productos.FirstOrDefaultAsync(e => e.BarCode == producto.Id);
                if (comprobarCodigo == null)
                {
                    var create = new Producto
                    {
                        BarCode = producto.Id,
                        CategoriaId = producto.Id_categoria,
                        MarcaId = producto.Id_marca,
                        PropiedadesId = propiedadConfirmacion == null ? propiedad.Id : propiedadConfirmacion.Id,
                        Imagen = producto.Foto.IsEmpty() || producto.Foto.Equals("") ? null : Encoding.ASCII.GetBytes(producto.Foto)

                    };
                    _context.Productos.Add(create);
                    await _context.SaveChangesAsync();
                };                
            }
            return Ok(new ObjectResult<Producto>()
            {
                Result = true,
                Respose = "Productos sincronizadas correctamente"
            });
        }

        [Route("Locales")]
        [HttpPost]
        public async Task<ActionResult<Local>> PostLocales(List<EnviarLocalesDto> enviarLocales)
        {
            foreach (EnviarLocalesDto local in enviarLocales)
            {
                var create = new Local
                {
                    Id = local.Id,
                    Nombre = local.Nombre,
                    Direccion = local.Direccion,
                    Latitud = (float)local.Latitud,
                    Longitud = (float)local.Longitud,
                    CanalId = local.Id_canal,
                    Habilitado = local.Habilitado
                };
                await _manageLocales.PostLocal(create);
                await _context.SaveChangesAsync();
            }
            return Ok(new ObjectResult<Producto>()
            {
                Result = true,
                Respose = "Locales sincronizados correctamente"
            });
        }

        [Route("Encuestas")]
        [HttpPost]
        public async Task<ActionResult<Encuesta>> PostEncuestas(List<EnviarEncuestasDto> enviarEncuesta)
        {
            foreach (EnviarEncuestasDto encuesta in enviarEncuesta)
            {
                var create = new Encuesta
                {
                    Id = encuesta.Id,
                    CustomUserId = encuesta.Id_encuestador,
                    LocalId = encuesta.Id_local,
                    MedicionId = encuesta.Id_medicion,
                    FechaInicio = DateTime.ParseExact(encuesta.Fecha_init, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
                    FechaCierre = DateTime.ParseExact(encuesta.Fecha_cierre, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
                    DiasTrabajados = encuesta.Dias_trabajados,
                    Visita = encuesta.Visita
                };
                _context.Encuestas.Add(create);
                await _context.SaveChangesAsync();
            }
            return Ok(new ObjectResult<Encuesta>()
            {
                Result = true,
                Respose = "Encuestas sincronizadas correctamente"
            });
        }

        [Route("DetalleEncuesta")]
        [HttpPost]
        public async Task<ActionResult<Local>> PostDetalleEncuesta(List<DetalleEncuestaDto> enviarDetalleEncuesta)
        {
            foreach (DetalleEncuestaDto detalleEncuesta in enviarDetalleEncuesta)
            {
                var encuesta = await _context.Encuestas.FirstOrDefaultAsync(e=> e.Id == detalleEncuesta.Id_encuesta);
                if(encuesta != null)
                {
                    var create = new DetalleEncuesta
                    {
                        Id = encuesta.Id,
                        EncuestaId = encuesta.Id,
                        ProductoId = detalleEncuesta.Id_producto,
                        StockInicial = detalleEncuesta.Stock_init,
                        StockFinal = detalleEncuesta.Stock_fin,
                        Compra = detalleEncuesta.Compra,
                        Pvd = detalleEncuesta.Pvd,
                        Pvp = detalleEncuesta.Pvp
                    };
                    _context.DetalleEncuestas.Add(create);
                    await _context.SaveChangesAsync();
                }
                
            }
            return Ok(new ObjectResult<Producto>()
            {
                Result = true,
                Respose = "Detalles de encuesta sincronizados correctamente"
            });
        }
        #endregion

        #region Descargar Datos
        [HttpGet("Mediciones")]
        public async Task<ActionResult<IEnumerable<Medicion>>> GetMedicion()
        {
            return await _context.Mediciones.Where(e => e.Activa).ToListAsync();
        }

        [Route("DescargarEncuestas")]
        [HttpPost]
        public async Task<ActionResult<IEnumerable<EnviarEncuestasDto>>> GetEncuestas(PeticionEncuestas sincronizacion)
        {
            var encuestas = await _context.Encuestas.Include(e => e.DetalleEncuestas).Where(m => m.CustomUserId == sincronizacion.UsuarioId && m.MedicionId == sincronizacion.MedicionId).ToListAsync();
            return encuestas.Select(e => new EnviarEncuestasDto
            {
                Id = e.Id,
                Id_encuestador = e.CustomUserId,
                Id_local = e.LocalId,
                Id_medicion = e.MedicionId,
                Fecha_init = e.FechaInicio.ToString(),
                Fecha_cierre = e.FechaCierre.ToString(),
                Dias_trabajados = e.DiasTrabajados,
                Visita = e.Visita,
                Habilitado = true
            }).ToList();
        }

        [Route("Productos")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductoDtoResponse>>> GetProductos()
        {
            return await _context.Productos.Select(p => new ProductoDtoResponse
            {
                BarCode = p.BarCode,
                Nombre = p.Nombre,
                Activado = p.Activado,
                CategoriaId = p.CategoriaId,
                MarcaId = p.MarcaId,
                PropiedadesId = p.PropiedadesId
            }).ToListAsync();
         }        

        [Route("Ciudades")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Ciudad>>> GetCiudades()
        {
            return await _manageLocales.GetCiudad();
        }

        [HttpPost("DescargarLocales")]
        public async Task<ActionResult<IEnumerable<EnviarLocalesDto>>> GetLocales(PeticionLocales sincronizacion)
        {
            List<Local> locales = new List<Local>();
            var encuestas = await _context.Encuestas.Include(e => e.Medicion).Include(e => e.Local).Where(e => e.Medicion.Activa && e.CustomUserId == sincronizacion.Id_encuestador).ToListAsync();
            foreach(Encuesta encuesta in encuestas)
            {
                locales.Add(encuesta.Local);
            }
            return locales.Select(e=> new EnviarLocalesDto
            {
                Id = e.Id,
                Id_canal = e.CanalId,
                Nombre = e.Nombre,
                Direccion = e.Direccion,
                Latitud = e.Latitud,
                Longitud = e.Longitud,
                Habilitado = e.Habilitado
            }).ToList();
        }

        [Route("Categorias")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Categoria>>> SyncCategorias()
        {
            return await _manageProductos.GetCategorias();
        }

        [Route("Marcas")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Marca>>> SyncMarcas()
        {
            return await _manageProductos.GetMarcas();
        }

        [Route("Propiedades")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Propiedades>>> SyncPropiedades()
        {
            return await _manageProductos.GetPropiedades();
        }

        [Route("Canal")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Canal>>> SyncCanales()
        {
            return await _manageLocales.GetCanales();
        }

        [HttpGet("Productos/{id}")]
        public async Task<ActionResult<Producto>> GetProducto(long id)
        {
            return await _manageProductos.GetProducto(id);
        }

        [HttpPut("Productos/{id}")]
        public async Task<IActionResult> PutProducto(long id, Producto registro)
        {
            return await _manageProductos.PutProducto(registro ,id);
        }

        [HttpDelete("Productos/{id}")]
        public async Task<IActionResult> DeleteProducto(long id)
        {
            return await _manageProductos.DeleteProducto(id);
        }
        #endregion
        
    }
}
