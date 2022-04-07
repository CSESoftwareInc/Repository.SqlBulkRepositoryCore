using CSESoftware.Repository.Builder;
using CSESoftware.Repository.SqlBulkRepositoryCore.TestDatabase;
using CSESoftware.Repository.SqlBulkRepositoryCore.TestProject.Setup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace CSESoftware.Repository.SqlBulkRepositoryCore.TestProject
{
    public class SelectTests : BaseTest
    {
        [Fact]
        public async Task SelectByIdTest()
        {
            var trees = await StartUpAsync();
            var selectValues = trees.Select(x => new { x.Id }).ToList();
            var returnValues = await Repository.BulkSelectAsync(new FamilyTree(), selectValues);

            Assert.NotNull(returnValues);
            Assert.NotEmpty(returnValues);
            foreach (var value in returnValues)
                Assert.Contains(value.Id, trees.Select(x => x.Id));

            await TearDownAsync(trees);
        }

        [Fact]
        public async Task SelectByGenderTest()
        {
            var trees = await StartUpAsync(gender: "Jackdaw");
            var selectValues = trees.Select(x => new { x.Gender }).ToList();
            var returnValues = await Repository.BulkSelectAsync(new FamilyTree(), selectValues);

            Assert.NotNull(returnValues);
            Assert.NotEmpty(returnValues);
            foreach (var value in returnValues)
                Assert.Contains(value.Id, trees.Select(x => x.Id));

            await TearDownAsync(trees);
        }

        [Fact]
        public async Task SelectByIdAndIsAliveTest()
        {
            var trees = await StartUpAsync(isAlive: false);
            var selectValues = trees.Select(x => new { x.Id, x.IsAlive }).ToList();
            var returnValues = await Repository.BulkSelectAsync(new FamilyTree(), selectValues);

            Assert.NotNull(returnValues);
            Assert.NotEmpty(returnValues);
            foreach (var value in returnValues)
                Assert.Contains(value.Id, trees.Select(x => x.Id));

            await TearDownAsync(trees);
        }

        [Fact]
        public async Task SelectByIdAndGenderTest()
        {
            var trees = await StartUpAsync();
            var selectValues = trees.Select(x => new { x.Id, x.Gender }).ToList();
            var returnValues = await Repository.BulkSelectAsync(new FamilyTree(), selectValues);

            Assert.NotNull(returnValues);
            Assert.NotEmpty(returnValues);
            foreach (var value in returnValues)
                Assert.Contains(value.Id, trees.Select(x => x.Id));

            await TearDownAsync(trees);
        }

        [Fact]
        public async Task SelectWithQueryTest()
        {
            var (trees, links) = await ComplexStartUpAsync();
            var query = GetTreeQuery();
            var selectValues = trees.Where(x => x.FatherId != Guid.Empty && x.Siblings.Any())
                .Select(x => new { x.Id }).ToList();
            var returnValues = await Repository.BulkSelectAsync(new FamilyTree(), selectValues, query);

            Assert.NotNull(returnValues);
            Assert.NotEmpty(returnValues);

            await TearDownAsync(trees, links);
        }

        [Fact]
        public async Task SelectMismatchedColumnTest()
        {
            var homes = await HomeStartUpAsync();
            var selectValues = homes.Select(x => new { x.Id, x.Name }).ToList();
            var returnValues = await Repository.BulkSelectAsync(new FamilyHome(), selectValues);

            Assert.NotNull(returnValues);
            Assert.NotEmpty(returnValues);
            foreach (var value in returnValues)
                Assert.Contains(value.Id, homes.Select(x => x.Id));

            await TearDownAsync(homes);
        }

        [Fact]
        public async Task SelectMismatchedColumnTest_WithIdColumnName()
        {
            var homes = await HomeStartUpAsync();
            var selectValues = homes.Select(x => new { HomeId = x.Id, x.Address }).ToList();
            var returnValues = await Repository.BulkSelectAsync(new FamilyHome(), selectValues);

            Assert.NotNull(returnValues);
            Assert.NotEmpty(returnValues);
            foreach (var value in returnValues)
                Assert.Contains(value.Id, homes.Select(x => x.Id));

            await TearDownAsync(homes);
        }

        [Fact]
        public async Task SelectMismatchedColumnTest_WithSelectColumnName()
        {
            var homes = await HomeStartUpAsync();
            var selectValues = homes.Select(x => new { x.Id, Home_Name = x.Name }).ToList();
            var returnValues = await Repository.BulkSelectAsync(new FamilyHome(), selectValues);

            Assert.NotNull(returnValues);
            Assert.NotEmpty(returnValues);
            foreach (var value in returnValues)
                Assert.Contains(value.Id, homes.Select(x => x.Id));

            await TearDownAsync(homes);
        }

        private async Task<List<FamilyTree>> StartUpAsync(int numberOfTrees = 100,
            string gender = "Tomato", bool isAlive = true)
        {
            var trees = DataProvider.GetSimpleTrees(numberOfTrees, gender, isAlive);
            await Repository.BulkCreateAsync(trees);

            return trees;
        }

        private async Task<(List<FamilyTree>, List<FamilyTreeLink>)> ComplexStartUpAsync()
        {
            var trees = DataProvider.GetComplexTrees();
            var links = GetDistinctLinks(trees);

            await Repository.BulkCreateAsync(trees);
            await Repository.BulkCreateAsync(links);

            return (trees, links);
        }

        private async Task<List<FamilyHome>> HomeStartUpAsync()
        {
            var homes = DataProvider.GetSimpleHomes(100);
            await Repository.BulkCreateAsync(homes);

            return homes;
        }

        private static IQuery<FamilyTree> GetTreeQuery()
        {
            return new QueryBuilder<FamilyTree>()
                .Include(x => x.Father)
                .Include(x => x.Siblings.Select(y => y.PrimarySibling))
                .Build();
        }
    }
}
