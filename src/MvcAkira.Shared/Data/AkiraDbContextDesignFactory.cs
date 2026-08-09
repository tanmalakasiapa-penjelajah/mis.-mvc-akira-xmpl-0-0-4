using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace MvcAkira.Shared.Data;

public class AkiraDbContextDesignFactory : IDesignTimeDbContextFactory<AkiraDbContext>
{
    public AkiraDbContext CreateDbContext(string[] args)
    {
        var opts = new DbContextOptionsBuilder<AkiraDbContext>()
            .UseSqlite(new SqliteConnectionStringBuilder { DataSource = DbPath.Absolute() }.ToString())
            .Options;
        return new AkiraDbContext(opts);
    }
}