using R2Model.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace R2Model.Context;
public class R2DbContext : IdentityDbContext<User>
{
    public DbSet<UserRight> UserRights { get; set; } 
    public DbSet<User> User { get; set; }
    public DbSet<Role> Role { get; set; }
    public DbSet<Statistic> Statistics { get; set; } 
    public DbSet<Resource> Ressources { get; set; } 
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Progression> Progressions { get; set; }

    public R2DbContext(DbContextOptions<R2DbContext> options)
    : base(options)
    {
    }
}
