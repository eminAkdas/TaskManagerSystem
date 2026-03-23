import axios from 'axios';

// Axios'un özel bir kopyasını (instance) oluşturuyoruz.
const api = axios.create({
    // Artık adresi sabit yazmıyoruz, Vite'in bizim için .env dosyasından okuduğu değişkeni kullanıyoruz!
    baseURL: import.meta.env.VITE_API_BASE_URL,
    // Render gibi ücretsiz bulut servisleri 15 dk boş kalınca uykuya dalar. İlk istekte uyanması 50 saniye sürebilir!
    // Bu yüzden Frontend'in hemen pes etmemesi (Network Error / CORS hatası vermemesi) için bekleme süresini 60 saniyeye çıkardık.
    timeout: 60000,
});

// ──────────────────────────────────────────────────────────────
// İSTEK YAKAÇISI (Request Interceptor)
// Mutfağa (API'ye) gidecek her istek burada duruyor.
// Kasadan (localStorage) token'ı alıp isteğin üstüne takıyoruz.
// ──────────────────────────────────────────────────────────────
api.interceptors.request.use(
    (config) => {
        const token = localStorage.getItem('token');
        if (token) {
            config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
    },
    (error) => {
        return Promise.reject(error);
    }
);

// ──────────────────────────────────────────────────────────────
// YANIT YAKAÇISI (Response Interceptor)
// C# API'den gelen her yanıt, esas servise ulaşmadan önce
// burada denetlenir. Başarılı yanıtlar geçer, hatalılar yakalanır.
// ──────────────────────────────────────────────────────────────
api.interceptors.response.use(
    // Durum kodu 2xx olan her yanıt bu kola girer — başarılı, geç.
    (response) => response,

    // Durum kodu 2xx DIŞINDA olan her yanıt bu kola girer.
    (error) => {
        // error.response → sunucudan gelen yanıt nesnesi (401, 403, 404, 500...)
        // Bazen sunucuya hiç ulaşılamaz (ağ kesilmesi, CORS bloğu).
        // O durumda error.response tamamen undefined olur — bu yüzden?.ile güvenli erişim.
        if (error.response?.status === 401) {
            // 401 Unauthorized: "Bu isteği yapmaya yetkin yok — kim olduğunu bile bilmiyorum."
            // Yani token ya:
            //   a) Hiç gönderilmedi
            //   b) Süresi doldu (JWT'nin exp claim'i geçti)
            //   c) Sunucunun secret'ıyla uyuşmuyor (imza bozuk)

            // Kasayı (localStorage) temizle — geçersiz token'ı saklamanın anlamı yok.
            localStorage.removeItem('token');

            // Kullanıcıyı login ekranına yönlendir.
            // window.location.href kullanıyoruz çünkü burada React Router'ın
            // navigate() fonksiyonuna erişimiz yok (bu bir React component'ı değil).
            // window.location.href sayfayı tamamen yeniden yükler — React state'i sıfırlanır.
            // Bu aslında istediğimiz şey: eski kullanıcı bilgileri bellekte kalmasın.
            window.location.href = '/login';
        }

        // Hatayı zincire geri gönder — her servis dosyasındaki catch bloğu
        // kendi özel hata mesajını üretebilsin.
        return Promise.reject(error);
    }
);

export default api;