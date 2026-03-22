import api from './api';

/**
 * Adminlere özel servis çağrıları.
 * Bu endpointlerin hepsi backend'de [Authorize(Roles = "Admin")] ile korunur.
 */

// Sistemdeki tüm kullanıcıları ve sahip oldukları rolleri getirir.
export const getUsersWithRoles = async () => {
    try {
        const response = await api.get('/admin/users');
        return response.data;
    } catch (error) {
        throw error;
    }
};

// Bir kullanıcıya yeni rol atar.
export const assignRole = async (userId, roleName) => {
    try {
        const response = await api.post('/admin/assign-role', { userId, roleName });
        return response.data;
    } catch (error) {
        throw error;
    }
};

// Bir kullanıcının mevcut rolünü siler.
export const removeRole = async (userId, roleName) => {
    try {
        const response = await api.post('/admin/remove-role', { userId, roleName });
        return response.data;
    } catch (error) {
        throw error;
    }
};
