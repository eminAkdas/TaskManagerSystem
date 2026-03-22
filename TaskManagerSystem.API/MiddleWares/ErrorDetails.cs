using System.Text.Json;

namespace TaskManagerSystem.API.Middlewares
{
    // Dışarı fırlatacağımız standart hata şablonu
    public class ErrorDetails
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        
        // Detaylar kısmı nullable (?). Canlı ortamda (Production) burayı boş bırakacağız ki hackerlar kodumuzun içini görmesin!
        public string? Details { get; set; } 

        // Sınıfı doğrudan JSON metnine çevirmek için küçük bir C# hilesi (Override)
        public override string ToString()
        {
            // JsonSerializer, C#'ın kendi kütüphanesidir. Sınıfı alır, { "StatusCode": 500, ... } formatına çevirir.
            return JsonSerializer.Serialize(this);
        }
    }
}
