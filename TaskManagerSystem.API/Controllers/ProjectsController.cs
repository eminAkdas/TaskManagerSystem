using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagerSystem.Business.Interfaces;
using TaskManagerSystem.Core.DTOs;

namespace TaskManagerSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Tüm proje endpoint'leri için giriş yapılmış olmak yeterli (rol fark etmez)
    public class ProjectsController : ControllerBase
    {
        // Controller sadece iki şeyi bilir: Service (iş mantığı) ve Mapper (çevirmen).
        // Veritabanından hiç haberi yok — bu N-Tier'ın özü.
        private readonly IProjectService _projectService;
        private readonly IMapper _mapper;

        public ProjectsController(IProjectService projectService, IMapper mapper)
        {
            _projectService = projectService;
            _mapper = mapper;
        }

        // GET: api/Projects
        [HttpGet]
        public async Task<IActionResult> GetAllProjects()
        {
            var projects = await _projectService.GetAllProjectsAsync();
            var projectDtos = _mapper.Map<IEnumerable<ProjectDto>>(projects);
            return Ok(projectDtos);
        }

        // POST: api/Projects
        // Gövde (Body): { "name": "...", "description": "...", "startDate": "2026-01-01" }
        // Sadece ProjectManager ve Admin yeni proje açabilir.
        // Employee görevi yapar ama projeyi o kurmaz — bu bir iş kuralıdır.
        [HttpPost]
        [Authorize(Roles = "ProjectManager, Admin")]
        public async Task<IActionResult> CreateProject([FromBody] CreateProjectDto createProjectDto)
        {
            if (string.IsNullOrWhiteSpace(createProjectDto.Name))
                return BadRequest(new { message = "Proje adı boş olamaz." });

            // İş mantığını Service'e devret
            var createdProjectDto = await _projectService.CreateProjectAsync(createProjectDto);

            // 201 Created: "Başarıyla oluşturuldu" — 200 OK'dan semantik olarak farklı.
            // İyi bir REST API, yeni kaynak oluşturulduğunda 201 döner.
            return CreatedAtAction(nameof(GetAllProjects), createdProjectDto);
        }

        // DELETE: api/Projects/{id}
        // URL'den gelen {id} ile hangi projenin silineceğini belirtiyoruz.
        // [FromRoute] → parametreyi URL'den oku (varsayılan da bu, explicit yazmak netlik için iyidir)
        // Sadece ProjectManager ve Admin silebilir.
        [HttpDelete("{id}")]
        [Authorize(Roles = "ProjectManager, Admin")]
        public async Task<IActionResult> DeleteProject(Guid id)
        {
            try
            {
                await _projectService.DeleteProjectAsync(id);

                // 204 No Content: "Silindi, sana gönderecek bir içerik yok."
                // REST standartlarına göre DELETE başarılı olduğunda body dönmez.
                return NoContent();
            }
            catch (Exception ex)
            {
                // Service'ten fırlanan "Proje bulunamadı" hatası buraya düşer → 404 dön
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
