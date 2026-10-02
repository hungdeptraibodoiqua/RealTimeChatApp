import React, { useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { Link, useNavigate } from 'react-router-dom';
import { registerUser, clearAuthError } from '../features/auth/authSlice';

export default function RegisterPage() {
  const dispatch = useDispatch();
  const navigate = useNavigate();
  const { loading, error } = useSelector((state) => state.auth);

  const [formData, setFormData] = useState({
    username: '',
    email: '',
    displayName: '',
    password: '',
    confirmPassword: '',
  });

  const [validationErrors, setValidationErrors] = useState({});
  const [successMessage, setSuccessMessage] = useState('');

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData((prev) => ({ ...prev, [name]: value }));
    if (validationErrors[name]) {
      setValidationErrors((prev) => ({ ...prev, [name]: '' }));
    }
    if (error) {
      dispatch(clearAuthError());
    }
  };

  // Kiểm tra tính hợp lệ của form đăng ký ở phía client
  const validate = () => {
    const errors = {};
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

    if (!formData.username.trim()) {
      errors.username = 'Tên tài khoản không được bỏ trống';
    } else if (formData.username.length < 3) {
      errors.username = 'Tên tài khoản phải có ít nhất 3 ký tự';
    }

    if (!formData.email.trim()) {
      errors.email = 'Email không được bỏ trống';
    } else if (!emailRegex.test(formData.email.trim())) {
      errors.email = 'Định dạng email không hợp lệ (vd: user@example.com)';
    }

    if (!formData.displayName.trim()) {
      errors.displayName = 'Tên hiển thị không được bỏ trống';
    }

    if (!formData.password) {
      errors.password = 'Mật khẩu không được bỏ trống';
    } else if (formData.password.length < 6) {
      errors.password = 'Mật khẩu phải có ít nhất 6 ký tự';
    }

    if (formData.password !== formData.confirmPassword) {
      errors.confirmPassword = 'Mật khẩu xác nhận không khớp';
    }

    setValidationErrors(errors);
    return Object.keys(errors).length === 0;
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!validate()) return;

    // Gửi payload loại bỏ trường confirmPassword tới backend API
    const { confirmPassword, ...registerPayload } = formData;
    const resultAction = await dispatch(registerUser(registerPayload));

    if (registerUser.fulfilled.match(resultAction)) {
      setSuccessMessage('Đăng ký tài khoản thành công! Đang chuyển hướng sang trang đăng nhập...');
      setTimeout(() => {
        navigate('/login');
      }, 1500);
    }
  };

  return (
    <div style={styles.container}>
      <div style={styles.card}>
        <h2 style={styles.title}>Tạo Tài Khoản</h2>
        <p style={styles.subtitle}>Gia nhập không gian trò chuyện Real-Time ngay hôm nay</p>

        {successMessage && (
          <div style={styles.alertSuccess}>
            <span>✅ {successMessage}</span>
          </div>
        )}

        {error && (
          <div style={styles.alertError}>
            <span>⚠️ {error}</span>
          </div>
        )}

        <form onSubmit={handleSubmit} style={styles.form}>
          <div style={styles.inputGroup}>
            <label style={styles.label}>Tên đăng nhập (Username)</label>
            <input
              type="text"
              name="username"
              value={formData.username}
              onChange={handleChange}
              placeholder="vd: hung_dep_trai"
              style={{
                ...styles.input,
                borderColor: validationErrors.username ? '#ef4444' : '#334155',
              }}
            />
            {validationErrors.username && (
              <span style={styles.errorText}>{validationErrors.username}</span>
            )}
          </div>

          <div style={styles.inputGroup}>
            <label style={styles.label}>Địa chỉ Email</label>
            <input
              type="email"
              name="email"
              value={formData.email}
              onChange={handleChange}
              placeholder="vd: user@example.com"
              style={{
                ...styles.input,
                borderColor: validationErrors.email ? '#ef4444' : '#334155',
              }}
            />
            {validationErrors.email && (
              <span style={styles.errorText}>{validationErrors.email}</span>
            )}
          </div>

          <div style={styles.inputGroup}>
            <label style={styles.label}>Tên hiển thị (Display Name)</label>
            <input
              type="text"
              name="displayName"
              value={formData.displayName}
              onChange={handleChange}
              placeholder="vd: Hùng Đẹp Trai"
              style={{
                ...styles.input,
                borderColor: validationErrors.displayName ? '#ef4444' : '#334155',
              }}
            />
            {validationErrors.displayName && (
              <span style={styles.errorText}>{validationErrors.displayName}</span>
            )}
          </div>

          <div style={styles.inputGroup}>
            <label style={styles.label}>Mật khẩu</label>
            <input
              type="password"
              name="password"
              value={formData.password}
              onChange={handleChange}
              placeholder="Tối thiểu 6 ký tự"
              style={{
                ...styles.input,
                borderColor: validationErrors.password ? '#ef4444' : '#334155',
              }}
            />
            {validationErrors.password && (
              <span style={styles.errorText}>{validationErrors.password}</span>
            )}
          </div>

          <div style={styles.inputGroup}>
            <label style={styles.label}>Xác nhận mật khẩu</label>
            <input
              type="password"
              name="confirmPassword"
              value={formData.confirmPassword}
              onChange={handleChange}
              placeholder="Nhập lại mật khẩu"
              style={{
                ...styles.input,
                borderColor: validationErrors.confirmPassword ? '#ef4444' : '#334155',
              }}
            />
            {validationErrors.confirmPassword && (
              <span style={styles.errorText}>{validationErrors.confirmPassword}</span>
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
            {loading ? 'Đang tạo tài khoản...' : 'Đăng Ký'}
          </button>
        </form>

        <p style={styles.footerText}>
          Đã có tài khoản?{' '}
          <Link to="/login" style={styles.link}>
            Đăng nhập
          </Link>
        </p>
      </div>
    </div>
  );
}

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
    maxWidth: '460px',
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
    marginBottom: '20px',
  },
  alertSuccess: {
    backgroundColor: 'rgba(34, 197, 94, 0.15)',
    border: '1px solid #22c55e',
    color: '#86efac',
    padding: '12px',
    borderRadius: '8px',
    fontSize: '14px',
    marginBottom: '16px',
  },
  alertError: {
    backgroundColor: 'rgba(239, 68, 68, 0.15)',
    border: '1px solid #ef4444',
    color: '#fca5a5',
    padding: '12px',
    borderRadius: '8px',
    fontSize: '14px',
    marginBottom: '16px',
  },
  form: {
    display: 'flex',
    flexDirection: 'column',
    gap: '14px',
  },
  inputGroup: {
    display: 'flex',
    flexDirection: 'column',
    gap: '5px',
  },
  label: {
    fontSize: '13px',
    fontWeight: '600',
    color: '#cbd5e1',
  },
  input: {
    padding: '10px 14px',
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
    marginTop: '10px',
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
    marginTop: '20px',
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
