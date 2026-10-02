import * as signalR from '@microsoft/signalr';

class SignalRService {
  constructor() {
    this.connection = null;
  }

  // Khởi tạo kết nối SignalR Hub có kèm token xác thực
  async startConnection() {
    const baseUrl = import.meta.env.VITE_API_URL || 'http://localhost:5080';

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(`${baseUrl}/hub/chat`, {
        // Cung cấp token qua accessTokenFactory để SignalR gửi token qua query string (?access_token=...)
        accessTokenFactory: () => {
          const token = localStorage.getItem('token');
          return token || '';
        },
        skipNegotiation: false,
        transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.LongPolling,
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000]) // Tự động kết nối lại khi rớt mạng
      .configureLogging(signalR.LogLevel.Information)
      .build();

    try {
      await this.connection.start();
      console.log('SignalR Hub đã kết nối thành công!');
      return this.connection;
    } catch (err) {
      console.error('Lỗi kết nối SignalR Hub:', err);
      throw err;
    }
  }

  // Ngắt kết nối khi người dùng đăng xuất
  async stopConnection() {
    if (this.connection) {
      await this.connection.stop();
      this.connection = null;
    }
  }
}

export const signalrService = new SignalRService();
