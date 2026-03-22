namespace TaskManagerSystem.Core.DTOs
{
    // DTO (Data Transfer Object): Veritabanı entity'sini doğrudan dışarı göndermiyoruz.
    // Bunun yerine, React'in ihtiyaç duyduğu alanları içeren bu "paket" sınıfını gönderiyoruz.
    // Neden? Yarın Project entity'sine hassas bir alan eklenseydi (örn: InternalBudget),
    // bu DTO sayesinde o alanı dışarıya asla göndermemiş olurduk.
    public class ProjectDto
    {
        // React'in dropdown menüsünde "value" olarak kullanacağı benzersiz kimlik
        public Guid Id { get; set; }

        // Dropdown'da kullanıcıya gösterilecek proje adı
        public string Name { get; set; } = string.Empty;

        // Proje detay sayfasında gösterilebilecek açıklama
        public string Description { get; set; } = string.Empty;

        // Projenin başlangıç ve bitiş tarihleri (opsiyonel gösterim için)
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
