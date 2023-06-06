﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SondeoBackend.Configuration;
using SondeoBackend.Context;
using SondeoBackend.DTO;
using SondeoBackend.DTO.Result;
using SondeoBackend.DTO.UserControl;
using SondeoBackend.Models;

namespace SondeoBackend.Controllers.Productos.Administrador
{
    [Route("api/[controller]")]
    [ApiController]
    public class MarcasAdminController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly AssignId _assignId;

        public MarcasAdminController(DataContext context, AssignId assignId)
        {
            _assignId = assignId;
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Marca>>> GetMarcas()
        {
            return await _context.Marcas.ToListAsync();
        }

        [HttpGet("{id}")]
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

        [HttpPut("{id}")]
        public async Task<IActionResult> PutMarca(int id, Marca marca)
        {
            if (id != marca.Id)
            {
                return BadRequest(error: new ObjectResult<Marca>()
                {
                    Result = false,
                    Respose = "El elemento no coincide"
                });
            }
            if (id == 1)
            {
                return BadRequest(error: new ObjectResult<Marca>()
                {
                    Result = false,
                    Respose = "No puedes modificar este elemento"
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

        [HttpPost]
        public async Task<ActionResult<Marca>> PostMarca(Marca marca, string email)
        {
            try
            {
                var identificador = "";
                _context.Marcas.Add(marca);
                var lastProduct = await _context.Marcas.OrderByDescending(m => m.Id).FirstOrDefaultAsync();
                if (lastProduct == null)
                {
                    identificador = await _assignId.AssignSyncId("0", email);
                }
                else
                {
                    identificador = await _assignId.AssignSyncId(lastProduct.SyncId, email);
                }
                marca.SyncId = identificador;
                await _context.SaveChangesAsync();
                return Ok(new ObjectResult<Marca>()
                {
                    Result = true,
                    Respose = "Elemento agregado correctamente"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(error: new ObjectResult<Marca>() { Result = false, Respose = $"No puedes agregar el elemento {ex.Message}"});
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMarca(int id)
        {
            if (_context.Marcas == null)
            {
                return NotFound();
            }
            var marca = await _context.Marcas.FindAsync(id);
            if (id == 1)
            {
                return BadRequest(error: new ObjectResult<Marca>() { Result = false, Respose = "No puedes eliminar este elemento" });
            }
            if (marca == null)
            {
                return BadRequest(error: new ObjectResult<Marca>() { Result = false, Respose = "No se ha encontrado elemento" });
            }
            _context.Marcas.Remove(marca);
            await _context.SaveChangesAsync();

            return Ok(new ObjectResult<Marca>() { Result = true, Respose = "Elemento eliminado correctamente" });
        }
    }
}
