using TaskManagerSystem.Core.Enums;

namespace TaskManagerSystem.Core.DTOs
{
    // Sadece durumu güncellemek için dışarıdan alacağımız güvenli paket
    public class UpdateTaskStatusDto
    {
        // Gelen yeni durum (1: ToDo, 2: InProgress, 3: Done)
        public TaskManagerSystem.Core.Enums.TaskStatus NewStatus { get; set; }
    }
}