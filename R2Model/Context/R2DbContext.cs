using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using R2Model.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


namespace R2Model.Context
{
    public class R2DbContext : IdentityDbContext<User>
    {
        public R2DbContext(DbContextOptions<R2DbContext> options) : base(options)
        {
        }
        public DbSet<UserRight> UserRights { get; set; } 
        public DbSet<Statistic> Statistics { get; set; } 
        public DbSet<Ressource> Ressources { get; set; } 
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Role> Roles { get; set; } 
        public DbSet<Category> Categories { get; set; }
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<RessourceType> RessourcesType { get; set; }
        public DbSet<RessourceStatus> RessourcesStatut { get; set; } 
        public DbSet<ProgressionStatus> ProgressionsStatus { get; set; }
        public DbSet<Progression> Progressions { get; set; }




    }
}
