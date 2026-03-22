using System;
using TaskManagerSystem.Core.Enums;

namespace TaskManagerSystem.Core.DTOs
{
    // API'den DIŞARIYA veri gönderirken kullanacağımız güvenli nesne
    public class TaskDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public TaskManagerSystem.Core.Enums.TaskStatus Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public Guid ProjectId { get; set; }
        public Guid? AssignedUserId { get; set; }
        
        // YENİ: Görev kartında (Frontend) ID veya Email yerine, atanan kişinin
        // doğrudan adını ve soyadını ("Ali Yılmaz") tutacak alan.
        // Veritabanında (ProjectTask) böyle bir sütun YOKTUR, AutoMapper bunu 
        // User tablosuna gidip (Join/Include ile) hesaplayarak dolduracak.
        public string AssignedUserName { get; set; } = string.Empty;
    }
}