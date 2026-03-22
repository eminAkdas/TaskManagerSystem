import api from './api';

export const getProjects = async () => {
    try {
        const response = await api.get('/Projects');
        return response.data;
    } catch (error) {
        throw error.response?.data?.message || "Projeler getirilirken bir hata oluştu!";
    }
};

// Yeni proje oluşturma — POST /api/Projects
// projectData: { name, description, startDate, endDate? }
export const createProject = async (projectData) => {
    try {
        const response = await api.post('/Projects', projectData);
        return response.data;
    } catch (error) {
        throw error.response?.data?.message || "Proje oluşturulurken bir hata oluştu!";
    }
};

// Projeyi sil — DELETE /api/Projects/{id}
// 204 No Content döner (body yok), hata durumunda exception fırlar
export const deleteProject = async (projectId) => {
    try {
        await api.delete(`/Projects/${projectId}`);
        // 204 döndüğünde response.data boş — sadece başarı/başarısızlık yeterli
    } catch (error) {
        throw error.response?.data?.message || "Proje silinirken bir hata oluştu!";
    }
};


