import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// Cấu hình Vite dev server proxy tới ASP.NET Core backend chạy trên port 5080
export default defineConfig({
  plugins: [react()],
  server: {
    port: 3000,
    proxy: {
      // Chuyển tiếp các request /api sang Backend giữ nguyên route
      '/api': {
        target: 'http://localhost:5080',
        changeOrigin: true,
        secure: false,
      },
      // Chuyển tiếp kết nối WebSocket cho SignalR Hub
      '/hub': {
        target: 'http://localhost:5080',
        changeOrigin: true,
        ws: true,
        secure: false,
      },
    },
  },
})

