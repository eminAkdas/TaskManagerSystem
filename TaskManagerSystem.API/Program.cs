using Microsoft.EntityFrameworkCore;
using TaskManagerSystem.Data.Contexts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models; 

var builder = WebApplication.CreateBuilder(args);
// appsettings.json içindeki "DefaultConnection" metnini okuyoruz.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// AppDbContext'i sisteme (IoC Container) kaydediyoruz ve PostgreSQL kullanacağını söylüyoruz.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

    // --- YENİ EKLENEN KISIM: BAĞIMLILIK ENJEKSİYONU (DI) ---

// 1. Repository Kaydı (Generic olduğu için typeof ile kayıt edilir)
builder.Services.AddScoped(typeof(TaskManagerSystem.Core.Interfaces.IGenericRepository<>), typeof(TaskManagerSystem.Data.Repositories.GenericRepository<>));

// 2. Service Kaydı
builder.Services.AddScoped<TaskManagerSystem.Business.Interfaces.ITaskService, TaskManagerSystem.Business.Services.TaskService>();

// 3. AuthService Kaydı
builder.Services.AddScoped<TaskManagerSystem.Business.Interfaces.IAuthService, TaskManagerSystem.Business.Services.AuthService>();

// 4. ProjectService Kaydı
// "IProjectService isteyene ProjectService ver" — ProjectsController bunu talep edecek.
builder.Services.AddScoped<TaskManagerSystem.Business.Interfaces.IProjectService, TaskManagerSystem.Business.Services.ProjectService>();

 // ... (Diğer DI kayıtları) ...

// AutoMapper'ı sisteme kaydediyoruz. 
// "AppDomain.CurrentDomain.GetAssemblies()" kodu, projedeki "Profile" sınıfından miras alan tüm dosyaları otomatik bulur.
builder.Services.AddAutoMapper(cfg => {
    cfg.AddProfile<TaskManagerSystem.Business.MappingProfiles.TaskMappingProfile>();
});
// --- CORS KURALI: REACT İÇİN KAPIYI AÇIYORUZ ---
// appsettings.json içerisinden "AllowedOrigins" dizisini buluyoruz.
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    // "AllowReactApp" adında özel bir VIP kuralı oluşturuyoruz
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.AllowAnyOrigin() // Herkese İzin Ver (Render.com Environment Bug'ı İçin Kesin Çözüm)
              .AllowAnyHeader()  // Her türlü başlığa (Authorization vb.) izin ver
              .AllowAnyMethod(); // Her türlü metoda (GET, POST, PUT, DELETE) izin ver
    });
});
// -----------------------------------------------------
builder.Services.AddControllers();
// ...  
// --- BİZİM EKLEDİĞİMİZ KISIM BİTİŞİ ---
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "TaskManager API", Version = "v1" });

    // 1. Swagger arayüzüne "Authorize" (Kilit) butonunu ekler
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Lütfen giriş yaptıktan sonra aldığınız 'ey...' ile başlayan Token'ı buraya yapıştırın."
    });

    // 2. Swagger'ın kilitli kapılara (Endpoints) giderken bu token'ı kullanmasını sağlar
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
// --- JWT GÜVENLİK GÖREVLİSİ AYARLARI ---
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["Secret"]!;

builder.Services.AddAuthentication(options =>
{
    // Sistemin varsayılan güvenlik yöntemi JWT olsun diyoruz
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true, // Bilekliği biz mi (Issuer) verdik kontrol et
        ValidateAudience = true, // Bileklik bizim sistemimiz (Audience) için mi kontrol et
        ValidateLifetime = true, // Süresi (120 dk) dolmuş mu kontrol et
        ValidateIssuerSigningKey = true, // Mühür (İmza) doğru mu kontrol et!
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey))
    };
});
// ----------------------------------------

var app = builder.Build();
// --- BİZİM BEKÇİMİZ (EN ÜSTE, İLK SIRAYA YAZILMALI) ---
app.UseMiddleware<TaskManagerSystem.API.Middlewares.ExceptionMiddleware>();
// -----------------------------------------------------

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

// REACT İÇİN KAPIYI AÇAN ŞALTER (YENİ EKLENDİ)
app.UseCors("AllowReactApp");
// --- GÜVENLİK GÖREVLİSİNİ AKTİF ETME ---
app.UseAuthentication(); // Kimlik doğrulama (Login) devrede
app.UseAuthorization();  // Yetkilendirme (Rol kontrolü) devrede
// ---------------------------------------- 

app.MapControllers();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
