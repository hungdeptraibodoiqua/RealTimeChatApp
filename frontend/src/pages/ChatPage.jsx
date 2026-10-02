import React, { useEffect, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { useNavigate } from 'react-router-dom';
import { logout } from '../features/auth/authSlice';
import { signalrService } from '../services/signalrService';

export default function ChatPage() {
  const dispatch = useDispatch();
  const navigate = useNavigate();
  const { user } = useSelector((state) => state.auth);

  const [signalrStatus, setSignalrStatus] = useState('Đang kết nối SignalR Hub...');
  const [isOnline, setIsOnline] = useState(false);
  const [onlineUsers, setOnlineUsers] = useState([]);

  useEffect(() => {
    let active = true;

    // Khởi tạo kết nối realtime SignalR khi vào trang
    const initSignalR = async () => {
      try {
        const connection = await signalrService.startConnection();
        if (!active) return;
        setSignalrStatus('Đã kết nối thành công tới Hub!');
        setIsOnline(true);

        // Lắng nghe sự kiện người dùng khác online
        connection.on('UserIsOnline', (userId) => {
          setOnlineUsers((prev) => [...new Set([...prev, userId])]);
        });

        // Lắng nghe sự kiện người dùng khác offline
        connection.on('UserIsOffline', (userId) => {
          setOnlineUsers((prev) => prev.filter((id) => id !== userId));
        });
      } catch (err) {
        if (!active) return;
        setSignalrStatus(`Kết nối SignalR thất bại: ${err.message || 'Lỗi 401 hoặc CORS'}`);
        setIsOnline(false);
      }
    };

    initSignalR();

    return () => {
      active = false;
      signalrService.stopConnection();
    };
  }, []);

  const handleLogout = () => {
    signalrService.stopConnection();
    dispatch(logout());
    navigate('/login');
  };

  return (
    <div style={styles.container}>
      <header style={styles.header}>
        <div style={styles.logo}>💬 Real-Time Chat App</div>
        <div style={styles.userSection}>
          <div style={styles.userInfo}>
            <span style={styles.displayName}>{user?.displayName || 'Người dùng'}</span>
            <span style={styles.username}>@{user?.username || 'user'}</span>
          </div>
          <button onClick={handleLogout} style={styles.logoutBtn}>
            Đăng xuất
          </button>
        </div>
      </header>

      <main style={styles.main}>
        <div style={styles.statusCard}>
          <h3 style={styles.statusTitle}>Trạng Thái Kết Nối Real-Time</h3>
          <div style={styles.statusRow}>
            <div
              style={{
                ...styles.statusDot,
                backgroundColor: isOnline ? '#22c55e' : '#ef4444',
              }}
            />
            <span style={styles.statusText}>{signalrStatus}</span>
          </div>
          <p style={styles.hint}>
            SignalR Hub đang chạy tại endpoint <code>/hub/chat</code> với token bảo vệ qua WebSocket query string.
          </p>
        </div>
      </main>
    </div>
  );
}

const styles = {
  container: {
    minHeight: '100vh',
    backgroundColor: '#0f172a',
    color: '#f8fafc',
    display: 'flex',
    flexDirection: 'column',
  },
  header: {
    display: 'flex',
    justifyContent: 'space-between',
    alignItems: 'center',
    padding: '16px 28px',
    backgroundColor: '#1e293b',
    borderBottom: '1px solid #334155',
  },
  logo: {
    fontSize: '20px',
    fontWeight: '700',
    color: '#38bdf8',
  },
  userSection: {
    display: 'flex',
    alignItems: 'center',
    gap: '20px',
  },
  userInfo: {
    display: 'flex',
    flexDirection: 'column',
    alignItems: 'flex-end',
  },
  displayName: {
    fontWeight: '600',
    fontSize: '14px',
  },
  username: {
    fontSize: '12px',
    color: '#94a3b8',
  },
  logoutBtn: {
    padding: '8px 16px',
    backgroundColor: '#334155',
    color: '#f8fafc',
    border: 'none',
    borderRadius: '6px',
    cursor: 'pointer',
    fontSize: '13px',
    fontWeight: '500',
  },
  main: {
    flex: 1,
    padding: '32px',
    display: 'flex',
    justifyContent: 'center',
    alignItems: 'flex-start',
  },
  statusCard: {
    backgroundColor: '#1e293b',
    padding: '24px',
    borderRadius: '12px',
    maxWidth: '560px',
    width: '100%',
    border: '1px solid #334155',
  },
  statusTitle: {
    fontSize: '16px',
    fontWeight: '600',
    marginBottom: '16px',
    color: '#38bdf8',
  },
  statusRow: {
    display: 'flex',
    alignItems: 'center',
    gap: '12px',
    marginBottom: '14px',
  },
  statusDot: {
    width: '12px',
    height: '12px',
    borderRadius: '50%',
  },
  statusText: {
    fontSize: '14px',
    fontWeight: '500',
  },
  hint: {
    fontSize: '13px',
    color: '#94a3b8',
    lineHeight: '1.5',
  },
};
