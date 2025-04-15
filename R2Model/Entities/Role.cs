namespace R2Model.Entities;

public class Role
{
    public int Id { get; set; }
    public string Label { get; set; } = null!;

    public ICollection<UserRight>? UserRights { get; set; } = null!;
}
