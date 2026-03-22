import { useState, useEffect } from 'react';
import { getUsersWithRoles, assignRole } from '../Services/adminService';
import { getUserRoleFromToken } from '../Services/authService';

function AdminPanel({ onBack }) {
    const [users, setUsers] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');
    const [updateMessage, setUpdateMessage] = useState('');

    const userRole = getUserRoleFromToken();

    useEffect(() => {
        // Güvenlik: Admin değilse uyar ve geri at.
        if (!userRole.includes('Admin')) {
            alert('Bu sayfayı görüntülemeye yetkiniz yok.');
            onBack();
            return;
        }

        const fetchUsers = async () => {
            try {
                const data = await getUsersWithRoles();
                setUsers(data);
            } catch (err) {
                setError('Kullanıcılar yüklenirken hata oluştu.');
            } finally {
                setLoading(false);
            }
        };
        fetchUsers();
    }, [userRole, onBack]);

    const handleRoleChange = async (userId, newRole) => {
        try {
            setUpdateMessage('Rol güncelleniyor...');
            // API'ye yeni rolü ataması için istek yolluyoruz
            await assignRole(userId, newRole);
            
            // Başarılıysa sayfayı yenilemeden React tarafında mevcut kullanıcının rolünü güncelliyoruz
            setUsers(users.map(u => 
                u.id === userId ? { ...u, roles: [newRole] } : u
            ));
            
            setUpdateMessage('Rol başarıyla güncellendi! ✅');
            
            // Mesajı 3 saniye sonra ekrandan sil
            setTimeout(() => setUpdateMessage(''), 3000);
        } catch (err) {
            alert('Rol güncellenirken hata oluştu: ' + err);
            setUpdateMessage('');
        }
    };

    if (loading) return <div style={{ padding: '40px' }}>Kullanıcılar yükleniyor, lütfen bekleyin...</div>;
    if (error) return <div style={{ padding: '40px', color: 'red' }}>Hatalı İşlem: {error}</div>;

    return (
        <div style={{ fontFamily: 'sans-serif', padding: '40px', backgroundColor: '#f0f2f5', minHeight: '100vh' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '20px' }}>
                <h1 style={{ color: '#172b4d', margin: 0 }}>🛡️ Kullanıcı Yönetimi (Admin Paneli)</h1>
                <button 
                    onClick={onBack}
                    style={{ padding: '8px 16px', backgroundColor: '#1890ff', color: 'white', border: 'none', borderRadius: '4px', cursor: 'pointer', fontWeight: 'bold' }}
                >
                    ⬅️ Pano'ya Dön
                </button>
            </div>

            {updateMessage && (
                <div style={{ padding: '12px', backgroundColor: '#d9f7be', color: '#237804', borderRadius: '4px', marginBottom: '20px', border: '1px solid #b7eb8f', fontWeight: 'bold' }}>
                    {updateMessage}
                </div>
            )}

            <div className="kanban-column" style={{ backgroundColor: 'white', borderRadius: '8px', overflow: 'hidden' }}>
                <table style={{ width: '100%', borderCollapse: 'collapse', textAlign: 'left' }}>
                    <thead>
                        <tr style={{ backgroundColor: '#fafafa', borderBottom: '1px solid #f0f0f0' }}>
                            <th style={{ padding: '16px', color: '#5e6c84', fontSize: '14px' }}>Ad Soyad</th>
                            <th style={{ padding: '16px', color: '#5e6c84', fontSize: '14px' }}>E-posta</th>
                            <th style={{ padding: '16px', color: '#5e6c84', fontSize: '14px' }}>Mevcut Rol</th>
                            <th style={{ padding: '16px', color: '#5e6c84', fontSize: '14px' }}>Hızlı İşlem</th>
                        </tr>
                    </thead>
                    <tbody>
                        {users.map(user => {
                            // Kullanıcının en güncel (veya atanmış ilk) rolünü gösterelim
                            const currentRole = user.roles && user.roles.length > 0 ? user.roles[0] : 'Employee';
                            
                            return (
                                <tr key={user.id} style={{ borderBottom: '1px solid #f0f0f0', transition: 'background-color 0.2s' }}>
                                    <td style={{ padding: '16px', color: '#172b4d', fontWeight: 'bold' }}>
                                        {user.firstName} {user.lastName}
                                    </td>
                                    <td style={{ padding: '16px', color: '#5e6c84' }}>{user.email}</td>
                                    <td style={{ padding: '16px' }}>
                                        {/* Seçili role göre rozetin rengini ve stilini belirliyoruz */}
                                        <span style={{ 
                                            padding: '4px 8px', 
                                            borderRadius: '4px', 
                                            fontSize: '12px',
                                            fontWeight: 'bold',
                                            backgroundColor: currentRole === 'Admin' ? '#fff0f6' : currentRole === 'ProjectManager' ? '#e6f7ff' : '#f6ffed',
                                            color: currentRole === 'Admin' ? '#eb2f96' : currentRole === 'ProjectManager' ? '#1890ff' : '#52c41a',
                                            border: `1px solid ${currentRole === 'Admin' ? '#ffadd2' : currentRole === 'ProjectManager' ? '#91d5ff' : '#b7eb8f'}`
                                        }}>
                                            {currentRole}
                                        </span>
                                    </td>
                                    <td style={{ padding: '16px' }}>
                                        <select 
                                            value={currentRole}
                                            onChange={(e) => handleRoleChange(user.id, e.target.value)}
                                            style={{ padding: '6px 12px', borderRadius: '4px', border: '1px solid #d9d9d9', backgroundColor: '#fff', cursor: 'pointer', outline: 'none' }}
                                        >
                                            <option value="Employee">Çalışan (Employee)</option>
                                            <option value="ProjectManager">Proje Yöneticisi (PM)</option>
                                            <option value="Admin">Baştanrı (Admin)</option>
                                        </select>
                                    </td>
                                </tr>
                            );
                        })}
                    </tbody>
                </table>
            </div>
        </div>
    );
}

export default AdminPanel;
