using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading.Tasks;

namespace TaskManagerSystem.API.Middlewares
{
    public class ExceptionMiddleware
    {
        // _next: Borudaki bir sonraki adıma (örneğin Controller'a) geçiş izni
        private readonly RequestDelegate _next; 
        
        // _logger: Hataları arka planda siyah konsola (veya bir dosyaya) yazdıran alet
        private readonly ILogger<ExceptionMiddleware> _logger; 
        
        // _env: Şu an kendi bilgisayarımızda mıyız (Development) yoksa gerçek sunucuda mıyız (Production)?
        private readonly IHostEnvironment _env;

        // Constructor üzerinden bu araçları .NET'ten içeri alıyoruz
        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        // İstek API'ye girdiği anda ilk burası çalışır (InvokeAsync)
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // İstek geldiğinde hiçbir şey yapma, sadece "Geçebilirsin" de ve borunun içine (Controller'a) yolla.
                await _next(context);
            }
            catch (Exception ex)
            {
                // EĞER İÇERİDE BİR HATA PATLARSA, SİSTEM ÇÖKMEDEN ÖNCE BURAYA DÜŞER!

                // 1. Hatayı biz görelim diye siyah terminal ekranına kırmızıyla yaz (Logla)
                _logger.LogError(ex, ex.Message);

                // 2. React'e gidecek cevabın (Response) tipini ve kodunu ayarla
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError; // Yani 500 kodu

                // 3. Bir önceki adımda hazırladığımız şık paketi doldur
                var response = new ErrorDetails
                {
                    StatusCode = context.Response.StatusCode,
                    Message = "Sunucu tarafında beklenmeyen bir hata oluştu. Lütfen daha sonra tekrar deneyin.",
                    
                    // İşin sırrı burası: Eğer kendi bilgisayarımızdaysak hatanın o uzun Stack Trace detaylarını ver.
                    // Ama gerçek sunucudaysak (canlıdaysak) dışarıya NULL ver, sırrımızı koru!
                    Details = _env.IsDevelopment() ? ex.StackTrace?.ToString() : null 
                };

                // 4. Paketi JSON'a çevirip React'e (veya Swagger'a) geri fırlat!
                await context.Response.WriteAsync(response.ToString());
            }
        }
    }
}