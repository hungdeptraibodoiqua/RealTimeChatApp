import React from 'react';

/**
 * Input component dùng chung có label và hiển thị error message.
 */
export default function Input({ label, value, onChange, placeholder, type = 'text', error, style = {}, ...props }) {
  return (
    <div style={styles.container}>
      {label && <label style={styles.label}>{label}</label>}
      <input
        type={type}
        value={value}
        onChange={onChange}
        placeholder={placeholder}
        style={{
          ...styles.input,
          borderColor: error ? '#ef4444' : '#cbd5e1',
          ...style,
        }}
        {...props}
      />
      {error && <span style={styles.errorText}>{error}</span>}
    </div>
  );
}

const styles = {
  container: {
    display: 'flex',
    flexDirection: 'column',
    gap: '4px',
    margin: '8px 0',
  },
  label: {
    fontSize: '13px',
    fontWeight: '500',
    color: '#475569',
  },
  input: {
    padding: '8px 12px',
    borderRadius: '6px',
    border: '1px solid #cbd5e1',
    fontSize: '14px',
    outline: 'none',
    boxSizing: 'border-box',
    width: '100%',
    color: '#0f172a',
    backgroundColor: '#ffffff',
  },
  errorText: {
    fontSize: '12px',
    color: '#ef4444',
  },
};
