using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using TaskManagerSystem.Business.Interfaces;
using TaskManagerSystem.Core.DTOs;

namespace TaskManagerSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // GÜVENLİK DUVARI: Bu kapıdan sadece ve sadece JWT Token'ı içinde
    // "Admin" rütbesi olanlar geçebilir. Employee veya ProjectManager içeri giremez!
    [Authorize(Roles = "Admin")] 
    public class AdminController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AdminController(IAuthService authService)
        {
            _authService = authService;
        }

        // Endpoint: POST api/admin/assign-role
        [HttpPost("assign-role")]
        public async Task<IActionResult> AssignRole([FromBody] AssignRoleDto request)
        {
            try
            {
                await _authService.AssignRoleAsync(request.UserId, request.RoleName);
                return Ok(new { message = $"'{request.RoleName}' rolü, kullanıcıya başarıyla atandı." });
            }
            catch (Exception ex)
            {
                // Sunucu çökmesi (500) yerine düzgün bir 400 (Bad Request) dönüp hatayı React'e yansıtıyoruz.
                return BadRequest(new { message = ex.Message });
            }
        }

        // Endpoint: POST api/admin/remove-role
        [HttpPost("remove-role")]
        public async Task<IActionResult> RemoveRole([FromBody] AssignRoleDto request)
        {
            try
            {
                await _authService.RemoveRoleAsync(request.UserId, request.RoleName);
                return Ok(new { message = $"'{request.RoleName}' rolü, kullanıcıdan başarıyla kaldırıldı." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Endpoint: GET api/admin/users
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            // Yeni oluşturduğumuz kullanıcıları rolleriyle dökme metodunu çağır
            var usersWithRoles = await _authService.GetAllUsersWithRolesAsync();
            
            return Ok(usersWithRoles);
        }
    }
}
