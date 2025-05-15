using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace R2Model.Entities;

public class User : IdentityUser<int>
{
    // Ces propriétés sont déjà dans IdentityUser : UserName, Email, PasswordHash
    // Donc on évite de redéfinir "Password" qui pourrait créer des conflits

    public string Name { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Pseudo { get; set; } = null!;
    public string City { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public bool IsAccountActivated { get; set; } = true;
    public ICollection<Resource>? FavoriteResources { get; set; }
    public ICollection<Resource>? ExploitedResources { get; set; }
    public ICollection<Resource>? DraftResources { get; set; }
    public ICollection<Resource>? CreatedResources { get; set; }
    public ICollection<Comment>? Comments { get; set; }
}
