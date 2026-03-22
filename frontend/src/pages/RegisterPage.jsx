import { useState } from 'react';
import { registerUser } from '../Services/authService';

function RegisterPage({ onSwitchToLogin }) {
    // Her form alanı için ayrı bir state.
    // React'te "controlled component" yaklaşımı: her input'un değeri
    // state'te tutulur, onChange ile senkronize edilir.
    const [firstName, setFirstName] = useState('');
    const [lastName, setLastName] = useState('');
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [confirmPassword, setConfirmPassword] = useState('');
    const [error, setError] = useState('');

    const handleRegisterSubmit = async (e) => {
        e.preventDefault();
        setError('');

        if (password !== confirmPassword) {
            setError("Şifreler birbiriyle uyuşmuyor!");
            return;
        }

        try {
            // Artık 4 parametre gönderiyoruz — authService bunu C#'a iletecek.
            await registerUser(firstName, lastName, email, password);
            alert("Kayıt Başarılı! Şimdi giriş yapabilirsiniz.");
            onSwitchToLogin();
        } catch (errMessage) {
            setError(errMessage);
        }
    };

    return (
        <div style={{ padding: '50px', maxWidth: '400px', margin: '0 auto', fontFamily: 'sans-serif' }}>
            <h2>Yeni Hesap Oluştur</h2>

            {error && <div style={{ color: 'red', marginBottom: '15px', backgroundColor: '#ffe6e6', padding: '10px', borderRadius: '4px' }}>{error}</div>}

            <form onSubmit={handleRegisterSubmit} style={{ display: 'flex', flexDirection: 'column', gap: '15px' }}>

                {/* YENİ: Ad alanı */}
                <div>
                    <label>Ad:</label>
                    <input
                        type="text"
                        value={firstName}
                        onChange={(e) => setFirstName(e.target.value)}
                        required
                        style={{ width: '100%', padding: '8px', marginTop: '5px' }}
                    />
                </div>

                {/* YENİ: Soyad alanı */}
                <div>
                    <label>Soyad:</label>
                    <input
                        type="text"
                        value={lastName}
                        onChange={(e) => setLastName(e.target.value)}
                        required
                        style={{ width: '100%', padding: '8px', marginTop: '5px' }}
                    />
                </div>

                <div>
                    <label>E-posta:</label>
                    <input
                        type="email"
                        value={email}
                        onChange={(e) => setEmail(e.target.value)}
                        required
                        style={{ width: '100%', padding: '8px', marginTop: '5px' }}
                    />
                </div>

                <div>
                    <label>Şifre:</label>
                    <input
                        type="password"
                        value={password}
                        onChange={(e) => setPassword(e.target.value)}
                        required
                        style={{ width: '100%', padding: '8px', marginTop: '5px' }}
                    />
                </div>

                <div>
                    <label>Şifre (Tekrar):</label>
                    <input
                        type="password"
                        value={confirmPassword}
                        onChange={(e) => setConfirmPassword(e.target.value)}
                        required
                        style={{ width: '100%', padding: '8px', marginTop: '5px' }}
                    />
                </div>

                <button type="submit" style={{ padding: '10px 20px', backgroundColor: '#52c41a', color: 'white', border: 'none', borderRadius: '4px', cursor: 'pointer', fontSize: '16px' }}>
                    Kayıt Ol
                </button>
            </form>

            <div style={{ marginTop: '20px', textAlign: 'center' }}>
                <p style={{ margin: '0 0 10px 0' }}>Zaten bir hesabın var mı?</p>
                <button
                    onClick={onSwitchToLogin}
                    style={{ padding: '8px 16px', backgroundColor: '#f0f0f0', border: '1px solid #ccc', borderRadius: '4px', cursor: 'pointer' }}>
                    Giriş Ekranına Dön
                </button>
            </div>
        </div>
    );
}

export default RegisterPage;