using CSESoftware.Core.Entity;

namespace CSESoftware.Repository.SqlBulkRepositoryCore.TestDatabase;

public class FamilyHome : BaseEntity<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;

    public virtual ICollection<FamilyTree> Families { get; set; } = [];
}