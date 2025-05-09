using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using R2Model.Context;
using R2Model.Entities;

namespace R2API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly R2DbContext _context;
    private readonly UserManager<User> _userManager;

    public UsersController(R2DbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: api/Users
    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetUsers()
    {
        return await _context.Users.ToListAsync();
    }

    // GET: api/Users/5
    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetUser(string id)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
        {
            return NotFound();
        }

        return user;
    }

    // PUT: api/Users/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<IActionResult> PutUser(string id, User user)
    {
        if (id != user.Id)
        {
            return BadRequest();
        }

        // Récupérez l'utilisateur existant
        var existingUser = await _context.Users.FindAsync(id);
        if (existingUser == null)
        {
            return NotFound();
        }

        // Mettez à jour uniquement les propriétés que vous voulez permettre de modifier
        existingUser.Name = user.Name;
        existingUser.LastName = user.LastName;
        existingUser.Pseudo = user.Pseudo;
        existingUser.City = user.City;
        existingUser.Address = user.Address;
        existingUser.IsAccountActivated = user.IsAccountActivated;
        existingUser.RoleId = user.RoleId;
        // Ne modifiez pas les propriétés sensibles comme Email ou UserName ici
        // Utilisez plutôt des méthodes spécifiques de UserManager

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!UserExists(id))
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

    // POST: api/Users
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<User>> PostUser(User user, string password)
    {
        // Utilisez UserManager pour créer un nouvel utilisateur
        var result = await _userManager.CreateAsync(user, password);

        if (result.Succeeded)
        {
            return CreatedAtAction("GetUser", new { id = user.Id }, user);
        }

        // Si la création a échoué, retournez les erreurs
        return BadRequest(result.Errors);
    }

    // DELETE: api/Users/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        // Utilisez UserManager pour supprimer l'utilisateur
        var result = await _userManager.DeleteAsync(user);

        if (result.Succeeded)
        {
            return NoContent();
        }

        return BadRequest(result.Errors);
    }

    // GET: api/Users/5/FavoriteResources
    [HttpGet("{id}/FavoriteResources")]
    public async Task<ActionResult<IEnumerable<Resource>>> GetFavoriteResources(string id)
    {
        var user = await _context.Users
            .Include(u => u.FavoriteResources)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            return NotFound();
        }

        return Ok(user.FavoriteResources ?? new List<Resource>());
    }

    // POST: api/Users/5/FavoriteResources/3
    [HttpPost("{userId}/FavoriteResources/{resourceId}")]
    public async Task<IActionResult> AddFavoriteResource(string userId, int resourceId)
    {
        var user = await _context.Users
            .Include(u => u.FavoriteResources)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return NotFound("User not found");
        }

        var resource = await _context.Ressources.FindAsync(resourceId);
        if (resource == null)
        {
            return NotFound("Resource not found");
        }

        if (user.FavoriteResources == null)
        {
            user.FavoriteResources = new List<Resource>();
        }

        if (!user.FavoriteResources.Any(r => r.Id == resourceId))
        {
            user.FavoriteResources.Add(resource);
            await _context.SaveChangesAsync();
        }

        return NoContent();
    }

    // DELETE: api/Users/5/FavoriteResources/3
    [HttpDelete("{userId}/FavoriteResources/{resourceId}")]
    public async Task<IActionResult> RemoveFavoriteResource(string userId, int resourceId)
    {
        var user = await _context.Users
            .Include(u => u.FavoriteResources)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return NotFound("User not found");
        }

        var resource = user.FavoriteResources?.FirstOrDefault(r => r.Id == resourceId);
        if (resource == null)
        {
            return NotFound("Resource not found in favorites");
        }

        user.FavoriteResources.Remove(resource);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // Méthodes similaires pour ExploitedResources
    [HttpGet("{id}/ExploitedResources")]
    public async Task<ActionResult<IEnumerable<Resource>>> GetExploitedResources(string id)
    {
        var user = await _context.Users
            .Include(u => u.ExploitedResources)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            return NotFound();
        }

        return Ok(user.ExploitedResources ?? new List<Resource>());
    }

    [HttpPost("{userId}/ExploitedResources/{resourceId}")]
    public async Task<IActionResult> AddExploitedResource(string userId, int resourceId)
    {
        var user = await _context.Users
            .Include(u => u.ExploitedResources)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return NotFound("User not found");
        }

        var resource = await _context.Ressources.FindAsync(resourceId);
        if (resource == null)
        {
            return NotFound("Resource not found");
        }

        if (user.ExploitedResources == null)
        {
            user.ExploitedResources = new List<Resource>();
        }

        if (!user.ExploitedResources.Any(r => r.Id == resourceId))
        {
            user.ExploitedResources.Add(resource);
            await _context.SaveChangesAsync();
        }

        return NoContent();
    }

    [HttpDelete("{userId}/ExploitedResources/{resourceId}")]
    public async Task<IActionResult> RemoveExploitedResource(string userId, int resourceId)
    {
        var user = await _context.Users
            .Include(u => u.ExploitedResources)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return NotFound("User not found");
        }

        var resource = user.ExploitedResources?.FirstOrDefault(r => r.Id == resourceId);
        if (resource == null)
        {
            return NotFound("Resource not found in exploited resources");
        }

        user.ExploitedResources.Remove(resource);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // Méthodes similaires pour DraftResources
    [HttpGet("{id}/DraftResources")]
    public async Task<ActionResult<IEnumerable<Resource>>> GetDraftResources(string id)
    {
        var user = await _context.Users
            .Include(u => u.DraftResources)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            return NotFound();
        }

        return Ok(user.DraftResources ?? new List<Resource>());
    }

    [HttpPost("{userId}/DraftResources/{resourceId}")]
    public async Task<IActionResult> AddDraftResource(string userId, int resourceId)
    {
        var user = await _context.Users
            .Include(u => u.DraftResources)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return NotFound("User not found");
        }

        var resource = await _context.Ressources.FindAsync(resourceId);
        if (resource == null)
        {
            return NotFound("Resource not found");
        }

        if (user.DraftResources == null)
        {
            user.DraftResources = new List<Resource>();
        }

        if (!user.DraftResources.Any(r => r.Id == resourceId))
        {
            user.DraftResources.Add(resource);
            await _context.SaveChangesAsync();
        }

        return NoContent();
    }

    [HttpDelete("{userId}/DraftResources/{resourceId}")]
    public async Task<IActionResult> RemoveDraftResource(string userId, int resourceId)
    {
        var user = await _context.Users
            .Include(u => u.DraftResources)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return NotFound("User not found");
        }

        var resource = user.DraftResources?.FirstOrDefault(r => r.Id == resourceId);
        if (resource == null)
        {
            return NotFound("Resource not found in draft resources");
        }

        user.DraftResources.Remove(resource);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // Méthodes similaires pour CreatedResources
    [HttpGet("{id}/CreatedResources")]
    public async Task<ActionResult<IEnumerable<Resource>>> GetCreatedResources(string id)
    {
        var user = await _context.Users
            .Include(u => u.CreatedResources)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            return NotFound();
        }

        return Ok(user.CreatedResources ?? new List<Resource>());
    }

    [HttpPost("{userId}/CreatedResources/{resourceId}")]
    public async Task<IActionResult> AddCreatedResource(string userId, int resourceId)
    {
        var user = await _context.Users
            .Include(u => u.CreatedResources)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return NotFound("User not found");
        }

        var resource = await _context.Ressources.FindAsync(resourceId);
        if (resource == null)
        {
            return NotFound("Resource not found");
        }

        if (user.CreatedResources == null)
        {
            user.CreatedResources = new List<Resource>();
        }

        if (!user.CreatedResources.Any(r => r.Id == resourceId))
        {
            user.CreatedResources.Add(resource);
            await _context.SaveChangesAsync();
        }

        return NoContent();
    }

    [HttpDelete("{userId}/CreatedResources/{resourceId}")]
    public async Task<IActionResult> RemoveCreatedResource(string userId, int resourceId)
    {
        var user = await _context.Users
            .Include(u => u.CreatedResources)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return NotFound("User not found");
        }

        var resource = user.CreatedResources?.FirstOrDefault(r => r.Id == resourceId);
        if (resource == null)
        {
            return NotFound("Resource not found in created resources");
        }

        user.CreatedResources.Remove(resource);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool UserExists(string id)
    {
        return _context.Users.Any(e => e.Id == id);
    }
}