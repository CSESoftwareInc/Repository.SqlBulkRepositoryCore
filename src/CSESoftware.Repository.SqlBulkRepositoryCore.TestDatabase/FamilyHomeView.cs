using CSESoftware.Core.Entity;

namespace CSESoftware.Repository.SqlBulkRepositoryCore.TestDatabase;

public class FamilyHomeView : Entity
{
    public string? Gender { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
}