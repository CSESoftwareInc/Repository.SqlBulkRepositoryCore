using CSESoftware.Repository.SqlBulkRepositoryCore.TestDatabase;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CSESoftware.Repository.SqlBulkRepositoryCore.TestProject.Setup
{
    internal static class DataProvider
    {
        internal static List<FamilyTree> GetSimpleTrees(int numberOfTrees = 10, string gender = "Tomato",
            bool isAlive = true)
        {
            var trees = new List<FamilyTree>();
            for (var i = 0; i < numberOfTrees; i++)
                trees.Add(GetTree(DateTime.UtcNow - TimeSpan.FromDays(i), gender, isAlive));

            return trees;
        }

        internal static List<FamilyTree> GetComplexTrees(int numberOfTrees = 10)
        {
            var father = GetTree(DateTime.UtcNow - TimeSpan.FromDays(365), "Strawberry");
            var mother = GetTree(DateTime.UtcNow - TimeSpan.FromDays(364), "Banana");

            var trees = new List<FamilyTree>();
            for (var i = 0; i < numberOfTrees; i++)
                trees.Add(GetTree(DateTime.UtcNow - TimeSpan.FromDays(i), "Kiwi"));

            foreach (var tree in trees.Select(tree => ManageSiblings(trees, tree)))
            {
                tree.Father = father;
                tree.FatherId = father.Id;
                tree.Mother = mother;
                tree.MotherId = mother.Id;
            }

            father.PaternalChildren = trees;
            mother.MaternalChildren = trees;

            trees.Add(father);
            trees.Add(mother);

            return trees;
        }

        internal static List<FamilyHome> GetSimpleHomes(int numberOfHomes = 10, string name = "Our House",
            string address = "555 Quantum Place")
        {
            var homes = new List<FamilyHome>();
            for (var i = 0; i < numberOfHomes; i++)
                homes.Add(GetHome($"{name} {i}", $"{address} Apt #{i}"));

            return homes;
        }

        private static FamilyTree GetTree(DateTime birthdate, string gender = "Tomato", bool isAlive = true)
        {
            return new FamilyTree
            {
                Id = Guid.NewGuid(),
                IsActive = true,
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now,
                IsAlive = isAlive,
                Gender = gender,
                Birthdate = birthdate,
                FatherId = Guid.Empty,
                Father = null,
                MotherId = Guid.Empty,
                Mother = null,
                Siblings = null,
                CounterSiblings = null,
                PaternalChildren = null,
                MaternalChildren = null
            };
        }

        private static FamilyTree ManageSiblings(IReadOnlyCollection<FamilyTree> trees, FamilyTree tree)
        {
            var excludedTrees = new List<FamilyTree> { tree };
            tree.Siblings = trees.Except(excludedTrees).Select(x => new FamilyTreeLink
            {
                PrimarySiblingId = tree.Id,
                PrimarySibling = tree,
                SecondarySiblingId = x.Id,
                SecondarySibling = x
            }).ToList();

            tree.CounterSiblings = trees.Except(excludedTrees).Select(x => new FamilyTreeLink
            {
                PrimarySiblingId = x.Id,
                PrimarySibling = x,
                SecondarySiblingId = tree.Id,
                SecondarySibling = tree
            }).ToList();

            return tree;
        }

        private static FamilyHome GetHome(string name, string address)
        {
            return new FamilyHome
            {
                Id = Guid.NewGuid(),
                IsActive = true,
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now,
                Name = name,
                Address = address,
                Families = new List<FamilyTree>()
            };
        }
    }
}
