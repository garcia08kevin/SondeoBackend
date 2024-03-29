﻿using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using SondeoBackend.Configuration;
using SondeoBackend.Context;
using SondeoBackend.DTO.Registros;
using SondeoBackend.DTO.Result;
using SondeoBackend.Models;
using System.Data;

namespace SondeoBackend.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrador,Encuestador")]
    [Route("api/[controller]")]
    [ApiController]
    public class ManageProductosController : ControllerBase
    {
        private readonly DataContext _context;

        public ManageProductosController(DataContext context)
        {
            _context = context;
        }

        #region Productos
        [Route("Productos")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Producto>>> GetProductos()
        {
            if (_context.Productos == null)
            {
                return NotFound();
            }
            return await _context.Productos.Include(e => e.Marca).Include(e => e.Categoria).Include(e => e.Propiedades).ToListAsync();
        }
        [Route("Productos/NoActivados")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Producto>>> GetProductosNoctivados()
        {
            if (_context.Productos == null)
            {
                return NotFound();
            }
            return await _context.Productos.Where(e => !e.Activado).Include(e => e.Marca).Include(e => e.Categoria).Include(e => e.Propiedades).ToListAsync();
        }

        [HttpGet("Productos/{id}")]
        public async Task<ActionResult<Producto>> GetProducto(long id)
        {
            var producto = await _context.Productos.Include(e => e.Propiedades).Include(e => e.Marca).Include(e => e.Categoria).FirstOrDefaultAsync(i => i.BarCode == id);
            if (producto == null)
            {
                return BadRequest();
            }
            return producto;
        }

        [HttpPut("Productos/{id}")]
        public async Task<IActionResult> PutProducto(Producto producto, long id)
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
        [Route("Productos")]
        [HttpPost]
        public async Task<ActionResult<Producto>> PostProducto([FromForm] RegistroProducto producto)
        {
            byte[] bytes = null;
            if (producto.Imagen != null)
            {
                using (BinaryReader br = new BinaryReader(producto.Imagen.OpenReadStream()))
                {
                    bytes = br.ReadBytes((int)producto.Imagen.Length);
                }
            }
            var comprobarExistente = await _context.Productos.Where(e => e.Nombre.Equals(producto.Nombre) && e.CategoriaId == producto.CategoriaId && e.PropiedadesId == producto.PropiedadesId && e.MarcaId == producto.MarcaId).FirstOrDefaultAsync();
            if (comprobarExistente != null)
            {
                return Ok(new ObjectResult<Producto>()
                {
                    Result = false,
                    Respose = "El producto con las caracteristicas ingresadas ya esta en el sistema"
                });
            };
            var comprobarCodigo = await _context.Productos.FirstOrDefaultAsync(e=>e.BarCode == producto.BarCode);
            if (comprobarCodigo != null)
            {
                return Ok(new ObjectResult<Producto>()
                {
                    Result = false,
                    Respose = "Este codigo de barra ya esta en uso"
                });
            };
            if (producto.BarCode == null)
            {
                return Ok(new ObjectResult<Producto>()
                {
                    Result = false,
                    Respose = "Ingrese porfavor un codigo de barra"
                });
            }
            var lastProduct = await _context.Productos.OrderByDescending(producto => producto.BarCode).FirstOrDefaultAsync();
            var productoAgregado = new Producto
            {
                Nombre = producto.Nombre,
                Imagen = bytes == null ? null : bytes,
                Activado = producto.Activado,
                CategoriaId = producto.CategoriaId,
                MarcaId = producto.MarcaId,
                PropiedadesId = producto.PropiedadesId,
                BarCode = producto.BarCode
            };
            _context.Productos.Add(productoAgregado);
            _context.Database.OpenConnection();
            _context.SaveChanges();
            
            return Ok(new ObjectResult<Producto>()
            {
                Result = true,
                Respose = "Producto creado correctamente",
                Object = productoAgregado
            });
        }

        [Route("Productos/CambiarImagen")]
        [HttpPost]
        public async Task<ActionResult<Producto>> CambiarImagen([FromForm] long id, IFormFile Imagen)
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

        [Route("Productos/ActivarProducto")]
        [HttpPost]
        public async Task<ActionResult> ActivarProducto(long id)
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

        [HttpDelete("Productos/{id}")]
        public async Task<IActionResult> DeleteProducto(long id)
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

        #region Marcas
        [Route("Marcas")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Marca>>> GetMarcas()
        {
            return await _context.Marcas.ToListAsync();
        }

        [HttpGet("Marcas/{id}")]
        public async Task<ActionResult<Marca>> GetMarca(int id)
        {
            var marca = await _context.Marcas.FindAsync(id);
            if (marca == null)
            {
                return BadRequest(error: new ObjectResult<Marca>()
                {
                    Result = false,
                    Respose = "No se ha encontrado elemento"
                });
            }
            return marca;
        }

        [HttpPut("Marcas/{id}")]
        public async Task<IActionResult> PutMarca(int id, Marca marca)
        {
            try
            {
                if (marca.Id != id)
                {
                    return BadRequest(error: new ObjectResult<Marca>()
                    {
                        Result = false,
                        Respose = "El elemento no coincide"
                    });
                }
                if (id == 1)
                {
                    return Ok(new ObjectResult<Propiedades>()
                    {
                        Result = false,
                        Respose = "No puedes eliminar este elemento"
                    });
                }
                _context.Entry(marca).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok(new ObjectResult<Marca>()
                {
                    Result = true,
                    Respose = "Elemento modificado correctamente"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(error: new ObjectResult<Propiedades>()
                {
                    Result = false,
                    Respose = $"Se ha producido un error {ex.Message}"
                });
            }            
        }

        [Route("Marcas")]
        [HttpPost]
        public async Task<ActionResult<Marca>> PostMarca(Marca marca)
        {
            try
            {
                var verificacion = await _context.Marcas.FirstOrDefaultAsync(e => e.NombreMarca.Equals(marca.NombreMarca));
                if (verificacion != null)
                {
                    return BadRequest(error: new ObjectResult<Producto>()
                    {
                        Result = false,
                        Respose = "Ya hay un marca con el mismo nombre"
                    });
                }
                _context.Marcas.Add(marca);
                await _context.SaveChangesAsync();
                return Ok(new ObjectResult<Marca>()
                {
                    Result = true,
                    Respose = "Elemento agregado correctamente"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(error: new ObjectResult<Marca>() { Result = false, Respose = $"Se ha producido un error {ex.Message}" });
            }
        }

        [HttpDelete("Marcas/{id}")]
        public async Task<IActionResult> DeleteMarca(int id)
        {
            try
            {
                var marca = await _context.Marcas.FindAsync(id);
                if (marca == null)
                {
                    return BadRequest(error: new ObjectResult<Marca>() { Result = false, Respose = "No se ha encontrado el elemento" });
                }
                if (id == 1)
                {
                    return Ok(new ObjectResult<Propiedades>()
                    {
                        Result = false,
                        Respose = "No puedes eliminar este elemento"
                    });
                }
                var verificacion = await _context.Productos.Where(e => e.MarcaId == id).ToListAsync();
                if (verificacion.Count > 0)
                {
                    return Ok(new ObjectResult<Propiedades>()
                    {
                        Result = false,
                        Respose = "Este elemento esta asignado a uno o mas productos"
                    });
                }
                _context.Marcas.Remove(marca);
                await _context.SaveChangesAsync();

                return Ok(new ObjectResult<Marca>() { Result = true, Respose = "Elemento eliminado correctamente" });
            }
            catch (Exception ex)
            {
                return BadRequest(error: new ObjectResult<Propiedades>()
                {
                    Result = false,
                    Respose = $"Se ha producido un error {ex.Message}"
                });
            }
        }
        #endregion

        #region Propiedades
        [Route("Propiedades")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Propiedades>>> GetPropiedades()
        {
            return await _context.Propiedades.ToListAsync();
        }

        [HttpGet("Propiedades/{id}")]
        public async Task<ActionResult<Propiedades>> GetPropiedad(string id)
        {
            var propiedades = await _context.Propiedades.FindAsync(id);
            if (propiedades == null)
            {
                return BadRequest(error: new ObjectResult<Marca>()
                {
                    Result = false,
                    Respose = "No se ha encontrado elemento"
                });
            }
            return propiedades;
        }

        [HttpPut("Propiedades/{id}")]
        public async Task<IActionResult> PutPropiedades(int id, Propiedades propiedades)
        {
            try
            {
                if (propiedades.Id != id)
                {
                    return Ok(new ObjectResult<Propiedades>()
                    {
                        Result = false,
                        Respose = "El elemento no coincide"
                    });
                }
                if (id == 1)
                {
                    return Ok(new ObjectResult<Propiedades>()
                    {
                        Result = false,
                        Respose = "No puedes eliminar este elemento"
                    });
                }
                _context.Entry(propiedades).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok(new ObjectResult<Propiedades>()
                {
                    Result = true,
                    Respose = "Elemento modificado correctamente"
                });
            }
            catch(Exception ex)
            {
                return BadRequest(error: new ObjectResult<Producto>()
                {
                    Result = false,
                    Respose = $"Se ha producido un error {ex.Message}"
                });
            }
        }

        [Route("Propiedades")]
        [HttpPost]
        public async Task<ActionResult<Propiedades>> PostPropiedades(Propiedades propiedades)
        {
            try
            {                
                var propiedadConfirmacion = await _context.Propiedades.Where(p => p.NombrePropiedades.Equals(propiedades.NombrePropiedades)).FirstOrDefaultAsync();
                if (propiedadConfirmacion != null)
                {
                    return BadRequest(error: new ObjectResult<Producto>()
                    {
                        Result = false,
                        Respose = "Ya hay una propiedad con el mismo nombre"
                    });
                }
                _context.Propiedades.Add(propiedades);
                await _context.SaveChangesAsync();
                return Ok(new ObjectResult<Propiedades>()
                {
                    Result = true,
                    Respose = "Elemento agregado correctamente",
                    Object = propiedades
                });
            }
            catch (Exception ex)
            {
                return BadRequest(error: new ObjectResult<Propiedades>()
                {
                    Result = false,
                    Respose = $"Se ha producido un error {ex.Message}"
                });
            }

        }

        [HttpDelete("Propiedades/{id}")]
        public async Task<IActionResult> DeletePropiedades(int id)
        {
            try
            {
                var propiedades = await _context.Propiedades.FindAsync(id);
                if (propiedades == null)
                {
                    return BadRequest(error: new ObjectResult<Propiedades>()
                    {
                        Result = false,
                        Respose = "No se ha encontrado el elemento"
                    });
                }
                if (id == 1)
                {
                    return Ok(new ObjectResult<Propiedades>()
                    {
                        Result = false,
                        Respose = "No puedes eliminar este elemento"
                    });
                }
                var verificacion = await _context.Productos.Where(e => e.PropiedadesId == id).ToListAsync();
                if (verificacion.Count > 0)
                {
                    return Ok(new ObjectResult<Propiedades>()
                    {
                        Result = false,
                        Respose = "Este elemento esta asignado a uno o mas productos"
                    });
                }
                _context.Propiedades.Remove(propiedades);
                await _context.SaveChangesAsync();

                return Ok(new ObjectResult<Propiedades>()
                {
                    Result = true,
                    Respose = "Elemento eliminado correctamente"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(error: new ObjectResult<Propiedades>()
                {
                    Result = false,
                    Respose = $"Se ha producido un error {ex.Message}"
                });
            }            
        }
        #endregion

        #region Categorias
        [Route("Categorias")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Categoria>>> GetCategorias()
        {
            if (_context.Categorias == null)
            {
                return NotFound();
            }
            return await _context.Categorias.ToListAsync();
        }

        [HttpGet("Categorias/{id}")]
        public async Task<ActionResult<Categoria>> GetCategoria(int id)
        {
            if (_context.Categorias == null)
            {
                return NotFound();
            }
            var categoria = await _context.Categorias.FindAsync(id);

            if (categoria == null)
            {
                return NotFound();
            }

            return categoria;
        }

        [HttpPut("Categorias/{id}")]
        public async Task<IActionResult> PutCategoria(int id, Categoria categoria)
        {
            if (id != categoria.Id)
            {
                return BadRequest(error: new ObjectResult<Categoria>()
                {
                    Result = false,
                    Respose = "El elemento no coincide"
                });
            }
            if (id == 1)
            {
                return BadRequest(error: new ObjectResult<Categoria>()
                {
                    Result = false,
                    Respose = "No puedes modificar este elemento"
                });
            }
            _context.Entry(categoria).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(new ObjectResult<Categoria>()
            {
                Result = true,
                Respose = "Elemento modificado correctamente"
            });
        }

        [Route("Categorias")]
        [HttpPost]
        public async Task<ActionResult<Categoria>> PostCategoria(Categoria categoria)
        {
            try
            {
                var verificacion = await _context.Categorias.FirstOrDefaultAsync(e => e.NombreCategoria.Equals(categoria.NombreCategoria));
                if(verificacion != null)
                {
                    return BadRequest(error: new ObjectResult<Producto>()
                    {
                        Result = false,
                        Respose = "Ya hay un categoria con el mismo nombre"
                    });
                }
                _context.Categorias.Add(categoria);
                await _context.SaveChangesAsync();
                return Ok(new ObjectResult<Categoria>()
                {
                    Result = true,
                    Respose = "Elemento agregado correctamente"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(error: new ObjectResult<Categoria>()
                {
                    Result = false,
                    Respose = $"No puedes agregar el elemento {ex}"
                });
            }
        }

        [HttpDelete("Categorias/{id}")]
        public async Task<IActionResult> DeleteCategoria(int id)
        {
            var categoria = await _context.Categorias.FindAsync(id);
            if (id == 1)
            {
                return BadRequest(error: new ObjectResult<Categoria>()
                {
                    Result = false,
                    Respose = "No puedes eliminar este elemento"
                });
            }            
            var verificacion = await _context.Productos.Where(e => e.MarcaId == id).ToListAsync();
            if (verificacion.Count > 0)
            {
                return Ok(new ObjectResult<Propiedades>()
                {
                    Result = false,
                    Respose = "Este elemento esta asignado a uno o mas productos"
                });
            }
            if (categoria == null)
            {
                return BadRequest(error: new ObjectResult<Categoria>()
                {
                    Result = false,
                    Respose = "No se ha encontrado elemento"
                });
            }
            _context.Categorias.Remove(categoria);
            await _context.SaveChangesAsync();

            return Ok(new ObjectResult<Categoria>()
            {
                Result = true,
                Respose = "Elemento eliminado correctamente"
            });
        }
        #endregion
    }
}
