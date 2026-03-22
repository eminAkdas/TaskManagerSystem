using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using TaskManagerSystem.Business.Interfaces;
using TaskManagerSystem.Core.DTOs;
using TaskManagerSystem.Core.Entities;
using TaskManagerSystem.Core.Interfaces;

namespace TaskManagerSystem.Business.Services
{
    public class ProjectService : IProjectService
    {
        private readonly IGenericRepository<Project> _projectRepository;

        // YENİ: AutoMapper'ı da enjekte ediyoruz.
        // DTO → Entity ve Entity → DTO dönüşümleri için gerekli.
        private readonly IMapper _mapper;

        public ProjectService(IGenericRepository<Project> projectRepository, IMapper mapper)
        {
            _projectRepository = projectRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<Project>> GetAllProjectsAsync()
        {
            return await _projectRepository.GetAllAsync();
        }

        public async Task<ProjectDto> CreateProjectAsync(CreateProjectDto createProjectDto)
        {
            // İş Kuralı 1: DTO → Entity'ye çevir (AutoMapper halleder)
            // Kullanıcının formdaki Name, Description, StartDate değerleri kopyalanır.
            var newProject = _mapper.Map<Project>(createProjectDto);

            // İş Kuralı 2: Sistem tarafından üretilen alanları BİZ dolduruyoruz.
            // Controller veya React bu değerleri belirleyemez — güvenlik ve tutarlılık için.
            newProject.Id = Guid.NewGuid();          // Benzersiz kimlik
            newProject.CreatedDate = DateTime.UtcNow; // Sunucu saati (istemciye güvenme)

            // Veritabanına ekle ve kaydet
            await _projectRepository.AddAsync(newProject);
            await _projectRepository.SaveChangesAsync();

            // Geri dönen değeri de DTO'ya çevir:
            // Yeni projenin ID'si ve tüm bilgileri React'e gönderilir.
            // Bu sayede React sol menüye yeni projeyi sayfayı yenilemeden ekleyebilir.
            return _mapper.Map<ProjectDto>(newProject);
        }

        public async Task DeleteProjectAsync(Guid projectId)
        {
            // İş Kuralı 1: Silinecek proje gerçekten var mı?
            // Hayali bir ID gelirse veritabanını gereksiz yere zorlamayız.
            var project = await _projectRepository.GetByIdAsync(projectId);
            if (project == null)
                throw new Exception($"Proje bulunamadı (ID: {projectId})");

            // İş Kuralı 2: Projeyi sil.
            // Veritabanımızdaki UserRoleConfiguration'da buna benzer,
            // ProjectTask entity'sinin ProjectId foreign key'ine CASCADE DELETE ayarlandı.
            // Yani bu projeye ait tüm görevler de otomatik silinir — biz ayrıca uğraşmayız.
            _projectRepository.Remove(project);
            await _projectRepository.SaveChangesAsync();
        }
    }
}

