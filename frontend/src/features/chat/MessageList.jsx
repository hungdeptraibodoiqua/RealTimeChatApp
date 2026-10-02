import React, { useEffect, useRef } from 'react';
import MessageItem from './MessageItem';

/**
 * Component danh sách tin nhắn của phòng chat:
 * - Render từng MessageItem
 * - Tự động cuộn xuống dưới cùng khi có tin nhắn mới hoặc vừa mở phòng chat
 */
export default function MessageList({ messages, currentUserId, onRecall, onEdit, onDelete }) {
  const bottomRef = useRef(null);

  // Tự động cuộn xuống cuối danh sách khi số lượng tin nhắn thay đổi
  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [messages]);

  if (!messages || messages.length === 0) {
    return (
      <div style={styles.emptyContainer}>
        <div style={styles.emptyIcon}>💬</div>
        <div style={styles.emptyTitle}>Chưa có tin nhắn nào</div>
        <div style={styles.emptySubtitle}>Hãy gửi tin nhắn đầu tiên để bắt đầu cuộc trò chuyện!</div>
      </div>
    );
  }

  return (
    <div style={styles.container}>
      {messages.map((message) => (
        <MessageItem
          key={message.id}
          message={message}
          currentUserId={currentUserId}
          onRecall={onRecall}
          onEdit={onEdit}
          onDelete={onDelete}
        />
      ))}
      <div ref={bottomRef} />
    </div>
  );
}

const styles = {
  container: {
    flex: 1,
    overflowY: 'auto',
    padding: '16px 0',
    display: 'flex',
    flexDirection: 'column',
  },
  emptyContainer: {
    flex: 1,
    display: 'flex',
    flexDirection: 'column',
    alignItems: 'center',
    justifyContent: 'center',
    color: '#64748b',
    padding: '32px',
  },
  emptyIcon: {
    fontSize: '48px',
    marginBottom: '12px',
  },
  emptyTitle: {
    fontSize: '16px',
    fontWeight: '600',
    color: '#94a3b8',
    marginBottom: '4px',
  },
  emptySubtitle: {
    fontSize: '13px',
    color: '#64748b',
  },
};
