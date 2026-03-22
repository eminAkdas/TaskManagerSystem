using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManagerSystem.Core.Entities;

namespace TaskManagerSystem.Data.Configurations
{
    public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
    {
        public void Configure(EntityTypeBuilder<UserRole> builder)
        {
            // 1. Aynı kullanıcıya aynı rolün 2 kez verilmesini engelle (Unique Index)
            builder.HasIndex(ur => new { ur.UserId, ur.RoleId }).IsUnique();

            // 2. User ile UserRole arasındaki ilişki (Bir User'ın birden fazla UserRole'ü olur)
            builder.HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Kullanıcı silinirse, sahip olduğu roller de (köprüden) silinsin.

            // 3. Role ile UserRole arasındaki ilişki (Bir Role'ün birden fazla UserRole'ü olur)
            builder.HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Cascade); // Rol silinirse, köprüdeki kayıtlar da silinsin.
        }
    }
}