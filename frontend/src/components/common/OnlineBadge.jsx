import React from 'react';

/**
 * OnlineBadge component hiển thị chấm tròn trạng thái online/offline của user.
 */
export default function OnlineBadge({ isOnline, size = 10, style = {} }) {
  return (
    <span
      style={{
        width: size,
        height: size,
        borderRadius: '50%',
        backgroundColor: isOnline ? '#10b981' : '#94a3b8',
        display: 'inline-block',
        border: '2px solid #ffffff',
        boxSizing: 'content-box',
        ...style,
      }}
      title={isOnline ? 'Online' : 'Offline'}
    />
  );
}
