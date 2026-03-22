using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace TaskManagerSystem.Core.Interfaces
{
    // <T> ifadesi: "Bana User, Project veya Task ver, ben ona göre çalışırım" demektir.
    // where T : class ifadesi: "Bana sadece bir sınıf (Entity) verebilirsin, int veya string veremezsin" kısıtlamasıdır.
    public interface IGenericRepository<T> where T : class
    {
        // Tek bir kaydı ID'ye göre getir
        Task<T?> GetByIdAsync(Guid id);

        // Tablodaki tüm kayıtları getirir.
        Task<IEnumerable<T>> GetAllAsync();

        // Belirli bir şarta göre filtreleme (WHERE) yapar.
        // YENİ: İlişkili verileri (Include) çekebilmek için 'includes' parametresi eklendi.
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> expression, params Expression<Func<T, object>>[] includes);
        
        // Yeni kayıt ekler.
        Task AddAsync(T entity);

        // Kaydı güncelle
        void Update(T entity);

        // Kaydı sil
        void Remove(T entity);
        
        Task<int> SaveChangesAsync();
    }
}