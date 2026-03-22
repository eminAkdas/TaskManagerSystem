using System;

namespace TaskManagerSystem.Core.Entities.Common
{
    public abstract class BaseEntity
    {
        // Tüm varlıkların ortak benzersiz kimliği (Primary Key olacak)
        public Guid Id { get; set; }

        // Kaydın veritabanına ilk eklendiği tarih
        public DateTime CreatedDate { get; set; }

        // Kaydın üzerinde en son değişiklik yapıldığı tarih
        public DateTime? UpdatedDate { get; set; }
    }
}