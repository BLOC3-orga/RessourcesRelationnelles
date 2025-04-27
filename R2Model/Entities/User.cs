using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace R2Model.Entities;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string Pseudo { get; set; } = null!;
    public string City { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public bool IsAccountActivated { get; set; } = true;

    [ForeignKey(nameof(Role))]
    public int RoleId { get; set; }
    public virtual Role? Role { get; set; }

    public ICollection<Ressource>? FavoritesRessources { get; set; } = null!;
    public ICollection<Ressource>? ExploitedRessources { get; set; } = null!;
    public ICollection<Ressource>? Draftressources { get; set; } = null!;
    public ICollection<Ressource>? CreatedRessources { get; set; } = null!;
    public ICollection<Comment>? Comments { get; set; } = null!;

}
