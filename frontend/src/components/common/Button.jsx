import React from 'react';

/**
 * Button component dùng chung hỗ trợ variants: primary, secondary, danger.
 */
export default function Button({ children, variant = 'primary', onClick, disabled = false, style = {}, type = 'button' }) {
  const getVariantStyle = () => {
    switch (variant) {
      case 'secondary':
        return styles.secondary;
      case 'danger':
        return styles.danger;
      default:
        return styles.primary;
    }
  };

  return (
    <button
      type={type}
      onClick={onClick}
      disabled={disabled}
      style={{
        ...styles.base,
        ...getVariantStyle(),
        opacity: disabled ? 0.6 : 1,
        cursor: disabled ? 'not-allowed' : 'pointer',
        ...style,
      }}
    >
      {children}
    </button>
  );
}

const styles = {
  base: {
    padding: '8px 16px',
    borderRadius: '6px',
    fontSize: '13px',
    fontWeight: '500',
    border: 'none',
    transition: 'all 0.15s ease-in-out',
    display: 'inline-flex',
    alignItems: 'center',
    justifyContent: 'center',
  },
  primary: {
    backgroundColor: '#2563eb',
    color: '#ffffff',
  },
  secondary: {
    backgroundColor: '#f1f5f9',
    color: '#475569',
    border: '1px solid #cbd5e1',
  },
  danger: {
    backgroundColor: '#dc2626',
    color: '#ffffff',
  },
};
