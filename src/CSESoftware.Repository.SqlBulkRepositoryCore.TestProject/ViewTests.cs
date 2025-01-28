using CSESoftware.Repository.SqlBulkRepositoryCore.TestDatabase;
using CSESoftware.Repository.SqlBulkRepositoryCore.TestProject.Setup;
using Xunit;

namespace CSESoftware.Repository.SqlBulkRepositoryCore.TestProject;

public class ViewTests : BaseTest
{
    [Theory]
    [MemberData(nameof(NumberOfHomes))]
    public async Task SelectFromViewTests(int numberOfHomes)
    {
        var homes = await CreateHomesForTestAsync(numberOfHomes);
        var trees = await CreateTreesForTestAsync(homes);

        var selectByValues = homes.Select(x => new { x.Name }).ToList();
        var results = await Repository.BulkSelectAsync(new FamilyHomeView(), selectByValues);

        Assert.Equal(homes.Count, results.Count);

        await TearDownAsync(trees);
        await TearDownAsync(homes);
    }

    private async Task<List<FamilyHome>> CreateHomesForTestAsync(int numberOfHomes)
    {
        var homes = DataProvider.GetSimpleHomes(numberOfHomes);
        await Repository.BulkCreateAsync(homes);
        return homes;
    }

    private async Task<List<FamilyTree>> CreateTreesForTestAsync(IEnumerable<FamilyHome> homes)
    {
        var trees = homes.Select(x => x.Id).GetTreesFromHomes();
        await Repository.BulkCreateAsync(trees);
        return trees;
    }

    public static readonly TheoryData<int> NumberOfHomes = [30, 40, 100];
}