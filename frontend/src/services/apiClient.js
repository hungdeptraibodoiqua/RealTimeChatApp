import axios from 'axios';

// Khởi tạo instance axios với cấu hình base URL chuẩn
const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_URL || 'http://localhost:5080',
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request Interceptor: Tự động đính kèm JWT Bearer token vào Header trước khi request rời khỏi trình duyệt
apiClient.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('token');
    if (token) {
      // Dữ liệu token đi từ localStorage -> Header Authorization của HTTP Request
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// Response Interceptor: Chuẩn hóa thông báo lỗi từ ExceptionMiddleware của .NET backend để UI dễ hiển thị
apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    let friendlyMessage = 'Đã có lỗi xảy ra, vui lòng thử lại sau.';

    if (error.response) {
      const data = error.response.data;

      // Xử lý lỗi validation từ ExceptionMiddleware: { errors: { FieldName: ['message'] } }
      if (data && data.errors) {
        const errorKeys = Object.keys(data.errors);
        if (errorKeys.length > 0) {
          friendlyMessage = data.errors[errorKeys[0]][0];
        }
      }
      // Xử lý lỗi nghiệp vụ AppException: { detail: 'Email already exists.' }
      else if (data && data.detail) {
        friendlyMessage = data.detail;
      } else if (data && data.title) {
        friendlyMessage = data.title;
      } else if (error.response.status === 401) {
        friendlyMessage = 'Phiên đăng nhập đã hết hạn hoặc không có quyền truy cập.';
        localStorage.removeItem('token');
        localStorage.removeItem('user');
        // Tự động chuyển về trang login nếu đang ở các trang cần xác thực
        if (typeof window !== 'undefined' && window.location.pathname !== '/login' && window.location.pathname !== '/register') {
          window.location.href = '/login';
        }
      }
    } else if (error.request) {
      // Lỗi do không kết nối được tới server hoặc bị CORS chặn
      friendlyMessage = 'Không thể kết nối đến máy chủ. Vui lòng kiểm tra backend và kết nối mạng.';
    }

    return Promise.reject(new Error(friendlyMessage));
  }
);

export default apiClient;
