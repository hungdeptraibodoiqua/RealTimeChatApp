import React, { useState, useRef } from 'react';

/**
 * Component thanh nhập tin nhắn chat:
 * - Nhập văn bản và gửi nhanh bằng phím Enter.
 * - Phát sự kiện Typing Indicator realtime qua SignalR (có debounce 2.5s).
 * - Nút đính kèm file ảnh hoặc video để tải lên server.
 */
export default function MessageInput({ roomId, onSendMessage, onUploadMedia, onSendTyping }) {
  const [content, setContent] = useState('');
  const [isUploading, setIsUploading] = useState(false);
  const typingTimeoutRef = useRef(null);
  const fileInputRef = useRef(null);

  const handleInputChange = (e) => {
    setContent(e.target.value);

    // Kích hoạt Typing Indicator qua SignalR
    onSendTyping(roomId, true);

    // Hủy timeout cũ nếu user vẫn đang gõ tiếp
    if (typingTimeoutRef.current) {
      clearTimeout(typingTimeoutRef.current);
    }

    // Tự động tắt trạng thái typing sau 2.5 giây không có thao tác gõ
    typingTimeoutRef.current = setTimeout(() => {
      onSendTyping(roomId, false);
    }, 2500);
  };

  const handleSend = () => {
    if (!content.trim()) return;

    onSendMessage(content.trim());
    setContent('');

    // Dừng trạng thái typing ngay khi tin nhắn được gửi đi
    if (typingTimeoutRef.current) {
      clearTimeout(typingTimeoutRef.current);
    }
    onSendTyping(roomId, false);
  };

  const handleKeyDown = (e) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      handleSend();
    }
  };

  const handleFileChange = async (e) => {
    const file = e.target.files?.[0];
    if (!file) return;

    // Kiểm tra dung lượng 25MB ở client
    if (file.size > 25 * 1024 * 1024) {
      alert('Dung lượng file vượt quá giới hạn 25MB.');
      e.target.value = '';
      return;
    }

    setIsUploading(true);
    try {
      await onUploadMedia(file, roomId);
    } finally {
      setIsUploading(false);
      e.target.value = ''; // Reset input để cho phép chọn lại file cùng tên
    }
  };

  return (
    <div style={styles.container}>
      {/* File input ẩn cho đính kèm ảnh và video */}
      <input
        type="file"
        ref={fileInputRef}
        onChange={handleFileChange}
        accept="image/*,video/*"
        style={{ display: 'none' }}
      />

      <button
        onClick={() => fileInputRef.current?.click()}
        style={{ ...styles.attachBtn, opacity: isUploading ? 0.6 : 1 }}
        disabled={isUploading}
        title="Gửi hình ảnh hoặc video"
      >
        {isUploading ? '⏳' : '📎'}
      </button>

      <input
        type="text"
        placeholder={isUploading ? 'Đang tải file lên...' : 'Nhập tin nhắn... (Nhấn Enter để gửi)'}
        value={content}
        onChange={handleInputChange}
        onKeyDown={handleKeyDown}
        disabled={isUploading}
        style={styles.input}
      />

      <button
        onClick={handleSend}
        disabled={!content.trim() || isUploading}
        style={{
          ...styles.sendBtn,
          opacity: !content.trim() || isUploading ? 0.5 : 1,
          cursor: !content.trim() || isUploading ? 'not-allowed' : 'pointer',
        }}
      >
        Gửi
      </button>
    </div>
  );
}

const styles = {
  container: {
    display: 'flex',
    alignItems: 'center',
    gap: '10px',
    padding: '12px 16px',
    backgroundColor: '#1e293b',
    borderTop: '1px solid #334155',
  },
  attachBtn: {
    backgroundColor: '#334155',
    border: '1px solid #475569',
    borderRadius: '8px',
    padding: '9px 12px',
    color: '#f8fafc',
    cursor: 'pointer',
    fontSize: '16px',
    display: 'flex',
    alignItems: 'center',
    justifyContent: 'center',
    transition: 'background-color 0.2s',
  },
  input: {
    flex: 1,
    padding: '10px 14px',
    borderRadius: '8px',
    border: '1px solid #475569',
    backgroundColor: '#0f172a',
    color: '#f8fafc',
    fontSize: '14px',
    outline: 'none',
  },
  sendBtn: {
    backgroundColor: '#2563eb',
    color: '#ffffff',
    border: 'none',
    borderRadius: '8px',
    padding: '10px 20px',
    fontWeight: '600',
    fontSize: '14px',
    transition: 'background-color 0.2s',
  },
};
