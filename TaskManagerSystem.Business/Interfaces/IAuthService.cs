using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskManagerSystem.Core.DTOs;
using TaskManagerSystem.Core.Entities;

namespace TaskManagerSystem.Business.Interfaces
{
    public interface IAuthService
    {
        // Kayıt işlemi sonunda veritabanına eklenen Kullanıcıyı döneriz
        Task<User> RegisterAsync(RegisterDto registerDto);

        // Giriş işlemi başarılı olursa, o meşhur "Bilekliği" (JWT Token'ı) metin (string) olarak döneceğiz.
        Task<string> LoginAsync(LoginDto loginDto);

        // Bir Admin, başka bir kullanıcıya istediği rolü atayabilir.
        // userId: Rolü atanacak kullanıcının ID'si
        // roleName: Atanacak rolün adı ("ProjectManager", "Admin" vb.)
        Task AssignRoleAsync(Guid userId, string roleName);

        // Bir Admin, kullanıcıdan belirli bir rolü kaldırabilir ("düşürme" işlemi için)
        Task RemoveRoleAsync(Guid userId, string roleName);

        // YENİ: Admin Paneli için tüm kullanıcıları ve rollerini listeleyen metod
        Task<IEnumerable<UserWithRoleDto>> GetAllUsersWithRolesAsync();
    }
}