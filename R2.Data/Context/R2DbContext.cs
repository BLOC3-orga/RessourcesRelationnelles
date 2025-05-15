using R2.Data.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace R2.Data.Context;

public class R2DbContext : IdentityDbContext<User, IdentityRole<int>, int>
{
    public DbSet<UserRight> UserRights { get; set; }
    public DbSet<Statistic> Statistics { get; set; }
    public DbSet<Resource> Ressources { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Progression> Progressions { get; set; }

    public R2DbContext(DbContextOptions<R2DbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuration de la relation User - FavoriteResources
        modelBuilder.Entity<User>()
            .HasMany(u => u.FavoriteResources)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "UserFavoriteResources",
                j => j.HasOne<Resource>().WithMany().HasForeignKey("ResourceId"),
                j => j.HasOne<User>().WithMany().HasForeignKey("UserId")
            );

        // Configuration de la relation User - ExploitedResources
        modelBuilder.Entity<User>()
            .HasMany(u => u.ExploitedResources)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "UserExploitedResources",
                j => j.HasOne<Resource>().WithMany().HasForeignKey("ResourceId"),
                j => j.HasOne<User>().WithMany().HasForeignKey("UserId")
            );

        // Configuration de la relation User - DraftResources
        modelBuilder.Entity<User>()
            .HasMany(u => u.DraftResources)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "UserDraftResources",
                j => j.HasOne<Resource>().WithMany().HasForeignKey("ResourceId"),
                j => j.HasOne<User>().WithMany().HasForeignKey("UserId")
            );

        // Configuration de la relation User - CreatedResources
        modelBuilder.Entity<User>()
            .HasMany(u => u.CreatedResources)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "UserCreatedResources",
                j => j.HasOne<Resource>().WithMany().HasForeignKey("ResourceId"),
                j => j.HasOne<User>().WithMany().HasForeignKey("UserId")
            );
    }
}