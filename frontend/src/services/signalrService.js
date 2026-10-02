import * as signalR from '@microsoft/signalr';

/**
 * Service quản lý kết nối SignalR Hub client-side.
 * Hỗ trợ tự động reconnect, join/leave room group, gửi typing indicator và lắng nghe các sự kiện realtime.
 */
class SignalRService {
  constructor() {
    this.connection = null;
    this.currentRoomId = null;
  }

  // Khởi tạo kết nối SignalR Hub có kèm token xác thực
  async startConnection() {
    if (this.connection && this.connection.state === signalR.HubConnectionState.Connected) {
      return this.connection;
    }

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
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    // Khi kết nối lại thành công, tự động join lại phòng chat đang mở
    this.connection.onreconnected(async (connectionId) => {
      console.log('SignalR đã kết nối lại, connectionId mới:', connectionId);
      if (this.currentRoomId) {
        await this.joinRoom(this.currentRoomId);
      }
    });

    try {
      await this.connection.start();
      console.log('SignalR Hub đã kết nối thành công!');
      return this.connection;
    } catch (err) {
      console.error('Lỗi kết nối SignalR Hub:', err);
      throw err;
    }
  }

  // Tham gia vào nhóm phòng chat để nhận tin nhắn và typing realtime
  async joinRoom(roomId) {
    this.currentRoomId = roomId;
    if (this.connection && this.connection.state === signalR.HubConnectionState.Connected) {
      try {
        await this.connection.invoke('JoinRoom', roomId.toString());
      } catch (err) {
        console.error(`Không thể join room ${roomId}:`, err);
      }
    }
  }

  // Rời khỏi nhóm phòng chat
  async leaveRoom(roomId) {
    if (this.currentRoomId === roomId) {
      this.currentRoomId = null;
    }
    if (this.connection && this.connection.state === signalR.HubConnectionState.Connected) {
      try {
        await this.connection.invoke('LeaveRoom', roomId.toString());
      } catch (err) {
        console.error(`Không thể leave room ${roomId}:`, err);
      }
    }
  }

  // Gửi sự kiện đang soạn tin nhắn tới các thành viên khác trong phòng
  async sendTyping(roomId, isTyping) {
    if (this.connection && this.connection.state === signalR.HubConnectionState.Connected) {
      try {
        await this.connection.invoke('SendTyping', roomId.toString(), isTyping);
      } catch (err) {
        console.error('Lỗi gửi sự kiện typing:', err);
      }
    }
  }

  // Đăng ký hàm lắng nghe sự kiện từ Hub
  on(eventName, callback) {
    if (this.connection) {
      this.connection.on(eventName, callback);
    }
  }

  // Hủy đăng ký lắng nghe sự kiện
  off(eventName, callback) {
    if (this.connection) {
      if (callback) {
        this.connection.off(eventName, callback);
      } else {
        this.connection.off(eventName);
      }
    }
  }

  // Ngắt kết nối khi người dùng đăng xuất
  async stopConnection() {
    if (this.connection) {
      await this.connection.stop();
      this.connection = null;
      this.currentRoomId = null;
    }
  }
}

export const signalrService = new SignalRService();
