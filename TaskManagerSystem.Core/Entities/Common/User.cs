using TaskManagerSystem.Core.Entities.Common;

namespace TaskManagerSystem.Core.Entities
{
    public class User : BaseEntity
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        
        // Şifreleri asla düz metin (123456 gibi) tutmayız. 
        // Hash'lenmiş (şifrelenmiş ve geri döndürülemez) halini tutacağız.
        public string PasswordHash { get; set; } = string.Empty;

       // --- NAVIGATION PROPERTIES ---
        
        // Bu kullanıcının üzerine atanmış BİRDEN FAZLA görev olabilir
        public ICollection<ProjectTask> Tasks { get; set; } = new List<ProjectTask>();
        // Bu kullanıcının sahip olduğu roller (Köprü tablo üzerinden)
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();    
    }
}