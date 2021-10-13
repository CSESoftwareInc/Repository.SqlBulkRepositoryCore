using CSESoftware.Repository.SqlBulkRepositoryCore.TestDatabase;
using CSESoftware.Repository.SqlBulkRepositoryCore.TestProject.Setup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace CSESoftware.Repository.SqlBulkRepositoryCore.TestProject
{
    public class CreateTests : BaseTest
    {
        [Theory]
        [MemberData(nameof(NumberOfRecords))]
        public async Task BasicCreateTests(int numberOfTrees)
        {
            var trees = DataProvider.GetSimpleTrees(numberOfTrees);
            await Repository.BulkCreateAsync(trees);
            var treeSelectValues = trees.Select(x => new { x.Id }).ToList();
            var returnTrees = trees.Any()
                ? await Repository.BulkSelectAsync(new FamilyTree(), treeSelectValues)
                : new List<FamilyTree>();

            var treeKeys = trees.ToDictionary(x => x.Id, x => x);
            CompareOriginalToCreated(treeKeys, returnTrees);
            await TearDownAsync(trees);
        }

        [Theory]
        [MemberData(nameof(NumberOfRecords))]
        public async Task ComplexCreateTests(int numberOfTrees)
        {
            var trees = DataProvider.GetComplexTrees(numberOfTrees);
            var distinctLinks = GetDistinctLinks(trees);

            await Repository.BulkCreateAsync(trees);
            await Repository.BulkCreateAsync(distinctLinks);

            var treeSelectValues = trees.Select(x => new { x.Id }).ToList();
            var linkSelectValues = distinctLinks.Select(x => new { x.PrimarySiblingId, x.SecondarySiblingId }).ToList();
            var returnTrees = await Repository.BulkSelectAsync(new FamilyTree(), treeSelectValues);
            var returnLinks = await Repository.BulkSelectAsync(new FamilyTreeLink(), linkSelectValues);

            var treeKeys = trees.ToDictionary(x => x.Id, x => x);
            CompareOriginalToCreated(treeKeys, returnTrees);
            Assert.Equal(distinctLinks.Count, returnLinks.Count);
            await TearDownAsync(trees, distinctLinks);
        }

        [Theory]
        [MemberData(nameof(NumberOfRecords))]
        public async Task BasicCreateAndReturnTests(int numberOfTrees)
        {
            var trees = DataProvider.GetSimpleTrees(numberOfTrees);
            var createdTrees = await Repository.BulkCreateAndReturnAsync<FamilyTree, Guid>(trees);

            var treeKeys = trees.ToDictionary(x => x.Id, x => x);
            CompareOriginalToCreated(treeKeys, createdTrees.Where(x => treeKeys.ContainsKey(x.Id)).ToList());
            await TearDownAsync(trees);
        }

        [Theory]
        [MemberData(nameof(NumberOfRecords))]
        public async Task ComplexCreateAndReturnTests(int numberOfTrees)
        {
            var trees = DataProvider.GetComplexTrees(numberOfTrees);
            var distinctLinks = GetDistinctLinks(trees);

            var createdTrees = await Repository.BulkCreateAndReturnAsync<FamilyTree, Guid>(trees);
            await Repository.BulkCreateAsync(distinctLinks);

            var linkSelectValues = distinctLinks.Select(x => new { x.PrimarySiblingId, x.SecondarySiblingId }).ToList();
            var returnLinks = await Repository.BulkSelectAsync(new FamilyTreeLink(), linkSelectValues);

            var treeKeys = trees.ToDictionary(x => x.Id, x => x);
            CompareOriginalToCreated(treeKeys, createdTrees.Where(x => treeKeys.ContainsKey(x.Id)).ToList());
            Assert.Equal(distinctLinks.Count, returnLinks.Count);
            await TearDownAsync(trees, distinctLinks);
        }

        private static void CompareOriginalToCreated(IReadOnlyDictionary<Guid, FamilyTree> original,
            List<FamilyTree> created)
        {
            Assert.Equal(original.Count, created.Count);
            foreach (var tree in created)
            {
                Assert.True(original.ContainsKey(tree.Id));
                var matchingTree = original[tree.Id];

                Assert.Equal(tree.Gender, matchingTree.Gender);
                Assert.Equal(tree.IsAlive, matchingTree.IsAlive);
                Assert.Equal(tree.FatherId, matchingTree.FatherId);
                Assert.Equal(tree.MotherId, matchingTree.MotherId);
            }
        }
        
        public static IEnumerable<object[]> NumberOfRecords =>
            new List<object[]>
            {
                new object[] { 0 },
                new object[] { 10 },
                new object[] { 100 }
            };
    }
}
