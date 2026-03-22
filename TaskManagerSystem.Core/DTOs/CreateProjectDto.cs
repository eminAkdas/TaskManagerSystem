namespace TaskManagerSystem.Core.DTOs
{
    // React'teki "Yeni Proje" formundan gelen veriyi taşıyan paket.
    // Sadece kullanıcının dolduracağı alanlar burada — Id, CreatedDate gibi
    // sistem tarafından üretilen alanlar burada OLMAMALI.
    public class CreateProjectDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        // Projenin başlangıç tarihi zorunlu
        public DateTime StartDate { get; set; }

        // Bitiş tarihi opsiyonel — proje açık uçlu olabilir
        public DateTime? EndDate { get; set; }
    }
}
