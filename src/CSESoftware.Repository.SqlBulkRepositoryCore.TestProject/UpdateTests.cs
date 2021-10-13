using CSESoftware.Repository.SqlBulkRepositoryCore.TestDatabase;
using CSESoftware.Repository.SqlBulkRepositoryCore.TestProject.Setup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace CSESoftware.Repository.SqlBulkRepositoryCore.TestProject
{
    public class UpdateTests : BaseTest
    {
        [Fact]
        public async Task UpdateGenderTest()
        {
            const string gender = "Passion Fruit";
            var trees = await StartUpAsync();
            var operationValues = trees.Select(x => new { x.Id, Gender = gender }).ToList();
            
            await Repository.BulkUpdateAsync(new FamilyTree(), operationValues);
            var returnValues = await GetUpdatedValuesAsync(operationValues);

            Assert.NotNull(returnValues);
            Assert.NotEmpty(returnValues);
            foreach (var tree in trees)
            {
                Assert.True(returnValues.ContainsKey(tree.Id));
                Assert.Equal(gender, returnValues[tree.Id].Gender);
            }

            await TearDownAsync(trees);
        }

        [Fact]
        public async Task UpdateIsAliveTest()
        {
            var trees = await StartUpAsync();
            var operationValues = trees.Select(x => new { x.Id, IsAlive = false }).ToList();

            await Repository.BulkUpdateAsync(new FamilyTree(), operationValues);
            var returnValues = await GetUpdatedValuesAsync(operationValues);

            Assert.NotNull(returnValues);
            Assert.NotEmpty(returnValues);
            foreach (var tree in trees)
            {
                Assert.True(returnValues.ContainsKey(tree.Id));
                Assert.False(returnValues[tree.Id].IsAlive);
            }

            await TearDownAsync(trees);
        }

        [Fact]
        public async Task UpdateBirthdateTest()
        {
            var birthdate = DateTime.UtcNow - TimeSpan.FromDays(4721);
            var trees = await StartUpAsync();
            var operationValues = trees.Select(x => new { x.Id, Birthdate = birthdate }).ToList();

            await Repository.BulkUpdateAsync(new FamilyTree(), operationValues);
            var returnValues = await GetUpdatedValuesAsync(operationValues);

            Assert.NotNull(returnValues);
            Assert.NotEmpty(returnValues);
            foreach (var tree in trees)
            {
                Assert.True(returnValues.ContainsKey(tree.Id));
                Assert.Equal(birthdate, returnValues[tree.Id].Birthdate);
            }

            await TearDownAsync(trees);
        }

        [Fact]
        public async Task UpdateFatherIdTest()
        {
            var fatherId = Guid.NewGuid();
            var father = new FamilyTree { Id = fatherId };
            var trees = await StartUpAsync(father);
            var operationValues = trees.Select(x => new { x.Id, FatherId = fatherId }).ToList();

            await Repository.BulkUpdateAsync(new FamilyTree(), operationValues);
            var returnValues = await GetUpdatedValuesAsync(operationValues);

            Assert.NotNull(returnValues);
            Assert.NotEmpty(returnValues);
            foreach (var tree in trees)
            {
                Assert.True(returnValues.ContainsKey(tree.Id));
                Assert.Equal(fatherId, returnValues[tree.Id].FatherId);
            }

            await TearDownAsync(trees);
        }

        private async Task<List<FamilyTree>> StartUpAsync(FamilyTree father = null)
        {
            var trees = DataProvider.GetSimpleTrees(100);
            if (father != null) trees.Add(father);
            await Repository.BulkCreateAsync(trees);

            return trees;
        }

        private async Task<Dictionary<Guid, FamilyTree>> GetUpdatedValuesAsync<TObject>(
            IReadOnlyCollection<TObject> operationValues)
        {
            var returnValues = await Repository.BulkSelectAsync(new FamilyTree(), operationValues);
            return returnValues.ToDictionary(x => x.Id, x => x);
        }
    }
}
