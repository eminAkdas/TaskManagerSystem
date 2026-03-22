using System;

namespace TaskManagerSystem.Core.DTOs
{
    // Sadece React'ten (Dışarıdan) GÖREV EKLERKEN almak istediğimiz saf veriler.
    // Id, CreatedDate, Status veya Navigation Property'ler YOK!
    public class CreateTaskDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        
        // Görevin hangi projeye ait olduğu zorunlu
        public Guid ProjectId { get; set; }
        
        // Kime atandığı opsiyonel (?)
        public Guid? AssignedUserId { get; set; }
    }
}