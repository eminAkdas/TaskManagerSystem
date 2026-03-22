using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TaskManagerSystem.Business.Interfaces;
using TaskManagerSystem.Core.DTOs;
using TaskManagerSystem.Core.Entities;
using TaskManagerSystem.Core.Interfaces;
using System.Linq;
    
namespace TaskManagerSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IGenericRepository<User> _userRepository;

        // Kimlik Doğrulama Müdürümüzü (AuthService) içeri alıyoruz
        public AuthController(IAuthService authService, IGenericRepository<User> userRepository)
        {
            _authService = authService;
            _userRepository = userRepository;
        }

        // POST: api/Auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            var user = await _authService.RegisterAsync(registerDto);
            
            // Güvenlik Kuralı: Kayıt başarılı olduğunda kullanıcıya tüm 'User' nesnesini 
            // (içinde PasswordHash vb. var) dönmüyoruz. Sadece bir başarı mesajı dönüyoruz.
            return Ok(new { message = "Kayıt işlemi başarıyla tamamlandı!", userId = user.Id });
        }

        // POST: api/Auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            // Müdürümüz şifreyi doğrulayıp bize o uzun, şifreli JWT metnini (Bilekliği) verecek
            var token = await _authService.LoginAsync(loginDto);
            
            // React (Frontend) tarafının bu token'ı kolayca okuyup tarayıcıya kaydedebilmesi için
            // bunu standart bir JSON objesi formatında ({ "token": "eyJh..." }) dönüyoruz.
            return Ok(new { token = token });
        }
        // YENİ EKLENEN: Sistemdeki tüm kullanıcıları listeleme kapısı
        // [Authorize] kullanmıyoruz — her oturum açmış kullanıcı görev atama için listeye ihtiyaç duyar.
        // Üst sınıfta [Authorize] olmadığı için zaten JWT gerekiyor (ProjectsController gibi ayarlanmadı).
        // Eğer hassas rol kısıtlaması gerekirse [Authorize] eklenebilir.
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var allUsers = await _userRepository.GetAllAsync();
            var users = allUsers
                .Select(u => new
                {
                    Id = u.Id,
                    Email = u.Email,
                    // YENİ: Artık isim-soyisim de gönderiyoruz.
                    // Görev atama dropdown'ında mail yerine "Ali Yılmaz" gibi isimler görünsün.
                    FirstName = u.FirstName,
                    LastName = u.LastName
                })
                .ToList();

            return Ok(users);
        }
    }
}