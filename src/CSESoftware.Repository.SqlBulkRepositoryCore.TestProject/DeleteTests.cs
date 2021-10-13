using CSESoftware.Repository.SqlBulkRepositoryCore.TestDatabase;
using CSESoftware.Repository.SqlBulkRepositoryCore.TestProject.Setup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace CSESoftware.Repository.SqlBulkRepositoryCore.TestProject
{
    public class DeleteTests : BaseTest
    {
        [Fact]
        public async Task DeleteByIdTest()
        {
            var trees = await StartUpAsync();
            var operationValues = trees.Select(x => new { x.Id }).ToList();
            await Repository.BulkDeleteAsync(new FamilyTree(), operationValues);

            var returnValues = await Repository.BulkSelectAsync(new FamilyTree(), operationValues);

            Assert.NotNull(returnValues);
            Assert.Empty(returnValues);
        }

        [Fact]
        public async Task DeleteByGenderTest()
        {
            var trees = await StartUpAsync("Eggplant");
            var operationValues = trees.Select(x => new { x.Gender }).ToList();
            await Repository.BulkDeleteAsync(new FamilyTree(), operationValues);

            var returnValues = await Repository.BulkSelectAsync(new FamilyTree(), operationValues);

            Assert.NotNull(returnValues);
            Assert.Empty(returnValues);
        }

        [Fact]
        public async Task DeleteByIdAndIsAliveTest()
        {
            var trees = await StartUpAsync();
            var operationValues = trees.Select(x => new { x.Id, x.IsAlive }).ToList();
            await Repository.BulkDeleteAsync(new FamilyTree(), operationValues);

            var returnValues = await Repository.BulkSelectAsync(new FamilyTree(), operationValues);

            Assert.NotNull(returnValues);
            Assert.Empty(returnValues);
        }

        [Fact]
        public async Task DeleteByIdAndGenderTest()
        {
            var trees = await StartUpAsync();
            var operationValues = trees.Select(x => new { x.Id, x.Gender }).ToList();
            await Repository.BulkDeleteAsync(new FamilyTree(), operationValues);

            var returnValues = await Repository.BulkSelectAsync(new FamilyTree(), operationValues);

            Assert.NotNull(returnValues);
            Assert.Empty(returnValues);
        }

        private async Task<List<FamilyTree>> StartUpAsync(string gender = "Tomato")
        {
            var isAlive = new Random().Next(0, 2) == 1;
            var trees = DataProvider.GetSimpleTrees(100, gender, isAlive);
            await Repository.BulkCreateAsync(trees);

            return trees;
        }
    }
}
