import { useState, useEffect } from 'react';
import { getTasksByProjectId, createTask, updateTaskStatus, deleteTask } from '../Services/taskService';
import { getUserRoleFromToken, getAllUsers, getUserIdFromToken, getUserProfileFromToken } from '../Services/authService';
import { getProjects, createProject, deleteProject } from '../Services/projectService';
function DashboardPage({ onAdminClick }) {
    // 1. HAFIZA (STATE) BÖLÜMÜ
    // tasks: Görevlerin listesini tutacak (Başlangıçta boş bir dizi: [])
    const [tasks, setTasks] = useState([]);
    const [error, setError] = useState('');
    const [loading, setLoading] = useState(true); // Veriler yükleniyor mu?
    // --- YENİ EKLENEN HAFIZA (STATE) ALANLARI ---
    // Form ekranda açık mı, kapalı mı? (Başlangıçta kapalı: false)
    const [isFormOpen, setIsFormOpen] = useState(false);
    // Kullanıcının kutucuklara yazdığı Başlık ve Açıklama
    const [newTaskTitle, setNewTaskTitle] = useState('');
    const [newTaskDesc, setNewTaskDesc] = useState('');
    const userRole = getUserRoleFromToken();
    const currentUserId = getUserIdFromToken();
    const userProfile = getUserProfileFromToken(); // YENİ: Sağ üstte profil bilgisini göstermek için
    const [users, setUsers] = useState([]);
    const [newAssignedUserId, setNewAssignedUserId] = useState('');

    // YENİ: Proje listesi ve seçili projeyi tutan state'ler
    // projects      : API'den gelen tüm projeler
    // selectedProjectId : Kullanıcının seçtiği projenin ID'si (başlangıçta hiçbiri seçili değil)
    const [projects, setProjects] = useState([]);
    const [selectedProjectId, setSelectedProjectId] = useState(null);

    // Proje oluşturma formu için state'ler
    const [isProjectFormOpen, setIsProjectFormOpen] = useState(false);
    const [newProjectName, setNewProjectName] = useState('');
    const [newProjectDesc, setNewProjectDesc] = useState('');
    const [newProjectStartDate, setNewProjectStartDate] = useState('');

    // EFFECT 1: Sayfa ilk açıldığında projeleri ve kullanıcıları çek.
    // Bağımlılık dizisi boş [] → sadece bir kez, mount anında çalışır.
    useEffect(() => {
        const fetchInitialData = async () => {
            try {
                const [projectsData, usersData] = await Promise.all([
                    getProjects(),  // GET /api/Projects
                    getAllUsers(),   // GET /api/Auth/users
                ]);
                setProjects(projectsData);
                setUsers(usersData);
                // Varsayılan olarak listedeki ilk projeyi seç
                if (projectsData.length > 0) {
                    setSelectedProjectId(projectsData[0].id);
                }
            } catch (err) {
                setError('Başlangıç verileri yüklenemedi.');
            } finally {
                setLoading(false);
            }
        };
        fetchInitialData();
    }, []);

    // EFFECT 2: Kullanıcı farklı bir projeye geçtiğinde görevleri yeniden çek.
    // [selectedProjectId] bağımlılığı: bu değer değiştiğinde effect yeniden tetiklenir.
    // Bu, Bölüm 5'te anlattığımız "bağımlılık dizisi" konseptinin pratik kullanımıdır.
    useEffect(() => {
        if (!selectedProjectId) return; // Henüz proje seçilmediyse çalışma

        const fetchTasks = async () => {
            setLoading(true);
            try {
                const tasksData = await getTasksByProjectId(selectedProjectId);
                setTasks(tasksData);
            } catch (err) {
                setError('Görevler yüklenemedi.');
            } finally {
                setLoading(false);
            }
        };
        fetchTasks();
    }, [selectedProjectId]);
    // YENİ EKLENEN: Form gönderildiğinde çalışacak motor!
    const handleCreateTask = async (e) => {
        e.preventDefault(); // Sayfanın F5 yapmasını (yenilenmesini) engelle

        // C#'ın bizden beklediği DTO formatını hazırlıyoruz (CreateTaskDto ile aynı olmalı)
        const newTaskData = {
            title: newTaskTitle,
            description: newTaskDesc,
            projectId: selectedProjectId, // Artık sabit değil, state'ten geliyor!
            assignedUserId: newAssignedUserId !== '' ? newAssignedUserId : null
        };

        try {
            // 1. Garsonu (createTask) C#'a yolla
            const createdTask = await createTask(newTaskData);

            // 2. İŞTE REACT'İN SİHRİ: Sayfayı yenilemeden yeni görevi ekrana çiz!
            // setTasks fonksiyonuna diyoruz ki: "Eski görevleri (...tasks) al, sonuna C#'tan yeni gelen görevi ekle"
            setTasks([...tasks, createdTask]);

            // 3. Formu temizle ve kapat
            setNewTaskTitle('');
            setNewTaskDesc('');
            setIsFormOpen(false);

        } catch (err) {
            alert("Hata: " + err); // Hata olursa kullanıcıya popup göster
        }
    };
    // YENİ EKLENEN: Görevi bir kolondan diğerine taşıma motoru
    const handleStatusChange = async (task, newStatus) => {
        try {
            // 2. Garsonu (updateTaskStatus) C#'a yolla ve veritabanını güncelle
            await updateTaskStatus(task.id, newStatus);

            // 3. REACT SİHRİ: Sayfayı yenilemeden ekranı güncelle!
            // Eski görev listesini gez, eğer ID'si güncellediğimiz görevin ID'sine eşitse yeni veriyi koy, değilse eskisi gibi bırak.
            setTasks(tasks.map(t => t.id === task.id ? { ...t, status: newStatus } : t));

        } catch (err) {
            alert("Durum güncellenirken hata: " + err);
        }
    };
    // YENİ EKLENEN: Görevi silme motoru
    const handleDeleteTask = async (taskId) => {
        // 1. GÜVENLİK: Kullanıcı yanlışlıkla basmış olabilir, teyit al!
        const isConfirmed = window.confirm("Bu görevi kalıcı olarak silmek istediğinize emin misiniz?");

        // Eğer "İptal" derse fonksiyonu burada durdur (return), aşağıya geçme.
        if (!isConfirmed) return;

        try {
            // 2. Garsonu (deleteTask) C#'a yolla ve veritabanından sil
            await deleteTask(taskId);

            // 3. REACT SİHRİ: Sayfayı yenilemeden o görevi ekrandan yok et!
            // Mantık: "Eski görevleri filtrele, ID'si sildiğimiz görev OLANLARI çöpe at, OLMAYANLARI ekranda bırak."
            setTasks(tasks.filter(t => t.id !== taskId));

        } catch (err) {
            alert("Silme işlemi başarısız: " + err);
        }
    };

    // Kart ilk tutulduğunda çalışır
    const handleDragStart = (e, taskId) => {
        // İŞ KURALI: Employee sadece kendi görevlerini veya kimseye atanmamış görevleri taşıyabilir!
        if (!userRole.includes('Admin') && !userRole.includes('ProjectManager')) {
            const taskToMove = tasks.find(t => t.id === taskId);
            // Görev başkasına atanmışsa engelle
            if (taskToMove && taskToMove.assignedUserId && taskToMove.assignedUserId !== currentUserId) {
                e.preventDefault(); // Sürüklemeyi iptal et
                alert("Sadece size atanan veya kimseye atanmamış görevleri taşıyabilirsiniz!");
                return;
            }
        }

        // react-dnd veya HTML5 Drag and Drop API'si kullanıyoruz.
        // Sürüklenen veriye 'taskId' adıyla görevin ID'sini ekliyoruz.
        e.dataTransfer.setData('taskId', taskId);
    };

    // YENİ: Proje oluşturma formu gönderildiğinde çalışar
    const handleCreateProject = async (e) => {
        e.preventDefault();
        try {
            const projectData = {
                name: newProjectName,
                description: newProjectDesc,
                startDate: new Date(newProjectStartDate).toISOString(),
            };
            // C#'a POST at, yeni projeyi (ID'siyle birlikte) geri al
            const createdProject = await createProject(projectData);

            // React Sihri: Sol menüye sayfayı yenilemeden yeni projeyi ekle
            setProjects([...projects, createdProject]);

            // Yeni projeyi hemen seç ki kanban panosu ona ait görevleri göstersin
            setSelectedProjectId(createdProject.id);

            // Formu temizle ve kapat
            setNewProjectName('');
            setNewProjectDesc('');
            setNewProjectStartDate('');
            setIsProjectFormOpen(false);
        } catch (err) {
            alert('Proje oluşturulamadı: ' + err);
        }
    };

    // YENİ: Proje silme motoru
    const handleDeleteProject = async (projectId) => {
        const isConfirmed = window.confirm('Bu projeyi ve tüm görevlerini silmek istediğinize emin misiniz?');
        if (!isConfirmed) return;

        try {
            await deleteProject(projectId);

            // React Sihri: Silinen projeyi sol menüden kaldır
            const remaining = projects.filter(p => p.id !== projectId);
            setProjects(remaining);

            // Eğer silinen proje seçiliyse, kalan ilk projeye geç (yoksa null)
            if (selectedProjectId === projectId) {
                setSelectedProjectId(remaining.length > 0 ? remaining[0].id : null);
                setTasks([]);
            }
        } catch (err) {
            alert('Proje silinemedi: ' + err);
        }
    };

    // 3. GÖRSELLİK BÖLÜMÜ
    return (
        <div style={{ display: 'flex', fontFamily: 'sans-serif', minHeight: '100vh' }}>

            {/* SOL MENÜ: Proje Listesi */}
            <div style={{ width: '220px', backgroundColor: '#1e1e2e', color: 'white', padding: '24px 16px', flexShrink: 0 }}>
                <h2 style={{ fontSize: '14px', textTransform: 'uppercase', letterSpacing: '1px', color: '#888', marginBottom: '16px' }}>Projeler</h2>
                {projects.map(project => (
                    <div
                        key={project.id}
                        onClick={() => setSelectedProjectId(project.id)}
                        style={{
                            padding: '10px 12px',
                            borderRadius: '6px',
                            cursor: 'pointer',
                            marginBottom: '6px',
                            backgroundColor: selectedProjectId === project.id ? '#4f46e5' : 'transparent',
                            fontWeight: selectedProjectId === project.id ? 'bold' : 'normal',
                            transition: 'background-color 0.2s',
                            display: 'flex',
                            justifyContent: 'space-between',
                            alignItems: 'center',
                        }}
                    >
                        <span>📁 {project.name}</span>
                        {/* Silme butonu — sadece ProjectManager/Admin görür, tıklanma olayı projeye geçmesin diye stopPropagation */}
                        {(userRole.includes('ProjectManager') || userRole.includes('Admin')) && (
                            <span
                                onClick={e => { e.stopPropagation(); handleDeleteProject(project.id); }}
                                style={{ color: '#ff4d4f', fontSize: '16px', fontWeight: 'bold', lineHeight: 1 }}
                                title='Projeyi sil'
                            >
                                ×
                            </span>
                        )}
                    </div>
                ))}

                {/* Sadece ProjectManager veya Admin projeyi görebilir */}
                {(userRole.includes('ProjectManager') || userRole.includes('Admin')) && (
                    <div style={{ marginTop: '24px' }}>
                        <button
                            onClick={() => setIsProjectFormOpen(!isProjectFormOpen)}
                            style={{ width: '100%', padding: '8px', backgroundColor: '#4f46e5', color: 'white', border: 'none', borderRadius: '6px', cursor: 'pointer', fontSize: '13px' }}
                        >
                            {isProjectFormOpen ? '× İptal' : '+ Yeni Proje'}
                        </button>

                        {isProjectFormOpen && (
                            <form onSubmit={handleCreateProject} style={{ marginTop: '12px', display: 'flex', flexDirection: 'column', gap: '8px' }}>
                                <input
                                    placeholder='Proje adı'
                                    value={newProjectName}
                                    onChange={e => setNewProjectName(e.target.value)}
                                    required
                                    style={{ padding: '6px', borderRadius: '4px', border: 'none', fontSize: '13px' }}
                                />
                                <input
                                    placeholder='Açıklama'
                                    value={newProjectDesc}
                                    onChange={e => setNewProjectDesc(e.target.value)}
                                    style={{ padding: '6px', borderRadius: '4px', border: 'none', fontSize: '13px' }}
                                />
                                <input
                                    type='date'
                                    value={newProjectStartDate}
                                    onChange={e => setNewProjectStartDate(e.target.value)}
                                    required
                                    style={{ padding: '6px', borderRadius: '4px', border: 'none', fontSize: '13px' }}
                                />
                                <button
                                    type='submit'
                                    style={{ padding: '8px', backgroundColor: '#22c55e', color: 'white', border: 'none', borderRadius: '4px', cursor: 'pointer', fontSize: '13px' }}
                                >
                                    Oluştur
                                </button>
                            </form>
                        )}
                    </div>
                )}
            </div>

            {/* SAĞ ALAN: Kanban Panosu */}
            <div style={{ flex: 1, padding: '40px', display: 'flex', flexDirection: 'column' }}>
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '20px' }}>
                    <h1 style={{ margin: 0, color: '#172b4d' }}>📋 {projects.find(p => p.id === selectedProjectId)?.name || 'Proje Seçin'}</h1>
                    
                    {/* YENİ: Profil Rozeti */}
                    {userProfile && (
                        <div style={{ 
                            display: 'flex', alignItems: 'center', gap: '12px', 
                            backgroundColor: '#fff', padding: '6px 16px 6px 6px', 
                            borderRadius: '50px', boxShadow: '0 2px 8px rgba(0,0,0,0.08)',
                            border: '1px solid #f0f0f0'
                        }}>
                            <div style={{ 
                                width: '38px', height: '38px', borderRadius: '50%', 
                                backgroundColor: '#4f46e5', color: 'white', 
                                display: 'flex', alignItems: 'center', justifyContent: 'center', 
                                fontWeight: 'bold', fontSize: '15px', letterSpacing: '1px' 
                            }}>
                                {userProfile.firstName.charAt(0).toUpperCase()}{userProfile.lastName.charAt(0).toUpperCase()}
                            </div>
                            <div style={{ display: 'flex', flexDirection: 'column' }}>
                                <span style={{ fontWeight: 'bold', color: '#172b4d', fontSize: '14px', lineHeight: '1.2' }}>
                                    {userProfile.firstName} {userProfile.lastName}
                                </span>
                                <span style={{ fontSize: '12px', color: '#5e6c84', fontWeight: '500' }}>
                                    {userProfile.role}
                                </span>
                            </div>
                        </div>
                    )}
                </div>

                {/* Üst Butonlar Grubu */}
                <div style={{ display: 'flex', gap: '10px', marginBottom: '20px' }}>
                    {/* YENİ EKLENEN: Sadece Admin'e görünen buton */}
                    {userRole.includes('Admin') && (
                        <button
                            onClick={onAdminClick}
                            style={{ padding: '8px 16px', backgroundColor: '#722ed1', color: 'white', border: 'none', borderRadius: '4px', cursor: 'pointer', fontWeight: 'bold' }}>
                            🛡️ Yönetim Paneli
                        </button>
                    )}
                    <button
                        onClick={() => { localStorage.removeItem('token'); window.location.reload(); }}
                        style={{ padding: '8px 16px', backgroundColor: '#ff4d4f', color: 'white', border: 'none', borderRadius: '4px', cursor: 'pointer' }}>
                        Çıkış Yap
                    </button>

                    {/* SADECE userRole dizisi içinde "ProjectManager" veya "Admin" VARSA bu butonu ekrana çiz! */}
                    {(userRole.includes("ProjectManager") || userRole.includes("Admin")) && (
                        <button
                            onClick={() => setIsFormOpen(!isFormOpen)}
                            style={{ padding: '8px 16px', backgroundColor: '#1890ff', color: 'white', border: 'none', borderRadius: '4px', cursor: 'pointer' }}>
                            {isFormOpen ? "Kapat" : "+ Yeni Görev Ekle"}
                        </button>
                    )}
                </div>

                {/* YENİ EKLENEN: Eğer isFormOpen 'true' ise bu formu ekrana çiz! */}
                {isFormOpen && (
                    <form onSubmit={handleCreateTask} style={{ marginBottom: '20px', padding: '20px', border: '1px solid #ccc', borderRadius: '8px', backgroundColor: '#f9f9f9', maxWidth: '400px' }}>
                        <div style={{ marginBottom: '10px' }}>
                            <label>Başlık:</label><br />
                            <input
                                type="text"
                                required
                                value={newTaskTitle}
                                onChange={(e) => setNewTaskTitle(e.target.value)}
                                style={{ width: '100%', padding: '8px', marginTop: '5px' }}
                            />
                        </div>
                        <div style={{ marginBottom: '10px' }}>
                            <label>Açıklama:</label><br />
                            <textarea
                                required
                                value={newTaskDesc}
                                onChange={(e) => setNewTaskDesc(e.target.value)}
                                style={{ width: '100%', padding: '8px', marginTop: '5px', height: '60px' }}
                            />
                        </div>
                        <div style={{ marginBottom: '10px' }}>
                            <label>Görevi Ata (Kime?):</label><br />
                            <select
                                value={newAssignedUserId}
                                onChange={(e) => setNewAssignedUserId(e.target.value)}
                                style={{ width: '100%', padding: '8px', marginTop: '5px' }}>
                                <option value="">-- Kimseye Atanmadı --</option>

                                {/* Kullanıcılar listesini dönerek her birini bir seçenek (option) yapıyoruz */}
                                {users.map(user => (
                                    <option key={user.id} value={user.id}>
                                        {/* firstName/lastName varsa "Ali Yılmaz", yoksa email'i göster */}
                                        {user.firstName && user.lastName
                                            ? `${user.firstName} ${user.lastName}`
                                            : user.email}
                                    </option>
                                ))}
                            </select>
                        </div>
                        <button type="submit" style={{ padding: '8px 16px', backgroundColor: '#52c41a', color: 'white', border: 'none', borderRadius: '4px', cursor: 'pointer' }}>
                            Kaydet
                        </button>
                    </form>
                )}

                {loading && <p>Görevler yükleniyor, lütfen bekleyin...</p>}
                {error && <p style={{ color: 'red' }}>Hata: {error}</p>}

                {/* Eski Görev Çizim Alanı Aynı Kalacak */}
                {/* --- KANBAN PANOSU (3 KOLONLU YAPI) --- */}
                <div style={{ display: 'flex', gap: '20px', alignItems: 'flex-start', marginTop: '30px' }}>

                    {/* 1. KOLON: YAPILACAKLAR (ToDo - Status: 1) */}
                    <div className="kanban-column" style={{ flex: 1, backgroundColor: '#ebecf0', padding: '15px', borderRadius: '8px', minHeight: '300px' }}>
                        <h2 style={{ fontSize: '16px', color: '#172b4d', marginBottom: '15px', textTransform: 'uppercase' }}>
                            📋 Yapılacaklar ({tasks.filter(t => t.status === 1).length})
                        </h2>
                        <div style={{ display: 'flex', flexDirection: 'column', gap: '10px' }}>
                            {tasks.filter(task => task.status === 1).map((task) => (
                                <div key={task.id} className="task-card" style={{ backgroundColor: 'white', padding: '15px', borderRadius: '6px', boxShadow: '0 1px 3px rgba(0,0,0,0.1)' }}>
                                    <h4 style={{ margin: '0 0 8px 0', color: '#172b4d' }}>{task.title}</h4>
                                    {/* YENİ EKLENEN: Atanan kişiyi bul ve E-postasını yazdır */}
                                    <p style={{ margin: '0 0 10px 0', fontSize: '12px', fontWeight: 'bold', color: '#d46b08', backgroundColor: '#fff7e6', padding: '4px', borderRadius: '4px', display: 'inline-block' }}>
                                        👤 {task.assignedUserName || 'Atanmadı'}
                                    </p>
                                    <p style={{ margin: 0, fontSize: '13px', color: '#5e6c84' }}>{task.description}</p>
                                    
                                    {/* YENİ İŞ KURALI: Employee sadece kendine atanmışı (veya boşu) ilerletebilir */}
                                    {(userRole.includes("Admin") || userRole.includes("ProjectManager") || task.assignedUserId === currentUserId || task.assignedUserId == null) && (
                                        <button
                                            onClick={() => handleStatusChange(task, 2)}
                                            style={{ backgroundColor: '#1890ff', color: 'white', border: 'none', padding: '5px 10px', borderRadius: '4px', cursor: 'pointer', fontSize: '12px', width: '100%', marginTop: '10px' }}>
                                            Başla (Sağa Kaydır) ➡️
                                        </button>
                                    )}
                                    {/* SİLME BUTONU: Sadece yetkili/sahibi silebilir */}
                                    {(userRole.includes("Admin") || userRole.includes("ProjectManager") || task.assignedUserId === currentUserId || task.assignedUserId == null) && (
                                        <button
                                            onClick={() => handleDeleteTask(task.id)}
                                            style={{ backgroundColor: '#ff4d4f', color: 'white', border: 'none', padding: '5px 10px', borderRadius: '4px', cursor: 'pointer', fontSize: '12px', width: '100%', marginTop: '10px' }}>
                                            🗑️ Sil
                                        </button>
                                    )}
                                </div>
                            ))}
                        </div>
                    </div>

                    {/* 2. KOLON: DEVAM EDENLER (InProgress - Status: 2) */}
                    <div className="kanban-column" style={{ flex: 1, backgroundColor: '#ebecf0', padding: '15px', borderRadius: '8px', minHeight: '300px' }}>
                        <h2 style={{ fontSize: '16px', color: '#172b4d', marginBottom: '15px', textTransform: 'uppercase' }}>
                            ⏳ Devam Edenler ({tasks.filter(t => t.status === 2).length})
                        </h2>
                        <div style={{ display: 'flex', flexDirection: 'column', gap: '10px' }}>
                            {tasks.filter(task => task.status === 2).map((task) => (
                                <div key={task.id} className="task-card" style={{ backgroundColor: 'white', padding: '15px', borderRadius: '6px', boxShadow: '0 1px 3px rgba(0,0,0,0.1)' }}>
                                    <h4 style={{ margin: '0 0 8px 0', color: '#172b4d' }}>{task.title}</h4>
                                    {/* YENİ EKLENEN: Atanan kişiyi bul ve E-postasını yazdır */}
                                    <p style={{ margin: '0 0 10px 0', fontSize: '12px', fontWeight: 'bold', color: '#d46b08', backgroundColor: '#fff7e6', padding: '4px', borderRadius: '4px', display: 'inline-block' }}>
                                        👤 {task.assignedUserName || 'Atanmadı'}
                                    </p>
                                    <p style={{ margin: 0, fontSize: '13px', color: '#5e6c84' }}>{task.description}</p>
                                    
                                    {/* YENİ İŞ KURALI: Sadece yetkili/sahibi durumu değiştirebilir */}
                                    {(userRole.includes("Admin") || userRole.includes("ProjectManager") || task.assignedUserId === currentUserId || task.assignedUserId == null) && (
                                        <div style={{ display: 'flex', gap: '5px', marginTop: '10px' }}>
                                            <button
                                                onClick={() => handleStatusChange(task, 1)}
                                                style={{ flex: 1, backgroundColor: '#faad14', color: 'white', border: 'none', padding: '5px', borderRadius: '4px', cursor: 'pointer', fontSize: '12px' }}>
                                                ⬅️ Geri Al
                                            </button>
                                            <button
                                                onClick={() => handleStatusChange(task, 3)}
                                                style={{ flex: 1, backgroundColor: '#52c41a', color: 'white', border: 'none', padding: '5px', borderRadius: '4px', cursor: 'pointer', fontSize: '12px' }}>
                                                Bitir ➡️
                                            </button>
                                        </div>
                                    )}

                                    {/* SİLME BUTONU: Sadece yetkili/sahibi silebilir */}
                                    {(userRole.includes("Admin") || userRole.includes("ProjectManager") || task.assignedUserId === currentUserId || task.assignedUserId == null) && (
                                        <button
                                            onClick={() => handleDeleteTask(task.id)}
                                            style={{ backgroundColor: '#ff4d4f', color: 'white', border: 'none', padding: '5px 10px', borderRadius: '4px', cursor: 'pointer', fontSize: '12px', width: '100%', marginTop: '10px' }}>
                                            🗑️ Sil
                                        </button>
                                    )}
                                </div>
                            ))}
                        </div>
                    </div>

                    {/* 3. KOLON: BİTENLER (Done - Status: 3) */}
                    <div className="kanban-column" style={{ flex: 1, backgroundColor: '#ebecf0', padding: '15px', borderRadius: '8px', minHeight: '300px' }}>
                        <h2 style={{ fontSize: '16px', color: '#172b4d', marginBottom: '15px', textTransform: 'uppercase' }}>
                            ✅ Bitenler ({tasks.filter(t => t.status === 3).length})
                        </h2>
                        <div style={{ display: 'flex', flexDirection: 'column', gap: '10px' }}>
                            {tasks.filter(task => task.status === 3).map((task) => (
                                <div key={task.id} className="task-card" style={{ backgroundColor: 'white', padding: '15px', borderRadius: '6px', boxShadow: '0 1px 3px rgba(0,0,0,0.1)' }}>
                                    <h4 style={{ margin: '0 0 8px 0', color: '#172b4d', textDecoration: 'line-through' }}>{task.title}</h4>
                                    {/* YENİ EKLENEN: Atanan kişiyi bul ve E-postasını yazdır */}
                                    <p style={{ margin: '0 0 10px 0', fontSize: '12px', fontWeight: 'bold', color: '#d46b08', backgroundColor: '#fff7e6', padding: '4px', borderRadius: '4px', display: 'inline-block' }}>
                                        👤 {task.assignedUserName || 'Atanmadı'}
                                    </p>
                                    <p style={{ margin: 0, fontSize: '13px', color: '#5e6c84' }}>{task.description}</p>
                                    
                                    {/* YENİ İŞ KURALI: Sadece yetkili/sahibi geriye döndürebilir */}
                                    {(userRole.includes("Admin") || userRole.includes("ProjectManager") || task.assignedUserId === currentUserId || task.assignedUserId == null) && (
                                        <button
                                            onClick={() => handleStatusChange(task, 2)}
                                            style={{ backgroundColor: '#faad14', color: 'white', border: 'none', padding: '5px 10px', borderRadius: '4px', cursor: 'pointer', fontSize: '12px', width: '100%', marginTop: '10px' }}>
                                            ⬅️ Devam Et
                                        </button>
                                    )}

                                    {/* SİLME BUTONU: Sadece yetkili/sahibi silebilir */}
                                    {(userRole.includes("Admin") || userRole.includes("ProjectManager") || task.assignedUserId === currentUserId || task.assignedUserId == null) && (
                                        <button
                                            onClick={() => handleDeleteTask(task.id)}
                                            style={{ backgroundColor: '#ff4d4f', color: 'white', border: 'none', padding: '5px 10px', borderRadius: '4px', cursor: 'cursor', fontSize: '12px', width: '100%', marginTop: '10px' }}>
                                            🗑️ Sil
                                        </button>
                                    )}
                                </div>
                            ))}
                        </div>
                    </div>

                </div>
                {/* ------------------------------------------- */}
            </div> {/* SAĞ ALAN sonu */}
        </div>
    );
}

export default DashboardPage;