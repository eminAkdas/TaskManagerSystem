import { useState } from 'react';
import LoginPage from './pages/LoginPage';
import DashboardPage from './pages/DashboardPage';
import RegisterPage from './pages/RegisterPage'; // Yeni sayfamızı içeri alıyoruz
import AdminPanel from './pages/AdminPanel'; // Yönetim Paneli

function App() {
  const isAuthenticated = !!localStorage.getItem('token');

  // Ekranda Login mi yoksa Register mı görünecek? (Başlangıçta true = Login)
  const [isLoginMode, setIsLoginMode] = useState(true);

  // Uygulamanın neresindeyiz? (dashboard veya admin)
  const [currentView, setCurrentView] = useState('dashboard');

  // Eğer giriş yapılmışsa direkt panoyu göster
  if (isAuthenticated) {
    if (currentView === 'admin') {
      return <AdminPanel onBack={() => setCurrentView('dashboard')} />;
    }
    return <DashboardPage onAdminClick={() => setCurrentView('admin')} />;
  }

  // Eğer giriş yapılmamışsa, isLoginMode durumuna göre sayfayı değiştir
  return (
    <div>
      {isLoginMode ? (
        <LoginPage onSwitchToRegister={() => setIsLoginMode(false)} />
      ) : (
        <RegisterPage onSwitchToLogin={() => setIsLoginMode(true)} />
      )}
    </div>
  );
}

export default App;