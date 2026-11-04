import React from 'react';
import { Routes, Route, Navigate } from 'react-router-dom';
import { useSelector } from 'react-redux';
import LoginPage from './pages/LoginPage';
import RegisterPage from './pages/RegisterPage';
import ChatPage from './pages/ChatPage';

// Component bảo vệ route yêu cầu người dùng phải có token đăng nhập
function ProtectedRoute({ children }) {
  const { isAuthenticated } = useSelector((state) => state.auth);
  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }
  return children;
}

// Component chuyển hướng người dùng đã đăng nhập nếu cố vào trang /login hoặc /register
function PublicOnlyRoute({ children }) {
  const { isAuthenticated } = useSelector((state) => state.auth);
  if (isAuthenticated) {
    return <Navigate to="/chat" replace />;
  }
  return children;
}

export default function App() {
  return (
    <Routes>
      <Route
        path="/login"
        element={
          <PublicOnlyRoute>
            <LoginPage />
          </PublicOnlyRoute>
        }
      />
      <Route
        path="/register"
        element={
          <PublicOnlyRoute>
            <RegisterPage />
          </PublicOnlyRoute>
        }
      />
      <Route
        path="/chat"
        element={
          <ProtectedRoute>
            <ChatPage />
          </ProtectedRoute>
        }
      />
      <Route path="*" element={<Navigate to="/chat" replace />} />
    </Routes>
  );
}
