
using Microsoft.EntityFrameworkCore;
using TaskManagerSystem.Core.Entities;

namespace TaskManagerSystem.Data.Contexts
{
    // DbContext'ten miras alarak bu sınıfın sıradan bir sınıf değil, 
    // bir veritabanı bağlamı olduğunu EF Core'a söylüyoruz.
    public class AppDbContext : DbContext
    {
        // Constructor (Yapıcı Metot): API katmanından veritabanı bağlantı adresini 
        // (Connection String) alıp ana DbContext sınıfına (base) iletecek.
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Veritabanında oluşacak Tablolarımız (DbSet'ler)
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectTask> Tasks { get; set; }
        // Sınıfın içine, DbSet'lerin altına bu metodu ekliyoruz:
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Bu satır, Data projesinin içindeki IEntityTypeConfiguration'dan miras alan 
            // tüm konfigürasyon sınıflarını (UserRoleConfiguration vb.) otomatik bulup uygular.
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

            base.OnModelCreating(modelBuilder);
        }
    }
}