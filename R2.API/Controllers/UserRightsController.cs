using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using R2.Data.Context;
using R2.Data.Entities;

namespace R2.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserRightsController : ControllerBase
{
    private readonly R2DbContext _context;

    public UserRightsController(R2DbContext context)
    {
        _context = context;
    }

    // GET: api/UserRights
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserRight>>> GetUserRights()
    {
        return await _context.UserRights.ToListAsync();
    }

    // GET: api/UserRights/5
    [HttpGet("{id}")]
    public async Task<ActionResult<UserRight>> GetUserRight(int id)
    {
        var userRight = await _context.UserRights.FindAsync(id);

        if (userRight == null)
        {
            return NotFound();
        }

        return userRight;
    }

    // PUT: api/UserRights/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<IActionResult> PutUserRight(int id, UserRight userRight)
    {
        if (id != userRight.Id)
        {
            return BadRequest();
        }

        _context.Entry(userRight).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!UserRightExists(id))
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

    // POST: api/UserRights
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<UserRight>> PostUserRight(UserRight userRight)
    {
        _context.UserRights.Add(userRight);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetUserRight", new { id = userRight.Id }, userRight);
    }

    // DELETE: api/UserRights/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUserRight(int id)
    {
        var userRight = await _context.UserRights.FindAsync(id);
        if (userRight == null)
        {
            return NotFound();
        }

        _context.UserRights.Remove(userRight);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool UserRightExists(int id)
    {
        return _context.UserRights.Any(e => e.Id == id);
    }
}
