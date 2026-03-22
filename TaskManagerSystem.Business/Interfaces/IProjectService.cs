using System.Collections.Generic;
using System.Threading.Tasks;
using TaskManagerSystem.Core.DTOs;
using TaskManagerSystem.Core.Entities;

namespace TaskManagerSystem.Business.Interfaces
{
    public interface IProjectService
    {
        // Tüm projeleri listele
        Task<IEnumerable<Project>> GetAllProjectsAsync();

        // Yeni proje oluştur ve geri döndür
        // CreateProjectDto: Kullanıcının form verisi
        // Dönen ProjectDto: Oluşturulan projenin geri gönderilecek hali (Id dahil)
        Task<ProjectDto> CreateProjectAsync(CreateProjectDto createProjectDto);

        // Projeyi ve bağlı tüm görevleri sil (CASCADE)
        Task DeleteProjectAsync(Guid projectId);
    }
}
