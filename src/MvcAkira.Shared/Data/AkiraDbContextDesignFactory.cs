using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace MvcAkira.Shared.Data;

public class AkiraDbContextDesignFactory : IDesignTimeDbContextFactory<AkiraDbContext>
{
    public AkiraDbContext CreateDbContext(string[] args)
    {
        var opts = new DbContextOptionsBuilder<AkiraDbContext>()
            .UseSqlite($"Data Source={DbPath.Absolute()}")
            .Options;
        return new AkiraDbContext(opts);
    }
}