import React, { useEffect, useState } from 'react';
import apiClient from '../../services/apiClient';

/**
 * Component hiển thị một tin nhắn trong danh sách chat:
 * - Hỗ trợ render Text, Image, Video, System message.
 * - Hiển thị nhãn "Đã chỉnh sửa" và trạng thái "Tin nhắn đã bị thu hồi".
 * - Cung cấp các nút tương tác Sửa / Thu hồi (trong vòng 1 tiếng) và Xóa.
 */
export default function MessageItem({ message, currentUserId, onRecall, onEdit, onDelete }) {
  const [isEditing, setIsEditing] = useState(false);
  const [editContent, setEditContent] = useState(message.content);

  const isMe = message.senderUserId === currentUserId;
  const isRecalled = Boolean(message.recalledAtUtc);
  const isMedia = message.messageType === 3 || message.messageType === 4;
  const mediaUrl = useProtectedMedia(message.id, isMedia && !isRecalled);

  // Kiểm tra thời hạn 1 tiếng kể từ lúc gửi (3,600,000 milliseconds)
  const isWithinOneHour = () => {
    if (!message.createdAtUtc) return false;
    const createdTime = new Date(message.createdAtUtc).getTime();
    const now = Date.now();
    return now - createdTime <= 3600000;
  };

  const canModify = isMe && !isRecalled && isWithinOneHour();

  const handleSaveEdit = () => {
    if (!editContent.trim()) return;
    onEdit(message.id, editContent.trim());
    setIsEditing(false);
  };

  const formatTime = (dateStr) => {
    try {
      const d = new Date(dateStr);
      return d.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
    } catch {
      return '';
    }
  };

  // Render tin nhắn thông báo hệ thống (ví dụ: gia nhập/rời phòng)
  if (message.messageType === 2) {
    return (
      <div style={styles.systemMessage}>
        <span style={styles.systemText}>{message.content}</span>
      </div>
    );
  }

  return (
    <div style={{ ...styles.row, justifyContent: isMe ? 'flex-end' : 'flex-start' }}>
      <div
        style={{
          ...styles.bubble,
          backgroundColor: isMe ? '#2563eb' : '#334155',
          borderRadius: isMe ? '16px 16px 4px 16px' : '16px 16px 16px 4px',
        }}
      >
        {/* Trường hợp 1: Tin nhắn đã bị thu hồi */}
        {isRecalled ? (
          <div style={styles.recalledText}>🚫 Tin nhắn đã bị thu hồi</div>
        ) : isEditing ? (
          /* Trường hợp 2: Đang mở chế độ sửa tin nhắn */
          <div style={styles.editContainer}>
            <input
              type="text"
              value={editContent}
              onChange={(e) => setEditContent(e.target.value)}
              style={styles.editInput}
              autoFocus
            />
            <div style={styles.editActions}>
              <button onClick={handleSaveEdit} style={styles.saveBtn}>
                Lưu
              </button>
              <button onClick={() => setIsEditing(false)} style={styles.cancelBtn}>
                Hủy
              </button>
            </div>
          </div>
        ) : (
          /* Trường hợp 3: Hiển thị nội dung bình thường (Text / Image / Video) */
          <>
            {/* Render Hình ảnh */}
            {message.messageType === 3 && (
              <div style={styles.mediaWrapper}>
                {mediaUrl ? (
                  <img
                    src={mediaUrl}
                    alt="Ảnh đính kèm"
                    style={styles.mediaImage}
                    onClick={() => window.open(mediaUrl, '_blank', 'noopener')}
                    title="Bấm để xem ảnh gốc"
                  />
                ) : (
                  <span style={styles.textContent}>Đang tải ảnh...</span>
                )}
              </div>
            )}

            {/* Render Video */}
            {message.messageType === 4 && (
              <div style={styles.mediaWrapper}>
                {mediaUrl ? (
                  <video controls src={mediaUrl} style={styles.mediaVideo} preload="metadata" />
                ) : (
                  <span style={styles.textContent}>Đang tải video...</span>
                )}
              </div>
            )}

            {/* Render Văn bản */}
            {message.messageType === 1 && (
              <div style={styles.textContent}>
                {message.content}
                {message.editedAtUtc && (
                  <span style={styles.editedTag} title={`Sửa lúc ${formatTime(message.editedAtUtc)}`}>
                    (Đã chỉnh sửa)
                  </span>
                )}
              </div>
            )}
          </>
        )}

        {/* Footer của bubble: Thời gian gửi và Menu thao tác */}
        <div style={styles.metaRow}>
          <span style={styles.time}>{formatTime(message.createdAtUtc)}</span>

          {/* Các nút tương tác: Chỉ hiện khi là tin của mình và trong hạn 1 giờ */}
          {canModify && (
            <div style={styles.actionMenu}>
              {message.messageType === 1 && (
                <button
                  onClick={() => {
                    setEditContent(message.content);
                    setIsEditing(true);
                  }}
                  style={styles.actionBtn}
                  title="Chỉnh sửa (trong vòng 1 giờ)"
                >
                  Sửa
                </button>
              )}
              <button
                onClick={() => onRecall(message.id)}
                style={styles.actionBtn}
                title="Thu hồi tin nhắn đối với tất cả mọi người"
              >
                Thu hồi
              </button>
              <button
                onClick={() => onDelete(message.id)}
                style={styles.actionBtnDanger}
                title="Xóa tin nhắn"
              >
                Xóa
              </button>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

const styles = {
  row: {
    display: 'flex',
    margin: '6px 0',
    padding: '0 16px',
  },
  systemMessage: {
    display: 'flex',
    justifyContent: 'center',
    margin: '8px 0',
  },
  systemText: {
    backgroundColor: '#334155',
    color: '#94a3b8',
    fontSize: '12px',
    padding: '4px 12px',
    borderRadius: '12px',
  },
  bubble: {
    maxWidth: '75%',
    padding: '10px 14px',
    color: '#ffffff',
    fontSize: '14px',
    boxShadow: '0 1px 3px rgba(0,0,0,0.2)',
  },
  textContent: {
    wordBreak: 'break-word',
    lineHeight: '1.45',
  },
  mediaWrapper: {
    marginTop: '4px',
    marginBottom: '4px',
  },
  mediaImage: {
    maxWidth: '100%',
    maxHeight: '320px',
    borderRadius: '8px',
    display: 'block',
    cursor: 'pointer',
    objectFit: 'cover',
  },
  mediaVideo: {
    maxWidth: '100%',
    maxHeight: '320px',
    borderRadius: '8px',
    display: 'block',
  },
  recalledText: {
    fontStyle: 'italic',
    color: '#cbd5e1',
    display: 'flex',
    alignItems: 'center',
    gap: '6px',
  },
  editedTag: {
    fontSize: '11px',
    color: '#93c5fd',
    marginLeft: '6px',
    fontStyle: 'italic',
  },
  metaRow: {
    display: 'flex',
    alignItems: 'center',
    justifyContent: 'space-between',
    gap: '12px',
    marginTop: '6px',
  },
  time: {
    fontSize: '11px',
    color: '#cbd5e1',
  },
  actionMenu: {
    display: 'flex',
    gap: '8px',
  },
  actionBtn: {
    background: 'none',
    border: 'none',
    color: '#bfdbfe',
    fontSize: '11px',
    cursor: 'pointer',
    padding: '0 2px',
    textDecoration: 'underline',
  },
  actionBtnDanger: {
    background: 'none',
    border: 'none',
    color: '#fca5a5',
    fontSize: '11px',
    cursor: 'pointer',
    padding: '0 2px',
    textDecoration: 'underline',
  },
  editContainer: {
    display: 'flex',
    flexDirection: 'column',
    gap: '6px',
  },
  editInput: {
    padding: '6px 10px',
    borderRadius: '6px',
    border: '1px solid #60a5fa',
    backgroundColor: '#1e293b',
    color: '#ffffff',
    fontSize: '13px',
    outline: 'none',
  },
  editActions: {
    display: 'flex',
    gap: '6px',
    justifyContent: 'flex-end',
  },
  saveBtn: {
    backgroundColor: '#10b981',
    color: '#ffffff',
    border: 'none',
    borderRadius: '4px',
    padding: '4px 10px',
    fontSize: '12px',
    cursor: 'pointer',
  },
  cancelBtn: {
    backgroundColor: '#64748b',
    color: '#ffffff',
    border: 'none',
    borderRadius: '4px',
    padding: '4px 10px',
    fontSize: '12px',
    cursor: 'pointer',
  },
};

/**
 * SEC-05: tải ảnh/video qua API có xác thực (GET /api/messages/{id}/media, gửi Bearer token qua apiClient)
 * thay vì đường dẫn /uploads/* công khai. File được chuyển thành blob URL cục bộ và giải phóng khi unmount.
 */
function useProtectedMedia(messageId, enabled) {
  const [url, setUrl] = useState(null);

  useEffect(() => {
    if (!enabled || !messageId) return undefined;
    let objectUrl = null;
    let cancelled = false;

    apiClient
      .get(`/api/messages/${messageId}/media`, { responseType: 'blob' })
      .then((res) => {
        if (cancelled) return;
        objectUrl = URL.createObjectURL(res.data);
        setUrl(objectUrl);
      })
      .catch(() => {
        if (!cancelled) setUrl(null);
      });

    return () => {
      cancelled = true;
      if (objectUrl) URL.revokeObjectURL(objectUrl);
    };
  }, [messageId, enabled]);

  return url;
}
