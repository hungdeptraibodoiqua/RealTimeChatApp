import React from 'react';

/**
 * Avatar component dùng chung hiển thị hình ảnh đại diện hoặc chữ cái đầu của tên.
 */
export default function Avatar({ src, name = 'U', size = 36, style = {} }) {
  const initial = name ? name[0].toUpperCase() : 'U';

  if (src) {
    return (
      <img
        src={src}
        alt={name}
        style={{
          width: size,
          height: size,
          borderRadius: '50%',
          objectFit: 'cover',
          flexShrink: 0,
          ...style,
        }}
      />
    );
  }

  return (
    <div
      style={{
        width: size,
        height: size,
        borderRadius: '50%',
        backgroundColor: '#6366f1',
        color: '#ffffff',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        fontWeight: '600',
        fontSize: Math.floor(size * 0.42),
        flexShrink: 0,
        ...style,
      }}
    >
      {initial}
    </div>
  );
}
