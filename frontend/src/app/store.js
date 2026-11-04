import { configureStore } from '@reduxjs/toolkit';
import authReducer from '../features/auth/authSlice';
import roomsReducer from '../features/rooms/roomsSlice';
import chatReducer from '../features/chat/chatSlice';

// Cấu hình Redux store trung tâm cho toàn bộ frontend application
export const store = configureStore({
  reducer: {
    auth: authReducer,
    rooms: roomsReducer,
    chat: chatReducer,
  },
  devTools: process.env.NODE_ENV !== 'production',
});
