import { configureStore } from '@reduxjs/toolkit';
import authReducer from '../features/auth/authSlice';

// Cấu hình Redux store trung tâm cho toàn bộ frontend application
export const store = configureStore({
  reducer: {
    auth: authReducer,
  },
  devTools: process.env.NODE_ENV !== 'production',
});
