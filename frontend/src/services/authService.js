import apiClient from './apiClient';

// Service gọi API xác thực: kết nối giữa Redux thunk/UI với backend controller
export const authService = {
  // Gửi request đăng ký tài khoản mới tới POST /api/auth/register
  register: async (registerData) => {
    const response = await apiClient.post('/api/auth/register', registerData);
    return response.data;
  },

  // Gửi thông tin đăng nhập tới POST /api/auth/login để nhận access token
  login: async (credentials) => {
    const response = await apiClient.post('/api/auth/login', credentials);
    return response.data;
  },

  // Làm mới access token khi sắp hết hạn
  refreshToken: async (tokenData) => {
    const response = await apiClient.post('/api/auth/refresh-token', tokenData);
    return response.data;
  },
};
