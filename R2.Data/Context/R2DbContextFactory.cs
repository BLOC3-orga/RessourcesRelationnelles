using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace R2.Data.Context;

public class R2DbContextFactory : IDesignTimeDbContextFactory<R2DbContext>
{
    public R2DbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<R2DbContext>();
        optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=R2Database;Trusted_Connection=True;MultipleActiveResultSets=true");

        return new R2DbContext(optionsBuilder.Options);
    }
}