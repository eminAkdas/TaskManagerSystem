using System;
using TaskManagerSystem.Core.Entities.Common;
using TaskManagerSystem.Core.Enums;

namespace TaskManagerSystem.Core.Entities
{
    public class ProjectTask : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        // Az önce oluşturduğumuz Enum'ı burada kullanıyoruz.
        // Varsayılan olarak her yeni görev "ToDo" (Yapılacak) olarak başlar.
        public TaskManagerSystem.Core.Enums.TaskStatus Status { get; set; } = TaskManagerSystem.Core.Enums.TaskStatus.ToDo;

        // --- İLİŞKİLER (FOREIGN KEYS - Yabancı Anahtarlar) ---
        
        // 1. Bu görev HANGİ projeye ait? (Her görevin zorunlu bir projesi olmalı)
        public Guid ProjectId { get; set; }

        // 2. Bu görev KİME atandı? 
        // Soru işareti (?) koyduk çünkü bir görev oluşturulduğunda henüz 
        // birine atanmamış (boş/null) olabilir.
        public Guid? AssignedUserId { get; set; }
        // --- NAVIGATION PROPERTIES (GEZİNME ÖZELLİKLERİ) ---
        
        // Bu görevin bağlı olduğu gerçek Proje nesnesi
        public Project Project { get; set; } = null!;

        // Bu görevin atandığı gerçek Kullanıcı nesnesi (Atanmamış olabilir, o yüzden nullable '?')
        public User? AssignedUser { get; set; }
    }
}