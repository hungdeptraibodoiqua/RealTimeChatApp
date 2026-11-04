import React from 'react';

/**
 * Modal dùng chung cho toàn bộ ứng dụng: Tạo phòng, Cài đặt phòng, Bạn bè.
 */
export default function Modal({ title, children, onClose }) {
  return (
    <div style={styles.overlay} onClick={onClose}>
      <div style={styles.modalBox} onClick={(e) => e.stopPropagation()}>
        <div style={styles.header}>
          <h3 style={styles.title}>{title}</h3>
          <button onClick={onClose} style={styles.closeBtn}>
            ✕
          </button>
        </div>
        <div style={styles.body}>{children}</div>
      </div>
    </div>
  );
}

const styles = {
  overlay: {
    position: 'fixed',
    top: 0,
    left: 0,
    right: 0,
    bottom: 0,
    backgroundColor: 'rgba(0, 0, 0, 0.65)',
    display: 'flex',
    alignItems: 'center',
    justifyContent: 'center',
    zIndex: 1000,
    backdropFilter: 'blur(3px)',
  },
  modalBox: {
    backgroundColor: '#ffffff',
    borderRadius: '12px',
    width: '90%',
    maxWidth: '520px',
    maxHeight: '85vh',
    display: 'flex',
    flexDirection: 'column',
    boxShadow: '0 20px 25px -5px rgba(0, 0, 0, 0.3)',
    overflow: 'hidden',
  },
  header: {
    display: 'flex',
    justifyContent: 'space-between',
    alignItems: 'center',
    padding: '16px 20px',
    borderBottom: '1px solid #e2e8f0',
  },
  title: {
    margin: 0,
    fontSize: '16px',
    fontWeight: '600',
    color: '#0f172a',
  },
  closeBtn: {
    background: 'none',
    border: 'none',
    fontSize: '18px',
    color: '#64748b',
    cursor: 'pointer',
    padding: '4px',
  },
  body: {
    padding: '20px',
    overflowY: 'auto',
  },
};
