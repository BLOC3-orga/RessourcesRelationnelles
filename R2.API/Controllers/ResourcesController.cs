using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using R2.Data.Context;
using R2.Data.Entities;

namespace R2.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ResourcesController : ControllerBase
{
    private readonly R2DbContext _context;

    public ResourcesController(R2DbContext context)
    {
        _context = context;
    }

    // GET: api/Resources
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Resource>>> GetRessources()
    {
        return await _context.Ressources.ToListAsync();
    }

    // GET: api/Resources/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Resource>> GetResource(int id)
    {
        var resource = await _context.Ressources.FindAsync(id);

        if (resource == null)
        {
            return NotFound();
        }

        return resource;
    }

    // PUT: api/Resources/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<IActionResult> PutResource(int id, Resource resource)
    {
        if (id != resource.Id)
        {
            return BadRequest();
        }

        _context.Entry(resource).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ResourceExists(id))
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

    // POST: api/Resources
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<Resource>> PostResource(Resource resource)
    {
        _context.Ressources.Add(resource);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetResource", new { id = resource.Id }, resource);
    }

    // DELETE: api/Resources/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteResource(int id)
    {
        var resource = await _context.Ressources.FindAsync(id);
        if (resource == null)
        {
            return NotFound();
        }

        _context.Ressources.Remove(resource);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool ResourceExists(int id)
    {
        return _context.Ressources.Any(e => e.Id == id);
    }
}
