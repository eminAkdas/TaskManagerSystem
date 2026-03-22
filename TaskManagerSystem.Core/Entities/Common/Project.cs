using System;
using TaskManagerSystem.Core.Entities.Common;

namespace TaskManagerSystem.Core.Entities
{
    public class Project : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        // Projenin hedeflenen başlangıç ve bitiş tarihleri
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }

       // --- NAVIGATION PROPERTIES ---
        
        // Bir projenin BİRDEN FAZLA görevi olur (One-to-Many)
        // Başlangıçta null hatası almamak için boş bir liste olarak başlatıyoruz (new List).
        public ICollection<ProjectTask> Tasks { get; set; } = new List<ProjectTask>();
    }
}