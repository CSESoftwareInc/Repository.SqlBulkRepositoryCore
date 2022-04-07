using CSESoftware.Repository.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace CSESoftware.Repository.SqlBulkRepositoryCore.TestDatabase
{
    public class TestContext : BaseDbContext
    {
        public TestContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<FamilyTree> FamilyTrees { get; set; }
        public DbSet<FamilyHome> FamilyHomes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FamilyTree>()
                .HasOne(pp => pp.Father)
                .WithMany(p => p.PaternalChildren)
                .HasForeignKey(pp => pp.FatherId);

            modelBuilder.Entity<FamilyTree>()
                .HasOne(pp => pp.Mother)
                .WithMany(p => p.MaternalChildren)
                .HasForeignKey(pp => pp.MotherId);

            modelBuilder.Entity<FamilyTree>()
                .HasOne(pp => pp.Home)
                .WithMany(p => p.Families)
                .HasForeignKey(pp => pp.HomeId);

            modelBuilder.Entity<FamilyTreeLink>()
                .HasKey(x => new { x.PrimarySiblingId, x.SecondarySiblingId });

            modelBuilder.Entity<FamilyTreeLink>()
                .HasOne(pp => pp.PrimarySibling)
                .WithMany(p => p.CounterSiblings)
                .HasForeignKey(pp => pp.PrimarySiblingId);

            modelBuilder.Entity<FamilyTreeLink>()
                .HasOne(pp => pp.SecondarySibling)
                .WithMany(p => p.Siblings)
                .HasForeignKey(pp => pp.SecondarySiblingId);

            modelBuilder.Entity<FamilyHome>()
                .ToTable("Home_Alpha");
            modelBuilder.Entity<FamilyHome>()
                .Property(x => x.Id).HasColumnName("HomeId");
            modelBuilder.Entity<FamilyHome>()
                .Property(x => x.Name).HasColumnName("Home_Name");
            modelBuilder.Entity<FamilyHome>()
                .Property(x => x.Address).HasColumnName("Home_Address");

            var cascadeFKs = modelBuilder.Model.GetEntityTypes()
                .SelectMany(t => t.GetForeignKeys())
                .Where(fk => !fk.IsOwnership && fk.DeleteBehavior == DeleteBehavior.Cascade || fk.DeleteBehavior == DeleteBehavior.ClientSetNull);

            foreach (var fk in cascadeFKs)
                fk.DeleteBehavior = DeleteBehavior.Restrict;

            base.OnModelCreating(modelBuilder);
        }
    }
}
