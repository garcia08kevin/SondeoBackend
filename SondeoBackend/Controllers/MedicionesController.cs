using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SondeoBackend.Context;
using SondeoBackend.Models;

namespace SondeoBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MedicionesController : ControllerBase
    {
        private readonly DataContext _context;

        public MedicionesController(DataContext context)
        {
            _context = context;
        }

        // GET: api/Mediciones
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Medicion>>> GetMediciones()
        {
          if (_context.Mediciones == null)
          {
              return NotFound();
          }
            return await _context.Mediciones.ToListAsync();
        }

        // GET: api/Mediciones/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Medicion>> GetMedicion(int id)
        {
          if (_context.Mediciones == null)
          {
              return NotFound();
          }
            var medicion = await _context.Mediciones.FindAsync(id);

            if (medicion == null)
            {
                return NotFound();
            }

            return medicion;
        }

        // PUT: api/Mediciones/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMedicion(int id, Medicion medicion)
        {
            if (id != medicion.Id)
            {
                return BadRequest();
            }

            _context.Entry(medicion).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MedicionExists(id))
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

        // POST: api/Mediciones
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Medicion>> PostMedicion(Medicion medicion)
        {
          if (_context.Mediciones == null)
          {
              return Problem("Entity set 'DataContext.Mediciones'  is null.");
          }
            _context.Mediciones.Add(medicion);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetMedicion", new { id = medicion.Id }, medicion);
        }

        // DELETE: api/Mediciones/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMedicion(int id)
        {
            if (_context.Mediciones == null)
            {
                return NotFound();
            }
            var medicion = await _context.Mediciones.FindAsync(id);
            if (medicion == null)
            {
                return NotFound();
            }

            _context.Mediciones.Remove(medicion);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool MedicionExists(int id)
        {
            return (_context.Mediciones?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
