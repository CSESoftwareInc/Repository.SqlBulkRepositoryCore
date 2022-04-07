using CSESoftware.Repository.SqlBulkRepositoryCore.TestDatabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSESoftware.Repository.SqlBulkRepositoryCore.TestProject.Setup
{
    public abstract class BaseTest
    {
        internal readonly BulkRepository<TestContext> Repository;

        protected BaseTest()
        {
            Repository = GetRepository();
        }

        internal async Task TearDownAsync(IEnumerable<FamilyTree> trees,
            IEnumerable<FamilyTreeLink> links = null)
        {
            var linksToDelete = links?.Select(x => new { x.PrimarySiblingId, x.SecondarySiblingId }).ToList();
            var treesToDelete = trees.Select(x => new { x.Id }).ToList();

            if (linksToDelete?.Any() ?? false)
                await Repository.BulkDeleteAsync(new FamilyTreeLink(), linksToDelete);
            if (treesToDelete.Any())
                await Repository.BulkDeleteAsync(new FamilyTree(), treesToDelete);
        }

        internal async Task TearDownAsync(IEnumerable<FamilyHome> homes)
        {
            var homesToDelete = homes.Select(x => new { x.Id }).ToList();
            
            if (homesToDelete.Any())
                await Repository.BulkDeleteAsync(new FamilyHome(), homesToDelete);
        }

        internal static List<FamilyTreeLink> GetDistinctLinks(IReadOnlyCollection<FamilyTree> trees)
        {
            return trees.Where(x => x.Siblings != null).SelectMany(x => x.Siblings)
                .Union(trees.Where(x => x.CounterSiblings != null).SelectMany(x => x.CounterSiblings))
                .GroupBy(x => new { x.PrimarySiblingId, x.SecondarySiblingId })
                .Select(x => x.First()).ToList();
        }

        private static BulkRepository<TestContext> GetRepository()
        {
            var context = new TestContextFactory().CreateDbContext(Array.Empty<string>());
            return new BulkRepository<TestContext>(context);
        }
    }
}
