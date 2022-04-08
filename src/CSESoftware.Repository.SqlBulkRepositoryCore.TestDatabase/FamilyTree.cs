using CSESoftware.Core.Entity;
using System;
using System.Collections.Generic;

namespace CSESoftware.Repository.SqlBulkRepositoryCore.TestDatabase
{
    public class FamilyTree : BaseEntity<Guid>
    {
        public bool IsAlive { get; set; }
        public string Gender { get; set; }
        public DateTime Birthdate { get; set; }

        public Guid FatherId { get; set; }
        public virtual FamilyTree Father { get; set; }

        public Guid MotherId { get; set; }
        public virtual FamilyTree Mother { get; set; }

        public Guid HomeId { get; set; }
        public virtual FamilyHome Home { get; set; }

        public virtual ICollection<FamilyTreeLink> Siblings { get; set; }
        public virtual ICollection<FamilyTreeLink> CounterSiblings { get; set; }

        public virtual ICollection<FamilyTree> PaternalChildren { get; set; }
        public virtual ICollection<FamilyTree> MaternalChildren { get; set; }
    }
}
