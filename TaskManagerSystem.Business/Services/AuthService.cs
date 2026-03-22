using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using AutoMapper;
using TaskManagerSystem.Business.Interfaces;
using TaskManagerSystem.Core.DTOs;
using TaskManagerSystem.Core.Entities;
using TaskManagerSystem.Core.Interfaces;
using BCrypt.Net;
using Microsoft.Extensions.Configuration;

namespace TaskManagerSystem.Business.Services
{
    public class AuthService : IAuthService
    {
        private readonly IGenericRepository<User> _userRepository;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IGenericRepository<UserRole> _userRoleRepository;
        private readonly IGenericRepository<Role> _roleRepository;

        // Garsonu (Repository) ve Çevirmeni (Mapper) müdürün emrine veriyoruz
       public AuthService(
            IGenericRepository<User> userRepository, 
            IGenericRepository<UserRole> userRoleRepository,
            IGenericRepository<Role> roleRepository,
            IMapper mapper, 
            IConfiguration configuration)
        {
            _userRepository = userRepository;
            _userRoleRepository = userRoleRepository;
            _roleRepository = roleRepository;
            _mapper = mapper;
            _configuration = configuration;
        }

        public async Task<User> RegisterAsync(RegisterDto registerDto)
        {
            // İş Kuralı 1: Bu e-posta adresiyle daha önce kayıt olunmuş mu?
            var existingUsers = await _userRepository.FindAsync(u => u.Email == registerDto.Email);
            if (existingUsers.Any())
            {
                throw new Exception("Bu e-posta adresi zaten kullanılıyor!");
            }

            // Çevirmen, DTO'daki Ad, Soyad ve E-postayı gerçek User nesnesine kopyalıyor
            var newUser = _mapper.Map<User>(registerDto);

            // İş Kuralı 2: Şifreyi ASLA düz metin kaydetme! BCrypt ile Hash'le.
            // BCrypt arka planda şifreye rastgele bir "Tuz (Salt)" ekler ve karmaşık bir metne çevirir.
            newUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);
            newUser.CreatedDate = DateTime.UtcNow;

            // Veritabanına kaydet
            await _userRepository.AddAsync(newUser);
            await _userRepository.SaveChangesAsync();

            // --- YENİ EKLENEN: Otomatik Rol Atama Mimarisi ---
            // İş mantığı (Business Logic) Controller'da değil, burada (Service'te) olmalıdır.
            // 1. Veritabanından "Employee" adlı varsayılan rolü buluyoruz.
            //    Kayıt olan her kullanıcı sisteme en düşük yetkiyle girer.
            //    Daha yüksek roller (ProjectManager, Admin) sonradan bir Admin tarafından atanır.
            var roles = await _roleRepository.FindAsync(r => r.Name == "Employee");
            var employeeRole = roles.FirstOrDefault();

            // 2. Eğer veritabanında "Employee" rolü tanımlıysa, yeni kullanıcıyla bağlıyoruz.
            if (employeeRole != null)
            {
                var userRole = new UserRole
                {
                    UserId = newUser.Id,
                    RoleId = employeeRole.Id
                };

                // 3. UserRole köprü tablosuna bu bağlantıyı ekleyip kaydediyoruz.
                await _userRoleRepository.AddAsync(userRole);
                await _userRoleRepository.SaveChangesAsync();
            }
            // ---------------------------------------------------

            return newUser;
        }

        // --- YENİ: Admin'in kullanıcılara rol atama motoru ---
        public async Task AssignRoleAsync(Guid userId, string roleName)
        {
            // 1. Atanacak kullanıcı gerçekten var mı? Yoksa hata fırlat.
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new Exception($"Kullanıcı bulunamadı (ID: {userId})");

            // 2. İstenen isimde bir rol veritabanında tanımlı mı?
            var roles = await _roleRepository.FindAsync(r => r.Name == roleName);
            var role = roles.FirstOrDefault();
            if (role == null)
                throw new Exception($"'{roleName}' adlı rol sistemde tanımlı değil.");

            // 3. Kullanıcının mevcut tüm rollerini bul
            var existingAssignments = await _userRoleRepository.FindAsync(ur => ur.UserId == userId);
            var existingList = existingAssignments.ToList();
            
            // Eğer zaten seçili role sahipse ve BAŞKA rolü yoksa hiçbir şey yapma (sessiz başarı)
            if (existingList.Count == 1 && existingList.First().RoleId == role.Id)
                return;

            // Diğer tüm eski rolleri veritabanından sil (Drop-down mantığı: Sadece tek bir role sahip olmalı)
            foreach (var assignment in existingList)
            {
                _userRoleRepository.Remove(assignment);
            }

            // 4. Yeni rolü köprü tablosuna ekle.
            var userRole = new UserRole
            {
                UserId = userId,
                RoleId = role.Id
            };
            await _userRoleRepository.AddAsync(userRole);
            await _userRoleRepository.SaveChangesAsync();
        }
        // ---------------------------------------------------

        // --- YENİ: Admin'in kullanıcıdan rol kaldırma motoru ---
        public async Task RemoveRoleAsync(Guid userId, string roleName)
        {
            // 1. İstenen isimde bir rol sistemde var mı?
            var roles = await _roleRepository.FindAsync(r => r.Name == roleName);
            var role = roles.FirstOrDefault();
            if (role == null)
                throw new Exception($"'{roleName}' adlı rol sistemde tanımlı değil.");

            // 2. Bu kullanıcıda zaten bu rol var mı? Yoksa kaldıracak bir şey yok.
            var existingAssignments = await _userRoleRepository.FindAsync(
                ur => ur.UserId == userId && ur.RoleId == role.Id);
            var assignment = existingAssignments.FirstOrDefault();
            if (assignment == null)
                throw new Exception($"Bu kullanıcı zaten '{roleName}' rolüne sahip değil.");

            // 3. Köprü tablosundaki bu satırı sil.
            _userRoleRepository.Remove(assignment);
            await _userRoleRepository.SaveChangesAsync();
        }
        // ---------------------------------------------------

        // YENİ: Admin Paneli için tüm kullanıcıları ve rollerini listeleyen metod
        public async Task<IEnumerable<UserWithRoleDto>> GetAllUsersWithRolesAsync()
        {
            var users = await _userRepository.GetAllAsync();
            var userRoles = await _userRoleRepository.GetAllAsync();
            var roles = await _roleRepository.GetAllAsync();

            var result = new List<UserWithRoleDto>();

            foreach (var user in users)
            {
                // Bu kullanıcının sahip olduğu rol ID'lerini buluyoruz
                var roleIds = userRoles.Where(ur => ur.UserId == user.Id).Select(ur => ur.RoleId).ToList();
                
                // O ID'lere denk gelen rol isimlerini buluyoruz
                var userRoleNames = roles.Where(r => roleIds.Contains(r.Id)).Select(r => r.Name).ToList();

                result.Add(new UserWithRoleDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Roles = userRoleNames
                });
            }

            return result;
        }

        public async Task<string> LoginAsync(LoginDto loginDto)
        {
            // 1. Kullanıcıyı e-posta adresinden bul
            var users = await _userRepository.FindAsync(u => u.Email == loginDto.Email);
            var user = users.FirstOrDefault();

            // Güvenlik Kuralı: "E-posta bulunamadı" demek yerine genel bir hata mesajı ver. 
            // Kötü niyetli kişilere sistemde hangi e-postaların kayıtlı olduğuna dair ipucu verme!
            if (user == null)
            {
                throw new Exception("E-posta veya şifre hatalı!");
            }

            // 2. Şifreyi Doğrula (Verify)
            // Kullanıcının girdiği "Memin123" ile veritabanındaki "$2a$11$K.G..." hashini karşılaştırır.
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash);

            if (!isPasswordValid)
            {
                throw new Exception("E-posta veya şifre hatalı!");
            }

            // 3. Şifre doğruysa JWT Token üretip döneceğiz.
            // (JWT üretim kodlarını bir sonraki adımda yazacağız, şimdilik geçici bir metin dönelim)
            return await GenerateJwtToken(user);
        }
        // Dışarıdan çağrılamayan, sadece Müdür'ün bileklik üretmek için kullandığı makine

        private async Task<string> GenerateJwtToken(User user)
        {
            // 1. appsettings.json'dan ayarları okuyoruz
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = Encoding.ASCII.GetBytes(jwtSettings["Secret"]!);

            // 2. Payload (Yük) kısmına koyacağımız bilgileri (Claims) hazırlıyoruz
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                // React tarafında Ekranda göstermek için İsim ve Soyisim bilgilerini de bilekliğe yazıyoruz
                new Claim("firstName", user.FirstName),
                new Claim("lastName", user.LastName)
            };
            // --- YENİ EKLENEN KISIM: KULLANICININ ROLLERİNİ BUL VE BİLEKLİĞE YAZ ---
            
            // 1. Köprü tablodan bu kullanıcının rol ID'lerini bul
            var userRoles = await _userRoleRepository.FindAsync(ur => ur.UserId == user.Id);
            
            // 2. Her bir rol ID'si için Rol tablosuna gidip Rolün Adını (Name) al ve Claims'e ekle
            foreach (var userRole in userRoles)
            {
                var role = await _roleRepository.GetByIdAsync(userRole.RoleId);
                if (role != null)
                {
                    // ClaimTypes.Role, .NET'in güvenlik görevlisinin özel olarak aradığı damgadır!
                    claims.Add(new Claim(ClaimTypes.Role, role.Name));
                }
            }

            // 3. İmza (Mühür) algoritmasını belirliyoruz
            var credentials = new SigningCredentials(
                new SymmetricSecurityKey(secretKey), 
                SecurityAlgorithms.HmacSha256Signature);

            // 4. Token'ın (Bilekliğin) taslağını oluşturuyoruz
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["DurationInMinutes"]!)),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                SigningCredentials = credentials
            };

            // 5. Taslağı gerçek bir metne çevirip geri dönüyoruz
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}