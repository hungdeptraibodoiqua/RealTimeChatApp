import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import { authService } from '../../services/authService';

// Đọc dữ liệu user và token khởi tạo từ localStorage
const storedToken = localStorage.getItem('token');
let storedUser = null;
try {
  const rawUser = localStorage.getItem('user');
  if (rawUser) {
    storedUser = JSON.parse(rawUser);
  }
} catch {
  storedUser = null;
}

const initialState = {
  user: storedUser,
  token: storedToken || null,
  isAuthenticated: !!storedToken,
  loading: false,
  error: null,
};

// Async thunk xử lý đăng nhập người dùng
export const loginUser = createAsyncThunk(
  'auth/loginUser',
  async (credentials, { rejectWithValue }) => {
    try {
      // Dữ liệu đăng nhập đi từ UI form -> loginUser thunk -> authService.login -> API
      const data = await authService.login(credentials);
      return data;
    } catch (error) {
      return rejectWithValue(error.message);
    }
  }
);

// Async thunk xử lý đăng ký người dùng mới
export const registerUser = createAsyncThunk(
  'auth/registerUser',
  async (registerData, { rejectWithValue }) => {
    try {
      const data = await authService.register(registerData);
      return data;
    } catch (error) {
      return rejectWithValue(error.message);
    }
  }
);

const authSlice = createSlice({
  name: 'auth',
  initialState,
  reducers: {
    // Đăng xuất: xóa token và user khỏi state và localStorage
    logout: (state) => {
      state.user = null;
      state.token = null;
      state.isAuthenticated = false;
      state.error = null;
      localStorage.removeItem('token');
      localStorage.removeItem('refreshToken');
      localStorage.removeItem('user');
    },
    // Xóa thông báo lỗi khi chuyển form hoặc người dùng nhập lại
    clearAuthError: (state) => {
      state.error = null;
    },
  },
  extraReducers: (builder) => {
    builder
      // Xử lý Login
      .addCase(loginUser.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(loginUser.fulfilled, (state, action) => {
        state.loading = false;
        state.isAuthenticated = true;
        state.user = {
          id: action.payload.id,
          username: action.payload.username,
          email: action.payload.email,
          displayName: action.payload.displayName,
        };
        state.token = action.payload.tokens?.accessToken || null;

        // Lưu thông tin vào localStorage để duy trì phiên đăng nhập sau F5
        if (action.payload.tokens) {
          localStorage.setItem('token', action.payload.tokens.accessToken);
          localStorage.setItem('refreshToken', action.payload.tokens.refreshToken);
        }
        localStorage.setItem('user', JSON.stringify(state.user));
      })
      .addCase(loginUser.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload || 'Đăng nhập không thành công';
      })
      // Xử lý Register
      .addCase(registerUser.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(registerUser.fulfilled, (state) => {
        state.loading = false;
        state.error = null;
      })
      .addCase(registerUser.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload || 'Đăng ký không thành công';
      });
  },
});

export const { logout, clearAuthError } = authSlice.actions;
export default authSlice.reducer;
