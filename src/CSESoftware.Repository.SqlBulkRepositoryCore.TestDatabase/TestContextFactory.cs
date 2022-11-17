using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CSESoftware.Repository.SqlBulkRepositoryCore.TestDatabase
{
    public class TestContextFactory : IDesignTimeDbContextFactory<TestContext>
    {
        private const string ConnectionString =
            "data source=.\\Development; initial catalog=TestDatabase; Trusted_Connection=True; TrustServerCertificate=True;";

        public TestContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<TestContext>();
            optionsBuilder.UseSqlServer(ConnectionString);

            return new TestContext(optionsBuilder.Options);
        }
    }
}
