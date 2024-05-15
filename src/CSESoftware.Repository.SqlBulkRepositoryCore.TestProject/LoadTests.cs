using CSESoftware.Repository.SqlBulkRepositoryCore.TestDatabase;
using CSESoftware.Repository.SqlBulkRepositoryCore.TestProject.Setup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace CSESoftware.Repository.SqlBulkRepositoryCore.TestProject
{
    public class LoadTests : BaseTest
    {
        private const string Gender = "Potato";

        [Theory]
        [InlineData(20000)]
        [InlineData(22000)]
        [InlineData(24000)]
        [InlineData(26000)]
        [InlineData(28000)]
        [InlineData(30000)]
        public async Task SelectLoadTest(int load)
        {
            var trees = await PerformCreateAsync(load);
            await PerformUpdateAsync(trees);
            var selectValues = await PerformSelectAsync(trees);
            await PerformDeleteAsync(trees);

            Assert.NotNull(selectValues);
            Assert.NotEmpty(selectValues);
            Assert.Equal(trees.Count, selectValues.Count);
            foreach (var tree in trees)
                Assert.True(selectValues.ContainsKey(tree.Id));
        }

        private async Task<List<FamilyTree>> PerformCreateAsync(int numberOfRecords)
        {
            var trees = DataProvider.GetSimpleTrees(numberOfRecords);
            await Repository.BulkCreateAsync(trees);

            return trees;
        }

        private async Task PerformUpdateAsync(IEnumerable<FamilyTree> trees)
        {
            var updateValues = trees.Select(x => new { x.Id, Gender }).ToList();
            await Repository.BulkUpdateAsync(new FamilyTree(), updateValues);
        }

        private async Task<Dictionary<Guid, FamilyTree>> PerformSelectAsync(IEnumerable<FamilyTree> trees)
        {
            var selectValues = trees.Select(x => new { x.Id, Gender, x.IsAlive }).ToList();
            var returnValues = await Repository.BulkSelectAsync(new FamilyTree(), selectValues);
            return returnValues.ToDictionary(x => x.Id, x => x);
        }

        private async Task PerformDeleteAsync(IEnumerable<FamilyTree> trees)
        {
            var deleteValues = trees.Select(x => new { x.Id, Gender, x.IsAlive }).ToList();
            await Repository.BulkDeleteAsync(new FamilyTree(), deleteValues);
        }
    }
}
