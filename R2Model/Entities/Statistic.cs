namespace R2Model.Entities;

public class Statistic
{
    public int Id { get; set; }
    public int RessourcesRead { get; set; }
    public int RessourcesCreated { get; set; }

    public ICollection<Resource> Ressources { get; set; }
    public ICollection<User> Users { get; set; }
}
