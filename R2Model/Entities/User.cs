using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace R2Model.Entities;

public class User : IdentityUser
{
    public string Name { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string Pseudo { get; set; } = null!;
    public string City { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public bool IsAccountActivated { get; set; } = true;

    [ForeignKey(nameof(Role))]
    public int RoleId { get; set; }
    public virtual Role? Role { get; set; }

    public ICollection<Resource>? FavoritesRessources { get; set; } = null!;
    public ICollection<Resource>? ExploitedRessources { get; set; } = null!;
    public ICollection<Resource>? Draftressources { get; set; } = null!;
    public ICollection<Resource>? CreatedRessources { get; set; } = null!;
    public ICollection<Comment>? Comments { get; set; } = null!;

}
