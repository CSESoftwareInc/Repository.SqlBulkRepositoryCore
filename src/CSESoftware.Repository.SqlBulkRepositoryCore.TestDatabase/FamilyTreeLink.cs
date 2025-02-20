using CSESoftware.Core.Entity;

namespace CSESoftware.Repository.SqlBulkRepositoryCore.TestDatabase;

public class FamilyTreeLink : Entity
{
    public Guid PrimarySiblingId { get; set; }
    public virtual FamilyTree? PrimarySibling { get; set; }

    public Guid SecondarySiblingId { get; set; }
    public virtual FamilyTree? SecondarySibling { get; set; }
}