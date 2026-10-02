import React, { useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { Link, useNavigate } from 'react-router-dom';
import { loginUser, clearAuthError } from '../features/auth/authSlice';

export default function LoginPage() {
  const dispatch = useDispatch();
  const navigate = useNavigate();
  const { loading, error } = useSelector((state) => state.auth);

  // Quản lý state cho form dữ liệu đăng nhập
  const [formData, setFormData] = useState({
    emailOrUserName: '',
    password: '',
  });

  // State lỗi validation trực tiếp tại client
  const [validationErrors, setValidationErrors] = useState({});

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData((prev) => ({ ...prev, [name]: value }));
    // Xóa lỗi validation của field khi người dùng đang nhập lại
    if (validationErrors[name]) {
      setValidationErrors((prev) => ({ ...prev, [name]: '' }));
    }
    if (error) {
      dispatch(clearAuthError());
    }
  };

  const validate = () => {
    const errors = {};
    if (!formData.emailOrUserName.trim()) {
      errors.emailOrUserName = 'Vui lòng nhập Email hoặc Tên đăng nhập';
    }
    if (!formData.password) {
      errors.password = 'Vui lòng nhập mật khẩu';
    }
    setValidationErrors(errors);
    return Object.keys(errors).length === 0;
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!validate()) return;

    // Gửi action đăng nhập qua Redux thunk
    const resultAction = await dispatch(loginUser(formData));
    if (loginUser.fulfilled.match(resultAction)) {
      // Đăng nhập thành công -> điều hướng sang trang chat chính
      navigate('/chat');
    }
  };

  return (
    <div style={styles.container}>
      <div style={styles.card}>
        <h2 style={styles.title}>Đăng Nhập</h2>
        <p style={styles.subtitle}>Chào mừng bạn quay trở lại với Real-Time Chat</p>

        {/* Hiển thị lỗi tổng quát trả về từ server (ExceptionMiddleware) */}
        {error && (
          <div style={styles.alertError}>
            <span>⚠️ {error}</span>
          </div>
        )}

        <form onSubmit={handleSubmit} style={styles.form}>
          <div style={styles.inputGroup}>
            <label style={styles.label}>Tên đăng nhập hoặc Email</label>
            <input
              type="text"
              name="emailOrUserName"
              value={formData.emailOrUserName}
              onChange={handleChange}
              placeholder="nhap_username hoac email@example.com"
              style={{
                ...styles.input,
                borderColor: validationErrors.emailOrUserName ? '#ef4444' : '#334155',
              }}
            />
            {validationErrors.emailOrUserName && (
              <span style={styles.errorText}>{validationErrors.emailOrUserName}</span>
            )}
          </div>

          <div style={styles.inputGroup}>
            <label style={styles.label}>Mật khẩu</label>
            <input
              type="password"
              name="password"
              value={formData.password}
              onChange={handleChange}
              placeholder="••••••••"
              style={{
                ...styles.input,
                borderColor: validationErrors.password ? '#ef4444' : '#334155',
              }}
            />
            {validationErrors.password && (
              <span style={styles.errorText}>{validationErrors.password}</span>
            )}
          </div>

          <button
            type="submit"
            disabled={loading}
            style={{
              ...styles.button,
              opacity: loading ? 0.7 : 1,
              cursor: loading ? 'not-allowed' : 'pointer',
            }}
          >
            {loading ? 'Đang xác thực...' : 'Đăng Nhập'}
          </button>
        </form>

        <p style={styles.footerText}>
          Chưa có tài khoản?{' '}
          <Link to="/register" style={styles.link}>
            Đăng ký ngay
          </Link>
        </p>
      </div>
    </div>
  );
}

// Styling inline sạch sẽ, hiện đại theo phong cách Dark Mode Card
const styles = {
  container: {
    minHeight: '100vh',
    display: 'flex',
    alignItems: 'center',
    justifyContent: 'center',
    backgroundColor: '#0f172a',
    padding: '20px',
  },
  card: {
    backgroundColor: '#1e293b',
    borderRadius: '14px',
    boxShadow: '0 20px 25px -5px rgba(0, 0, 0, 0.5), 0 8px 10px -6px rgba(0, 0, 0, 0.5)',
    width: '100%',
    maxWidth: '420px',
    padding: '36px',
    color: '#f8fafc',
  },
  title: {
    fontSize: '26px',
    fontWeight: '700',
    marginBottom: '8px',
    textAlign: 'center',
    color: '#38bdf8',
  },
  subtitle: {
    fontSize: '14px',
    color: '#94a3b8',
    textAlign: 'center',
    marginBottom: '24px',
  },
  alertError: {
    backgroundColor: 'rgba(239, 68, 68, 0.15)',
    border: '1px solid #ef4444',
    color: '#fca5a5',
    padding: '12px',
    borderRadius: '8px',
    fontSize: '14px',
    marginBottom: '20px',
  },
  form: {
    display: 'flex',
    flexDirection: 'column',
    gap: '16px',
  },
  inputGroup: {
    display: 'flex',
    flexDirection: 'column',
    gap: '6px',
  },
  label: {
    fontSize: '13px',
    fontWeight: '600',
    color: '#cbd5e1',
  },
  input: {
    padding: '11px 14px',
    borderRadius: '8px',
    border: '1px solid #334155',
    backgroundColor: '#0f172a',
    color: '#f8fafc',
    fontSize: '14px',
    outline: 'none',
  },
  errorText: {
    fontSize: '12px',
    color: '#ef4444',
  },
  button: {
    marginTop: '8px',
    padding: '12px',
    backgroundColor: '#0284c7',
    color: '#ffffff',
    border: 'none',
    borderRadius: '8px',
    fontSize: '15px',
    fontWeight: '600',
    transition: 'background-color 0.2s',
  },
  footerText: {
    marginTop: '24px',
    fontSize: '14px',
    textAlign: 'center',
    color: '#94a3b8',
  },
  link: {
    color: '#38bdf8',
    textDecoration: 'none',
    fontWeight: '600',
  },
};
