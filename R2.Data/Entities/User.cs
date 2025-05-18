using Microsoft.AspNetCore.Identity;

namespace R2.Data.Entities;

public class User : IdentityUser<int>
{
    // Ces propriétés sont déjà dans IdentityUser : UserName, Email, PasswordHash
    // Donc ne les redéfinit pas

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
