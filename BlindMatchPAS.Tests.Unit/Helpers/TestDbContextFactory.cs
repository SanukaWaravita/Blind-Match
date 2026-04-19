using BlindMatchPAS.Data;
using Microsoft.EntityFrameworkCore;

namespace BlindMatchPAS.Tests.Unit.Helpers;

internal static class TestDbContextFactory
{
    public static ApplicationDbContext CreateInMemoryContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        return new ApplicationDbContext(options);
    }
}
