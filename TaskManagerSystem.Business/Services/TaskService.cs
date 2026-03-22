using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskManagerSystem.Business.Interfaces;
using TaskManagerSystem.Core.Entities;
using TaskManagerSystem.Core.Interfaces;

namespace TaskManagerSystem.Business.Services
{
    public class TaskService : ITaskService
    {
        // Data katmanındaki Garsonumuzu (Repository) içeri alıyoruz
        private readonly IGenericRepository<ProjectTask> _taskRepository;

        // Dependency Injection ile Garsonu Müdürün emrine veriyoruz
        public TaskService(IGenericRepository<ProjectTask> taskRepository)
        {
            _taskRepository = taskRepository;
        }

        public async Task<ProjectTask> CreateTaskAsync(ProjectTask task)
        {
            // İş Kuralı 1: Yeni oluşturulan bir görev her zaman "ToDo" (Yapılacak) statüsünde başlamalıdır.
            task.Status = Core.Enums.TaskStatus.ToDo;
            task.CreatedDate = DateTime.UtcNow;

            await _taskRepository.AddAsync(task);
            await _taskRepository.SaveChangesAsync(); // Veritabanına yaz!

            return task;
        }

        public async Task UpdateTaskStatusAsync(Guid taskId, Core.Enums.TaskStatus newStatus)
        {
            // Önce görevi veritabanından bul
            var task = await _taskRepository.GetByIdAsync(taskId);
            
            // İş Kuralı 2: Eğer görev yoksa hata fırlat!
            if (task == null)
            {
                throw new Exception("Görev bulunamadı!");
            }

            // Görevin durumunu ve güncellenme tarihini değiştir
            task.Status = newStatus;
            task.UpdatedDate = DateTime.UtcNow;

            _taskRepository.Update(task);
            await _taskRepository.SaveChangesAsync(); // Veritabanına yaz!
        }

        public async Task<IEnumerable<ProjectTask>> GetTasksByProjectIdAsync(Guid projectId)
        {
            // YENİ: Görevleri çekerken, onlara atanmış olan Kullanıcıları (User nesnesini) da JOIN ile çekiyoruz.
            // Bu sayede AutoMapper, TaskDto'daki AssignedUserName alanını doldurabilecek.
            return await _taskRepository.FindAsync(
                x => x.ProjectId == projectId, 
                x => x.AssignedUser // Include edilecek property (ProjectTask entity'sindeki adı)
            );
        }

        public async Task DeleteTaskAsync(Guid taskId)
        {
            // Önce görevi veritabanından bul
            var task = await _taskRepository.GetByIdAsync(taskId);
            
            // Eğer görev yoksa hata fırlat
            if (task == null)
            {
                throw new Exception("Silinecek görev bulunamadı!");
            }

            // Görevi sil ve veritabanına kaydet
            _taskRepository.Remove(task);
            await _taskRepository.SaveChangesAsync();
        }
    }
}