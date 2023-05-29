using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using Microsoft.Win32;
using SondeoBackend.Configuration;
using SondeoBackend.Context;
using SondeoBackend.DTO;
using SondeoBackend.Models;
using System.Collections.ObjectModel;

namespace SondeoBackend.Controllers.Encuestas
{
    [Route("api/[controller]")]
    [ApiController]
    public class EncuestasController : ControllerBase
    {
        private readonly DataContext _context;

        public EncuestasController(DataContext context)
        {
            _context = context;
        }
        
        [Route("EncuestasActivas")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Encuesta>>> GetEncuestasActivas()
        {
            if (_context.Encuestas == null)
            {
                return NotFound();
            }
            return await _context.Encuestas.Include(e => e.Medicion).Where(e=> e.Medicion.Finalizada== false).ToListAsync();
        }

        [Route("EncuestasFromLocal/{id}")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Encuesta>>> GetEncuestasFromLocal(int id)
        {
            if (_context.Encuestas == null)
            {
                return NotFound();
            }
            return await _context.Encuestas.Include(e => e.Medicion).Where(e => e.LocalId == id).ToListAsync();
        }

        [Route("ProductosEncuesta/{id}")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Encuesta>>> GetProductosEncuesta(int id)
        {
            if (_context.Encuestas == null)
            {
                return NotFound();
            }
            return await _context.Encuestas.Include(e => e.Medicion).Include(e => e.DetalleEncuestas).Where(e => e.Id == id).ToListAsync();
        }

        [Route("CrearEncuesta")]
        [HttpPost]
        public async Task<IActionResult> CrearEncuesta(RegistroEncuesta registro)
        {
            var MoreCurrentDate = new DateTime();
            var ultimaMedicion = new Medicion();
            var encuesta = new Encuesta();
            var medicion = new Medicion();
            var local = await _context.Locales.Include(e=>e.Encuestas).Include(e => e.Mediciones).FirstOrDefaultAsync(i => i.Id == registro.LocalId);
            if (local == null)
            {
                return BadRequest(error: new ModelResult()
                {
                    Result = false,
                    Errors = new List<string>()
                        {
                            "No se encontro el local asignado"
                        }
                });
            }
            if(local.Mediciones == null || local.Mediciones.Count == 0)
            {
                medicion = new Medicion
                {
                    Finalizada = false,
                    FechaRealizada = DateTime.Now,
                    LocalesId = registro.LocalId
                };
                _context.Mediciones.Add(medicion);
                await _context.SaveChangesAsync();
                encuesta = new Encuesta
                {
                    FechaInicio = DateTime.Now,
                    FechaCierre = null,
                    DiasTrabajados = 0,
                    CustomUserId = registro.UserId,
                    LocalId = registro.LocalId,
                    MedicionId = medicion.Id
                };                
                _context.Encuestas.Add(encuesta);
                await _context.SaveChangesAsync();
                return Ok(new ModelResult()
                {
                    Result = true,
                    Contenido = "Encuesta creado correctamente"
                });
            }
            foreach (Medicion mediciones in local.Mediciones)
            {
                if(MoreCurrentDate < mediciones.FechaRealizada)
                {
                    MoreCurrentDate = mediciones.FechaRealizada;
                    ultimaMedicion = mediciones;
                }
            }
            if (!ultimaMedicion.Finalizada)
            {
                return BadRequest(error: new ModelResult()
                {
                    Result = false,
                    Errors = new List<string>()
                        {
                            "Ya hay una encuesta activa para este local"
                        }
                });                
            }
            medicion = new Medicion
            {
                Finalizada = false,
                FechaRealizada = DateTime.Now,
                LocalesId = registro.LocalId
            };
            _context.Mediciones.Add(medicion);
            await _context.SaveChangesAsync();
            encuesta = new Encuesta
            {
                FechaInicio = DateTime.Now,
                FechaCierre = null,
                DiasTrabajados = 0,
                CustomUserId = registro.UserId,
                LocalId = registro.LocalId,
                MedicionId = medicion.Id
            };
            _context.Encuestas.Add(encuesta);
            await _context.SaveChangesAsync();
            var ultimaEncuesta = await _context.Encuestas.Where(e => e.MedicionId == ultimaMedicion.Id).Include(e=>e.DetalleEncuestas).FirstOrDefaultAsync();
            if(ultimaEncuesta.DetalleEncuestas != null || ultimaEncuesta.DetalleEncuestas.Count != 0)
            {
                var nuevoDetalleEncuestas = new Collection<DetalleEncuesta>();
                foreach(DetalleEncuesta detalle in ultimaEncuesta.DetalleEncuestas)
                {
                    var nuevoDetalleEncuesta = new DetalleEncuesta
                    {
                        StockInicial = detalle.StockFinal,
                        StockFinal = 0,
                        Compra = detalle.Compra,
                        Pvd = detalle.Pvd,
                        Pvp = detalle.Pvp,
                        EncuestaId = encuesta.Id,
                        ProductoId = detalle.ProductoId,
                    };
                    nuevoDetalleEncuestas.Add(nuevoDetalleEncuesta);
                }
                encuesta.DetalleEncuestas = nuevoDetalleEncuestas;
                await _context.SaveChangesAsync();
                return Ok(new ModelResult()
                {
                    Result = true,
                    Contenido = "Encuesta creado correctamente"
                });
            }
            return Ok(new ModelResult()
            {
                Result = true,
                Contenido = "Encuesta creado correctamente"
            });
        }

        [Route("AgregarProductoEncuesta")]
        [HttpPost]
        public async Task<IActionResult> AgregarProductoEncuesta(RegistroProductoEncuesta registro)
        {
            var detalleEncuesta = new DetalleEncuesta
            {
                StockInicial = registro.StockInicial,
                StockFinal = null,
                Compra  = registro.Compra,
                Pvd = registro.Pvd,
                Pvp = registro.Pvp,
                EncuestaId = registro.EncuestaId,
                ProductoId = registro.ProductoId
            };
            var productoEncuesta = await _context.DetalleEncuestas.Where(e=>e.ProductoId == registro.ProductoId && e.EncuestaId == registro.EncuestaId).FirstOrDefaultAsync();
            if (productoEncuesta== null)
            {
                _context.DetalleEncuestas.Add(detalleEncuesta);
                await _context.SaveChangesAsync();
                return Ok(new ModelResult()
                {
                    Result = true,
                    Contenido = "Encuesta creado correctamente"
                });
            }
            return BadRequest(error: new ModelResult()
            {
                Result = false,
                Errors = new List<string>()
                        {
                            "Este producto ya esta asignado a esta encuesta"
                        }
            });
        }

        [Route("CerrarProducto/{id}")]
        [HttpPost]
        public async Task<IActionResult> CerrarProducto(int id, CerrarProducto registro)
        {
            if(id != registro.Id)
            {
                return BadRequest(error: new ModelResult()
                {
                    Result = false,
                    Errors = new List<string>()
                        {
                            "El identificador no coincide"
                        }
                });
            }
            var detalle = await _context.DetalleEncuestas.FindAsync(id);
            detalle.StockInicial = registro.StockInicial;
            detalle.StockFinal = registro.StockFinal;
            detalle.Compra = registro.Compra;
            detalle.Pvp = registro.Pvp;
            detalle.Pvd = registro.Pvd;
            await _context.SaveChangesAsync();
            return Ok(new ModelResult()
            {
                Result = true,
                Contenido = "Producto cerrado correctamente"
            });
        }

        [Route("TransferirEncuesta")]
        [HttpPost]
        public async Task<IActionResult> TransferirEncuesta(int idEncuesta, int idUsuario)
        {
            var encuesta = await _context.Encuestas.Include(e => e.Medicion).FirstOrDefaultAsync(i => i.Id == idEncuesta);
            if (encuesta.Medicion.Finalizada)
            {
                var medicion = new Medicion
                {
                    Finalizada = false,
                    FechaRealizada = DateTime.Now,
                    LocalesId = encuesta.LocalId
                };
                _context.Mediciones.Add(medicion);
                await _context.SaveChangesAsync();
                var encuestaNueva = new Encuesta
                {
                    FechaInicio = DateTime.Now,
                    FechaCierre = null,
                    DiasTrabajados = null,
                    CustomUserId = idUsuario,
                    LocalId = encuesta.LocalId,
                    MedicionId = medicion.Id
                };
                _context.Encuestas.Add(encuestaNueva);
                await _context.SaveChangesAsync();
                var ultimaEncuesta = await _context.Encuestas.Where(e => e.MedicionId == encuesta.Medicion.Id).Include(e => e.DetalleEncuestas).FirstOrDefaultAsync();
                if (ultimaEncuesta.DetalleEncuestas != null || ultimaEncuesta.DetalleEncuestas.Count != 0)
                {
                    var nuevoDetalleEncuestas = new Collection<DetalleEncuesta>();
                    foreach (DetalleEncuesta detalle in ultimaEncuesta.DetalleEncuestas)
                    {
                        var nuevoDetalleEncuesta = new DetalleEncuesta
                        {
                            StockInicial = detalle.StockFinal,
                            StockFinal = null,
                            Compra = detalle.Compra,
                            Pvd = detalle.Pvd,
                            Pvp = detalle.Pvp,
                            EncuestaId = encuesta.Id,
                            ProductoId = detalle.ProductoId,
                        };
                        nuevoDetalleEncuestas.Add(nuevoDetalleEncuesta);
                    }
                    encuesta.DetalleEncuestas = nuevoDetalleEncuestas;
                    await _context.SaveChangesAsync();
                    return Ok(new ModelResult()
                    {
                        Result = true,
                        Contenido = "Encuesta creado correctamente"
                    });
                }
            }            
            encuesta.CustomUserId = idUsuario;
            await _context.SaveChangesAsync();
            return Ok(new ModelResult()
            {
                Result = true,
                Contenido = "Encuesta creado correctamente"
            });
        }

        [Route("CerrarEncuesta")]
        [HttpPost]
        public async Task<IActionResult> CerrarEncuesta(int idEncuesta)
        {
            try
            {
                int cont = 0;
                var encuesta = _context.Encuestas.Include(e => e.Medicion).Include(e => e.DetalleEncuestas).FirstOrDefault(i => i.Id == idEncuesta);
                foreach(DetalleEncuesta detalle in encuesta.DetalleEncuestas)
                {
                    if(detalle.StockFinal == null)
                    {
                        cont++;
                    }
                }
                if (cont != 0)
                {
                    return BadRequest(error: new ModelResult()
                    {
                        Result = false,                        
                        Errors = new List<string>()
                        {
                            "No puedes cerrar esta encuesta, todavia tienes productos que no has terminado"
                        }
                    });
                }
                encuesta.FechaCierre = DateTime.Now;
                encuesta.DiasTrabajados = (int?)(encuesta.FechaCierre - encuesta.FechaInicio).Value.TotalDays;
                encuesta.Medicion.Finalizada = true;
                await _context.SaveChangesAsync();
                return Ok(new ModelResult()
                {
                    Result = true,
                    Contenido = "Encuesta cerrada correctamente"
                });
            }
            catch(Exception ex)
            {
                return BadRequest(error: new ModelResult()
                {
                    Result = false,
                    Contenido =  "No se pudo cerrar la encuesta",                
                    Errors = new List<string>()
                        {
                            ex.Message
                        }
                });
            }
        }
    }
}
