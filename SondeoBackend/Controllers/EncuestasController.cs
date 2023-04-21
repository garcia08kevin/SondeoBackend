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
    public class EncuestasController : ControllerBase
    {
        private readonly DataContext _context;

        public EncuestasController(DataContext context)
        {
            _context = context;
        }

        // GET: api/Encuestas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Encuesta>>> GetEncuestas()
        {
          if (_context.Encuestas == null)
          {
              return NotFound();
          }
            return await _context.Encuestas.ToListAsync();
        }

        // GET: api/Encuestas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Encuesta>> GetEncuesta(int id)
        {
          if (_context.Encuestas == null)
          {
              return NotFound();
          }
            var encuesta = await _context.Encuestas.FindAsync(id);

            if (encuesta == null)
            {
                return NotFound();
            }

            return encuesta;
        }

        // PUT: api/Encuestas/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEncuesta(int id, Encuesta encuesta)
        {
            if (id != encuesta.Id)
            {
                return BadRequest();
            }

            _context.Entry(encuesta).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EncuestaExists(id))
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

        // POST: api/Encuestas
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Encuesta>> PostEncuesta(Encuesta encuesta)
        {
          if (_context.Encuestas == null)
          {
              return Problem("Entity set 'DataContext.Encuestas'  is null.");
          }
            _context.Encuestas.Add(encuesta);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetEncuesta", new { id = encuesta.Id }, encuesta);
        }

        // DELETE: api/Encuestas/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEncuesta(int id)
        {
            if (_context.Encuestas == null)
            {
                return NotFound();
            }
            var encuesta = await _context.Encuestas.FindAsync(id);
            if (encuesta == null)
            {
                return NotFound();
            }

            _context.Encuestas.Remove(encuesta);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool EncuestaExists(int id)
        {
            return (_context.Encuestas?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
