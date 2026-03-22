using System;
using System.Collections.Generic;

namespace TaskManagerSystem.Core.DTOs
{
    // Admin Paneline gidecek "Rolleri ile birlikte Kullanıcı" paketi
    public class UserWithRoleDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        
        // Bir kullanıcının birden fazla rolü olabilir, bu yüzden liste tutuyoruz.
        // Genelde tek rolü olur ama altyapımız çoklu role izin veriyor.
        public List<string> Roles { get; set; } = new List<string>();
    }
}
