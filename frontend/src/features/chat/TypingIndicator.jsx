import React from 'react';

/**
 * Component hiển thị thông báo "Ai đó đang soạn tin nhắn...":
 * - Tiếp nhận danh sách user đang gõ từ SignalR hub
 * - Hiệu ứng chuyển động dấu ba chấm sinh động
 */
export default function TypingIndicator({ typingUsers }) {
  if (!typingUsers || Object.keys(typingUsers).length === 0) {
    return null;
  }

  const names = Object.values(typingUsers).map((u) => u.displayName || 'Ai đó');
  let text = '';
  if (names.length === 1) {
    text = `${names[0]} đang soạn tin nhắn...`;
  } else if (names.length === 2) {
    text = `${names[0]} và ${names[1]} đang soạn tin nhắn...`;
  } else {
    text = `${names[0]} và ${names.length - 1} người khác đang soạn tin nhắn...`;
  }

  return (
    <div style={styles.container}>
      <div style={styles.dotContainer}>
        <span style={{ ...styles.dot, animationDelay: '0s' }} />
        <span style={{ ...styles.dot, animationDelay: '0.2s' }} />
        <span style={{ ...styles.dot, animationDelay: '0.4s' }} />
      </div>
      <span style={styles.text}>{text}</span>
    </div>
  );
}

const styles = {
  container: {
    display: 'flex',
    alignItems: 'center',
    gap: '8px',
    padding: '4px 16px',
    backgroundColor: '#0f172a',
    fontSize: '12px',
    color: '#94a3b8',
    fontStyle: 'italic',
  },
  dotContainer: {
    display: 'flex',
    alignItems: 'center',
    gap: '3px',
  },
  dot: {
    width: '5px',
    height: '5px',
    borderRadius: '50%',
    backgroundColor: '#38bdf8',
    display: 'inline-block',
  },
  text: {
    color: '#38bdf8',
  },
};
