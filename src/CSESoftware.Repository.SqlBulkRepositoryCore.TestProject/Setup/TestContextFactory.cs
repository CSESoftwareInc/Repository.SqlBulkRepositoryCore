using CSESoftware.Repository.SqlBulkRepositoryCore.TestDatabase;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CSESoftware.Repository.SqlBulkRepositoryCore.TestProject.Setup
{
    public class TestContextFactory : IDesignTimeDbContextFactory<TestContext>
    {
        public TestContext CreateDbContext(string[] args)
        {
            var connectionString = new Environment().GetConnectionString();
            var optionsBuilder = new DbContextOptionsBuilder<TestContext>();
            optionsBuilder.UseSqlServer(connectionString);
            return new TestContext(optionsBuilder.Options);
        }
    }
}
