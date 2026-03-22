using TaskManagerSystem.Core.Entities.Common;
using System.Collections.Generic;

namespace TaskManagerSystem.Core.Entities
{
    public class Role : BaseEntity
    {
        // Rolün adı ("Admin", "Developer", "ProjectManager")
        public string Name { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;

        // Köprü tabloya giden yol
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        
    
    }
}