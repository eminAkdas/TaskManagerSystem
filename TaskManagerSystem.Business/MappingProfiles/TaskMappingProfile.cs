using AutoMapper;
using TaskManagerSystem.Core.DTOs;
using TaskManagerSystem.Core.Entities;

namespace TaskManagerSystem.Business.MappingProfiles
{
    // Profile sınıfından miras alarak bunun bir AutoMapper ayar dosyası olduğunu belirtiyoruz.
    public class TaskMappingProfile : Profile
    {
        public TaskMappingProfile()
        {
            // CreateMap<Nereden, Nereye>();
            // React'ten gelen DTO'yu, Veritabanının anladığı Entity'ye çevirme izni veriyoruz.
            CreateMap<CreateTaskDto, ProjectTask>();
            
            // YENİ: Görev Listelemede çalışacak AutoMapper kuralı
            CreateMap<ProjectTask, TaskDto>()
                .ForMember(dest => dest.AssignedUserName, opt => opt.MapFrom(src => 
                    src.AssignedUser != null 
                    ? (string.IsNullOrWhiteSpace(src.AssignedUser.FirstName) && string.IsNullOrWhiteSpace(src.AssignedUser.LastName) 
                        ? src.AssignedUser.Email // İsim yoksa email fallback
                        : $"{src.AssignedUser.FirstName} {src.AssignedUser.LastName}".Trim()) 
                    : null));

            CreateMap<RegisterDto, User>();

            // YENİ EKLENEN: Project entity'sini React'e gönderilecek ProjectDto'ya çevir.
            // Alan isimleri birebir eşleştiği için ReverseMap veya özel kural gerekmez.
            // (Id→Id, Name→Name, Description→Description, StartDate→StartDate, EndDate→EndDate)
            CreateMap<Project, ProjectDto>();

            // YENİ: React'teki form verisini (CreateProjectDto) Entity'ye çevirme izni
            // Id, CreatedDate gibi alanlar formda yok — Service katmanında biz dolduracagız
            CreateMap<CreateProjectDto, Project>();
        }

    }
}