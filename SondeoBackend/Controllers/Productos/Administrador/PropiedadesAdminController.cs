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
using SondeoBackend.Models;

namespace SondeoBackend.Controllers.Productos.Administrador
{
    
    [Route("api/[controller]")]
    [ApiController]
    public class PropiedadesAdminController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly AssignId _assignId;

        public PropiedadesAdminController(DataContext context, AssignId assignId)
        {
            _assignId = assignId;
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Propiedades>>> GetPropiedades()
        {
            if (_context.Propiedades == null)
            {
                return NotFound();
            }
            return await _context.Propiedades.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Propiedades>> GetPropiedades(int id)
        {
            if (_context.Propiedades == null)
            {
                return NotFound();
            }
            var propiedades = await _context.Propiedades.FindAsync(id);

            if (propiedades == null)
            {
                return NotFound();
            }

            return propiedades;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutPropiedades(int id, Propiedades propiedades)
        {
            if (id != propiedades.Id)
            {
                return BadRequest(error: new ObjectResult<Propiedades>()
                {
                    Result = false,
                    Respose = "El elemento no coincide"
                });
            }
            if (id == 1)
            {
                return BadRequest(error: new ObjectResult<Propiedades>()
                {
                    Result = false,
                    Respose = "No puedes modificar este elemento"
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

        [HttpPost]
        public async Task<ActionResult<Propiedades>> PostPropiedades(Propiedades propiedades, string email)
        {
            try
            {
                var identificador = "";
                _context.Propiedades.Add(propiedades);
                var lastProduct = await _context.Propiedades.OrderByDescending(m => m.Id).FirstOrDefaultAsync();
                if (lastProduct == null)
                {
                    identificador = await _assignId.AssignSyncId("0", email);
                }
                else
                {
                    identificador = await _assignId.AssignSyncId(lastProduct.SyncId, email);
                }
                propiedades.SyncId = identificador;
                await _context.SaveChangesAsync();
                return Ok(new ObjectResult<Propiedades>()
                {
                    Result = true,
                    Respose = "Elemento agregado correctamente"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(error: new ObjectResult<Propiedades>()
                {
                    Result = false,
                    Respose = $"No se pudo agregar el elemento {ex.Message}"
                });
            }
            
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePropiedades(int id)
        {
            var propiedades = await _context.Propiedades.FindAsync(id);
            if (id == 1)
            {
                return BadRequest(error: new ObjectResult<Propiedades>()
                {
                    Result = false,
                    Respose = "No puedes eliminar este elemento"
                });
            }
            if (propiedades == null)
            {
                return BadRequest(error: new ObjectResult<Propiedades>()
                {
                    Result = false,
                    Respose = "No se ha encontrado el elemento"
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
    }
}
