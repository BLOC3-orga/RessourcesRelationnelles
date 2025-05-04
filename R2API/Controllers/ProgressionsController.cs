using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using R2Model.Context;
using R2Model.Entities;

namespace R2API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProgressionsController : ControllerBase
    {
        private readonly R2DbContext _context;

        public ProgressionsController(R2DbContext context)
        {
            _context = context;
        }

        // GET: api/Progressions
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Progression>>> GetProgressions()
        {
            return await _context.Progressions.ToListAsync();
        }

        // GET: api/Progressions/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Progression>> GetProgression(int id)
        {
            var progression = await _context.Progressions.FindAsync(id);

            if (progression == null)
            {
                return NotFound();
            }

            return progression;
        }

        // PUT: api/Progressions/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProgression(int id, Progression progression)
        {
            if (id != progression.Id)
            {
                return BadRequest();
            }

            _context.Entry(progression).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProgressionExists(id))
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

        // POST: api/Progressions
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Progression>> PostProgression(Progression progression)
        {
            _context.Progressions.Add(progression);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetProgression", new { id = progression.Id }, progression);
        }

        // DELETE: api/Progressions/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProgression(int id)
        {
            var progression = await _context.Progressions.FindAsync(id);
            if (progression == null)
            {
                return NotFound();
            }

            _context.Progressions.Remove(progression);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProgressionExists(int id)
        {
            return _context.Progressions.Any(e => e.Id == id);
        }
    }
}
