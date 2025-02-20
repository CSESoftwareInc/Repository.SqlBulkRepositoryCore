using CSESoftware.Core.Entity;

namespace CSESoftware.Repository.SqlBulkRepositoryCore.TestDatabase;

public class FamilyHome : BaseEntity<Guid>
{
    public string? Name { get; set; }
    public string? Address { get; set; }

    public virtual ICollection<FamilyTree> Families { get; set; } = [];
}