using System;
using TaskManagerSystem.Core.Entities.Common;

namespace TaskManagerSystem.Core.Entities
{
    public class UserRole : BaseEntity
    {
        // 1. Kullanıcı ID'si ve Nesnesi
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        // 2. Rol ID'si ve Nesnesi
        public Guid RoleId { get; set; }
        public Role Role { get; set; } = null!;
    }
}