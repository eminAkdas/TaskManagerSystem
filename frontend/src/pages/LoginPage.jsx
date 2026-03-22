// React'in hafıza yöneticisini (useState) içeri alıyoruz
import { useState } from 'react';
// Az önce yazdığımız garsonu (servisi) içeri alıyoruz
import { login } from '../Services/authService';

function LoginPage({ onSwitchToRegister }) {
    // 1. HAFIZA BÖLÜMÜ (STATE)
    // [okuduğumuz_değer, değiştireceğimiz_fonksiyon] = useState(başlangıç_değeri)
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [error, setError] = useState('');

    // 2. AKSİYON BÖLÜMÜ (Butona basılınca ne olacak?)
    const handleLoginSubmit = async (e) => {
        // HTML formlarının varsayılan huyu sayfayı yenilemektir. Bunu engelliyoruz!
        e.preventDefault();
        setError(''); // Yeni denemede eski hatayı temizle

        try {
            // Garsona (login fonksiyonuna) elimizdeki email ve şifreyi verip "Git getir" diyoruz.
            const data = await login(email, password);

            // Gelen cevabın içindeki sihirli bilekliği (token) alıp tarayıcıya (LocalStorage) kaydediyoruz!

            // YENİ EKLENEN KISIM: Token'ı tarayıcının kasasına (LocalStorage) 'token' adıyla kaydediyoruz!
            localStorage.setItem('token', data.token);



            // Sayfayı yenileyerek App.jsx'in baştan çalışmasını ve kasadaki (LocalStorage) yeni token'ı görmesini sağlıyoruz.
            window.location.reload();
        } catch (errMessage) {
            // Eğer garson C#'tan fırça yiyip (401/500) dönerse, hatayı hafızaya al ki ekrana çizelim
            setError(errMessage);
        }
    };

    // 3. GÖRSELLİK BÖLÜMÜ (Ekrana ne çizilecek?)
    return (
        <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: '100vh', backgroundColor: '#f4f5f7' }}>
            <div className="kanban-column" style={{ width: '100%', maxWidth: '420px', backgroundColor: 'white', padding: '40px', borderRadius: '12px' }}>
                <h2 style={{ textAlign: 'center', color: '#172b4d', marginBottom: '30px' }}>TaskManager Giriş</h2>

                {/* Eğer hafızada bir 'error' varsa, onu kırmızı bir kutu içinde göster */}
                {error && <div style={{ color: '#cf1322', backgroundColor: '#fff1f0', border: '1px solid #ffa39e', padding: '10px', borderRadius: '4px', marginBottom: '15px', fontSize: '14px' }}>{error}</div>}

                {/* Form gönderildiğinde (onSubmit) bizim yazdığımız fonksiyon çalışacak */}
                <form onSubmit={handleLoginSubmit}>
                    <div style={{ marginBottom: '15px' }}>
                        <label style={{ fontSize: '14px', fontWeight: '500', color: '#5e6c84' }}>E-posta Adresi</label> <br />
                        <input
                            type="email"
                            value={email}
                            onChange={(e) => setEmail(e.target.value)}
                            required
                            style={{ width: '100%', padding: '10px', marginTop: '5px', fontSize: '15px' }}
                            placeholder="mail@sirket.com"
                        />
                    </div>

                    <div style={{ marginBottom: '20px' }}>
                        <label style={{ fontSize: '14px', fontWeight: '500', color: '#5e6c84' }}>Şifre</label> <br />
                        <input
                            type="password"
                            value={password}
                            onChange={(e) => setPassword(e.target.value)}
                            required
                            style={{ width: '100%', padding: '10px', marginTop: '5px', fontSize: '15px' }}
                            placeholder="••••••••"
                        />
                    </div>

                    <button type="submit" style={{ width: '100%', padding: '12px', backgroundColor: '#4f46e5', color: 'white', border: 'none', borderRadius: '6px', fontWeight: 'bold', fontSize: '16px', cursor: 'pointer' }}>
                        Giriş Yap
                    </button>
                    
                    {/* YENİ EKLENEN: Register sayfasına gidiş butonu */}
                    <div style={{ marginTop: '25px', textAlign: 'center', borderTop: '1px solid #f0f0f0', paddingTop: '20px' }}>
                        <p style={{ margin: '0 0 10px 0', color: '#5e6c84', fontSize: '14px' }}>Hesabın yok mu?</p>
                        <button
                            onClick={onSwitchToRegister}
                            type="button"
                            style={{ padding: '8px 16px', backgroundColor: '#fff', color: '#4f46e5', border: '1px solid #4f46e5', borderRadius: '6px', cursor: 'pointer', fontWeight: '600' }}>
                            Yeni Hesap Oluştur
                        </button>
                    </div>
                </form>
            </div>
        </div>
    );
}

export default LoginPage;