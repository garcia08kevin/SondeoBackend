using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SondeoBackend.Context;
using SondeoBackend.Models;

namespace SondeoBackend.Controllers.Productos
{
    [Route("api/[controller]")]
    [ApiController]
    public class PropiedadesController : ControllerBase
    {
        private readonly DataContext _context;

        public PropiedadesController(DataContext context)
        {
            _context = context;
        }

        // GET: api/Propiedades
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Propiedades>>> GetPropiedades()
        {
          if (_context.Propiedades == null)
          {
              return NotFound();
          }
            return await _context.Propiedades.ToListAsync();
        }

        // GET: api/Propiedades/5
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

        // PUT: api/Propiedades/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPropiedades(int id, Propiedades propiedades)
        {
            if (id != propiedades.Id)
            {
                return BadRequest();
            }

            _context.Entry(propiedades).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PropiedadesExists(id))
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

        // POST: api/Propiedades
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Propiedades>> PostPropiedades(Propiedades propiedades)
        {
          if (_context.Propiedades == null)
          {
              return Problem("Entity set 'DataContext.Propiedades'  is null.");
          }
            _context.Propiedades.Add(propiedades);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPropiedades", new { id = propiedades.Id }, propiedades);
        }

        // DELETE: api/Propiedades/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePropiedades(int id)
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

            _context.Propiedades.Remove(propiedades);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PropiedadesExists(int id)
        {
            return (_context.Propiedades?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
