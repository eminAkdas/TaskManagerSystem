using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using TaskManagerSystem.Business.Interfaces;
using TaskManagerSystem.Core.DTOs;
using TaskManagerSystem.Core.Entities;

namespace TaskManagerSystem.API.Controllers
{
    [ApiController] 
    [Route("api/[controller]")] 
    [Authorize]     // Artık bu kapıdan girmek için "Bileklik" (Token) şart!
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _taskService;
        private readonly IMapper _mapper; // Çevirmenimizi ekledik

        // Constructor üzerinden hem Müdürü hem Çevirmeni içeri alıyoruz
        public TasksController(ITaskService taskService, IMapper mapper)
        {
            _taskService = taskService;
            _mapper = mapper;
        }

        [HttpPost]
        [Authorize(Roles = "ProjectManager, Admin")]
        public async Task<IActionResult> CreateTask([FromBody] CreateTaskDto taskDto)
        {
            // 1. Çevirmen (AutoMapper) tek satırda DTO'yu Entity'ye çeviriyor!
            var newTaskEntity = _mapper.Map<ProjectTask>(taskDto);

            // 2. Entity'yi Müdür'e veriyoruz
            var createdTask = await _taskService.CreateTaskAsync(newTaskEntity);
            
            return Ok(_mapper.Map<TaskDto>(createdTask));
        }

       // GET: api/Tasks/project/{projectId}
        [HttpGet("project/{projectId}")]
        [Authorize(Roles = "ProjectManager, Admin, Employee")]
        public async Task<IActionResult> GetTasksByProjectId(Guid projectId)
        {
            // 1. Müdürden (Service) o projeye ait gerçek görev listesini (Entity) istiyoruz
            var tasks = await _taskService.GetTasksByProjectIdAsync(projectId);

            // 2. Çevirmene (AutoMapper) diyoruz ki: "Bu gelen gerçek görevleri al, TaskDto listesine çevir"
            var tasksDto = _mapper.Map<IEnumerable<TaskDto>>(tasks);

            // 3. Güvenli paketleri (DTO) HTTP 200 (OK) ile React'e (veya Postmana) geri dönüyoruz
            return Ok(tasksDto);
        }
        // PATCH: api/Tasks/{taskId}/status
        [HttpPatch("{taskId}/status")]
        // YENİ: Artık Employee'ler de (Çalışanlar) görev durumunu güncelleyebilir!
        [Authorize(Roles = "ProjectManager, Admin, Employee")]
        public async Task<IActionResult> UpdateTaskStatus(Guid taskId, [FromBody] UpdateTaskStatusDto statusDto)
        {
            // Müdürümüze (TaskService) görevin ID'sini ve yeni durumunu veriyoruz.
            // O arka planda görevi bulacak, kuralları kontrol edip veritabanına kaydedecek.
            await _taskService.UpdateTaskStatusAsync(taskId, statusDto.NewStatus);

            // İşlem başarılı olduysa HTTP 204 (No Content) döneriz.
            // Anlamı: "İşlemi başarıyla yaptım, sana geri gönderecek ekstra bir verim yok."
            return NoContent(); 
        }

        // DELETE: api/Tasks/{taskId}
        [HttpDelete("{taskId}")]
        // YENİ: Employee'ler (Çalışanlar) de kendilerine ait görevleri silebilir
        [Authorize(Roles = "ProjectManager, Admin, Employee")]
        public async Task<IActionResult> DeleteTask(Guid taskId)
        {
            // Müdürümüze (TaskService) görevin ID'sini verip silmesini söylüyoruz.
            await _taskService.DeleteTaskAsync(taskId);
            
            // İşlem başarılı olduysa HTTP 204 (No Content) döneriz.
            return NoContent();
        }
    }
}