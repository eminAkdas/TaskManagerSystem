using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskManagerSystem.Core.Entities;
using TaskManagerSystem.Core.Enums;

namespace TaskManagerSystem.Business.Interfaces
{
    public interface ITaskService
    {
        // Yeni bir görev oluştur
        Task<ProjectTask> CreateTaskAsync(ProjectTask task);
        
        // Bir görevin durumunu (Örn: ToDo -> Done) güncelle
        Task UpdateTaskStatusAsync(Guid taskId, Core.Enums.TaskStatus newStatus);
        
        // Projeye ait tüm görevleri getir
        Task<IEnumerable<ProjectTask>> GetTasksByProjectIdAsync(Guid projectId);

        // Bir görevi sil
        Task DeleteTaskAsync(Guid taskId);
    }
}