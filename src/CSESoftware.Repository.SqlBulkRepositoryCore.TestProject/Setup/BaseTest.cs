using CSESoftware.Repository.SqlBulkRepositoryCore.TestDatabase;

namespace CSESoftware.Repository.SqlBulkRepositoryCore.TestProject.Setup;

public abstract class BaseTest
{
    internal readonly BulkRepository<TestContext> Repository = GetRepository();

    internal async Task TearDownAsync(IEnumerable<FamilyTree> trees,
        IEnumerable<FamilyTreeLink>? links = null)
    {
        var linksToDelete = links?.Select(x => new { x.PrimarySiblingId, x.SecondarySiblingId }).ToList() ?? [];
        var treesToDelete = trees.Select(x => new { x.Id }).ToList();

        if (linksToDelete.Count != 0)
            await Repository.BulkDeleteAsync(new FamilyTreeLink(), linksToDelete);
        if (treesToDelete.Count != 0)
            await Repository.BulkDeleteAsync(new FamilyTree(), treesToDelete);
    }

    internal async Task TearDownAsync(IEnumerable<FamilyHome> homes)
    {
        var homesToDelete = homes.Select(x => new { x.Id }).ToList();
            
        if (homesToDelete.Count != 0)
            await Repository.BulkDeleteAsync(new FamilyHome(), homesToDelete);
    }

    internal static List<FamilyTreeLink> GetDistinctLinks(IReadOnlyCollection<FamilyTree> trees)
    {
        return trees.Where(x => x.Siblings.Count > 0).SelectMany(x => x.Siblings)
            .Union(trees.Where(x => x.CounterSiblings.Count > 0).SelectMany(x => x.CounterSiblings))
            .GroupBy(x => new { x.PrimarySiblingId, x.SecondarySiblingId })
            .Select(x => x.First()).ToList();
    }

    private static BulkRepository<TestContext> GetRepository()
    {
        var context = new TestContextFactory().CreateDbContext([]);
        return new BulkRepository<TestContext>(context);
    }
}