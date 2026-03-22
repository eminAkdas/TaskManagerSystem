using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaskManagerSystem.Core.Interfaces;
using TaskManagerSystem.Data.Contexts;

namespace TaskManagerSystem.Data.Repositories
{
    // =============================================================
    // GENERIC REPOSITORY PATTERNİ — N-Tier Mimarinin "Data" Katmanı
    // =============================================================
    // Bu sınıf "Veri Erişim Katmanı (Data Access Layer)"dır.
    // Tek görevi: Veritabanına gidip veri getirmek veya kaydetmek.
    // İş kurallarından (Business Logic) tamamen habersizdir.
    //
    // <T> (Generic Type Parameter):
    //   Bu sınıfı User için de, Task için de, Role için de kullanabiliriz.
    //   "new GenericRepository<User>()" → Users tablosunu yönetir
    //   "new GenericRepository<ProjectTask>()" → Tasks tablosunu yönetir
    //   where T : class → int veya string gibi ilkel tipler gönderilemesin
    //
    // IGenericRepository<T> : Sözleşmeye (interface) bağlıyız.
    //   Bu sayede Service katmanı bu sınıfı değil, ARABIRIMI bilir.
    //   İleride veritabanını değiştirirsek sadece bu dosyayı değiştiririz,
    //   Service katmanına hiç dokunmayız. Buna "Loose Coupling" denir.
    // =============================================================
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        // _context : EF Core'un veritabanına açılan tek kapısı (Unit of Work)
        // _dbSet   : Hangi tablo üzerinde çalışacağımız (örn: context.Users)
        protected readonly AppDbContext _context;
        protected readonly DbSet<T> _dbSet;

        // Dependency Injection ile DbContext geliyor.
        // Program.cs'de "builder.Services.AddDbContext<AppDbContext>(...)" yazdığımızda
        // .NET, bunu ihtiyaç duyulduğunda otomatik oluşturup buraya verir.
        public GenericRepository(AppDbContext context)
        {
            _context = context;
            // context.Set<T>() : "T neyse onun tablosunu ver" demektir.
            // T = User ise → context.Users ile aynı şeydir.
            _dbSet = context.Set<T>();
        }

        // ID ile tek kayıt getir. Bulamazsa null döner (T? → nullable).
        public async Task<T?> GetByIdAsync(Guid id)
        {
            return await _dbSet.FindAsync(id);
        }

        // Tablodaki tüm kayıtları getir. Büyük tablolarda dikkatli kullanılmalı!
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        // Lambda (Func) ile şartlı sorgulama — SQL'deki WHERE karşılığı.
        // YENİ: Artik includes parametresi ile "Task'i çekerken AssignedUser'i de JOIN ile çek" diyebiliyoruz.
        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> expression, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet.Where(expression);

            if (includes != null)
            {
                foreach (var includeProperty in includes)
                {
                    query = query.Include(includeProperty);
                }
            }

            return await query.ToListAsync();
        }

        // Yeni kayıt EF Core'un "takip listesine" eklenir (henüz DB'ye yazılmaz!).
        // SaveChangesAsync çağrılana kadar sadece bellekte tutulur.
        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        // Kaydı "değiştirildi" olarak işaretler. Yine SaveChangesAsync bekler.
        public void Update(T entity)
        {
            _dbSet.Update(entity);
        }

        // Kaydı "silinecek" olarak işaretler. Yine SaveChangesAsync bekler.
        public void Remove(T entity)
        {
            _dbSet.Remove(entity);
        }

        // ===========================================
        // KRİTİK — "Unit of Work" Paterni
        // ===========================================
        // Add, Update, Remove tek başlarına hiçbir şey yazmaz.
        // Hepsi EF Core'un belleğinde bekler.
        // Bu metot çağrıldığında BEKLEYENLERİN TAMAMI tek bir
        // SQL transaction içinde veritabanına işlenir.
        // Başarısız olursa hiçbiri yazılmaz → veri tutarlılığı garantidir.
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}