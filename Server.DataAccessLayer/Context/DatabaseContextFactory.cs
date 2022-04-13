using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Server.DataAccessLayer.Context;

public class DatabaseContextFactory : IDesignTimeDbContextFactory<DatabaseContext>
{
    public DatabaseContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=sc;Username=postgres;Password=123;");
        return new DatabaseContext(optionsBuilder.Options);
    }
}