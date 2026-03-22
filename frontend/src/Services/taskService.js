import api from './api';

// Belirli bir projeye ait görevleri C# API'den çeken fonksiyon
export const getTasksByProjectId = async (projectId) => {
    try {
        // DİKKAT ET: Burada token'ı hiç düşünmüyoruz! 
        // Çünkü api.js içindeki o harika Interceptor (Araya Girici) token'ı kasadan alıp bu isteğin başına otomatik takacak.
        const response = await api.get(`/Tasks/project/${projectId}`);
        return response.data; // C#'tan gelen görev listesini (Array) döndürür
    } catch (error) {
        throw error.response?.data?.message || "Görevler getirilirken bir hata oluştu!";
    }
};
// YENİ EKLENEN: Veritabanına yeni görev ekleme fonksiyonu
export const createTask = async (taskData) => {
    try {
        // api.post('/Tasks', veri) diyerek C#'taki [HttpPost] metodumuzu tetikliyoruz.
        // Interceptor yine bilekliğimizi (Token) gizlice bu isteğin başına takacak!
        const response = await api.post('/Tasks', taskData);

        // C# API'miz, başarıyla oluşturulan görevi (ID'si atanmış halde) bize geri dönüyordu.
        return response.data;
    } catch (error) {
        throw error.response?.data?.message || "Görev eklenirken bir hata oluştu!";
    }
};
// YENİ EKLENEN: Görev durumu güncelleme (PATCH) motoru
export const updateTaskStatus = async (taskId, newStatusValue) => {
    try {
        // api.patch ile C#'taki [HttpPatch("{taskId}/status")] metodumuzu tetikliyoruz.
        const response = await api.patch(`/Tasks/${taskId}/status`, { newStatus: newStatusValue });
        return response.data;
    } catch (error) {
        throw error.response?.data?.message || "Görev güncellenirken bir hata oluştu!";
    }
};
// YENİ EKLENEN: Görevi veritabanından silme (DELETE) motoru
export const deleteTask = async (taskId) => {
    try {
        // api.delete ile C#'taki [HttpDelete("{id}")] metodumuzu tetikliyoruz.
        const response = await api.delete(`/Tasks/${taskId}`);
        return response.data;
    } catch (error) {
        throw error.response?.data?.message || "Görev silinirken bir hata oluştu!";
    }
};