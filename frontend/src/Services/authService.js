// Az önce yazdığımız baş garsonu içeri alıyoruz
import api from './api';
import { jwtDecode } from 'jwt-decode';
// async/await yapısı: C#'taki Task mantığıyla birebir aynıdır. 
// "Gidip gelmem biraz sürebilir, beni bekle" demektir.
export const login = async (email, password) => {
    try {
        // 1. Baş garsona diyoruz ki: "/Auth/login" adresine POST isteği yap ve şu verileri (email, password) götür.
        // Axios bu veriyi arka planda otomatik olarak JSON formatına çevirir!
        const response = await api.post('/Auth/login', {
            email: email,
            password: password
        });

        // 2. C# API'miz bize ne dönüyordu? { token: "ey..." }
        // Axios gelen cevabı 'response.data' içine koyar. Biz de bunu geri döndürüyoruz.
        return response.data;
    } catch (error) {
        // Eğer C# API bize 401 veya 500 hatası fırlatırsa sistem buraya düşer.
        // Hatayı React sayfamızda (örneğin ekranda kırmızı bir yazıyla) göstermek için dışarı fırlatıyoruz.
        throw error.response?.data?.message || "Giriş yapılırken bir hata oluştu!";
    }
};
// YENİ EKLENEN: Kayıt Olma Motoru
// Artık firstName ve lastName de alıyor.
// Bu parametreler RegisterPage'deki form alanlarından gelir.
export const registerUser = async (firstName, lastName, email, password) => {
    try {
        const response = await api.post('/Auth/register', {
            firstName: firstName,  // C#'taki RegisterDto.FirstName → bu değeri alır
            lastName: lastName,    // C#'taki RegisterDto.LastName → bu değeri alır
            email: email,
            password: password
        });

        return response.data;
    } catch (error) {
        throw error.response?.data?.message || "Kayıt işlemi başarısız oldu!";
    }
};
// --- GÜNCELLENMİŞ ARAÇ FONKSİYONU ---
export const getUserRoleFromToken = () => {
    const token = localStorage.getItem('token');
    if (!token) return [];

    try {
        const decodedToken = jwtDecode(token);
        // İçi açılmış bileklik (Token'ın içindeki veriler)

        // .NET birden fazla rol varsa 'role' alanını bir DİZİ (array) olarak gönderir.
        // Tek bir rol varsa ise düz string olarak gönderir.
        // Biz her iki durumu da dizi olarak normalize ediyoruz ki kontrollerimiz tutarlı olsun.
        const rawRole = decodedToken.role ||
            decodedToken["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"];

        // Dizi ise doğrudan al, string ise tek elemanlı diziye çevir, yoksa boş dizi döndür.
        const roles = Array.isArray(rawRole) ? rawRole : rawRole ? [rawRole] : [];

        // Eğer token içinde rol listesi dizi ise dizi olarak döner, String ise diziye çevirir
        return roles;
    } catch (error) {
        return [];
    }
};

// YENİ EKLENEN: Token içindeki Kullanıcı ID'sini okuma motoru
export const getUserIdFromToken = () => {
    const token = localStorage.getItem('token');
    if (!token) return null;

    try {
        const decodedToken = jwtDecode(token);
        // .NET varsayılan olarak ID'yi NameIdentifier (http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier) veya 'sub' claim'i içinde tutar.
        const userId = decodedToken.sub || 
                       decodedToken["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"];
        return userId || null;
    } catch (error) {
        return null;
    }
};

// YENİ EKLENEN: API'den tüm kullanıcıları çeken motor
export const getAllUsers = async () => {
    try {
        const response = await api.get('/Auth/users');
        return response.data; // Kullanıcı listesi (Array) döner
    } catch (error) {
        console.error("Kullanıcılar getirilemedi:", error);
        return []; // Hata olursa boş liste dön ki uygulama çökmesin
    }
};

// YENİ EKLENEN: Token içinden Kullanıcının Profil Bilgilerini (Ad, Soyad, Rol) okuma motoru
export const getUserProfileFromToken = () => {
    const token = localStorage.getItem('token');
    if (!token) return null;

    try {
        const decodedToken = jwtDecode(token);
        
        // Rolü bul
        const rawRole = decodedToken.role || decodedToken["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"];
        const roles = Array.isArray(rawRole) ? rawRole : rawRole ? [rawRole] : [];
        const mainRole = roles.length > 0 ? roles[0] : 'Employee';

        return {
            firstName: decodedToken.firstName || 'Bilinmeyen',
            lastName: decodedToken.lastName || 'Kullanıcı',
            email: decodedToken.email || decodedToken["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress"] || '',
            role: mainRole
        };
    } catch (error) {
        return null;
    }
};
// ------------------------------------