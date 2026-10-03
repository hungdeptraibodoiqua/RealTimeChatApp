import * as signalR from '@microsoft/signalr';

/**
 * Service quản lý kết nối SignalR Hub client-side:
 * - Áp dụng startPromise singleton để giải quyết triệt để vấn đề re-render/re-mount của React 18 StrictMode.
 * - Lưu trữ danh sách event handlers nội bộ để tự động gắn lại listener khi khởi tạo hoặc reconnect.
 * - Chuẩn hóa roomId sang chữ thường (lowercase) đồng bộ 100% giữa Client và Backend.
 */
class SignalRService {
  constructor() {
    this.connection = null;
    this.currentRoomId = null;
    this.startPromise = null;
    this.handlers = new Map(); // Lưu trữ map các event callbacks: Map<eventName, Set<Function>>
  }

  // Khởi tạo kết nối SignalR Hub có kèm token xác thực và cơ chế chống race-condition
  async startConnection() {
    // Nếu kết nối đã thành công, trả về ngay
    if (this.connection && this.connection.state === signalR.HubConnectionState.Connected) {
      return this.connection;
    }

    // Nếu đang trong quá trình start, trả về cùng một Promise để tránh tạo kết nối trùng lặp
    if (this.startPromise) {
      return this.startPromise;
    }

    const baseUrl = import.meta.env.VITE_API_URL || 'http://localhost:5080';

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(`${baseUrl}/hub/chat`, {
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

    // Tự động gia nhập lại phòng hiện tại khi rớt mạng rồi kết nối lại thành công
    this.connection.onreconnected(async (connectionId) => {
      console.log('SignalR đã kết nối lại thành công, connectionId mới:', connectionId);
      if (this.currentRoomId) {
        await this.joinRoom(this.currentRoomId);
      }
    });

    // Tự động gắn lại toàn bộ các event listeners đã đăng ký vào instance kết nối mới
    this.handlers.forEach((callbacks, eventName) => {
      callbacks.forEach((cb) => {
        this.connection.on(eventName, cb);
      });
    });

    this.startPromise = (async () => {
      try {
        await this.connection.start();
        console.log('SignalR Hub đã kết nối thành công!');

        // Nếu đã có phòng chat active trước đó, tự động join ngay sau khi start thành công
        if (this.currentRoomId) {
          await this.joinRoom(this.currentRoomId);
        }

        return this.connection;
      } catch (err) {
        console.error('Lỗi khi start kết nối SignalR Hub:', err);
        throw err;
      } finally {
        this.startPromise = null;
      }
    })();

    return this.startPromise;
  }

  // Đăng ký hàm lắng nghe sự kiện từ Hub với lưu trữ nội bộ để duy trì qua các lần reconnect
  on(eventName, callback) {
    if (!this.handlers.has(eventName)) {
      this.handlers.set(eventName, new Set());
    }
    this.handlers.get(eventName).add(callback);

    if (this.connection) {
      this.connection.on(eventName, callback);
    }
  }

  // Hủy đăng ký lắng nghe sự kiện
  off(eventName, callback) {
    if (this.handlers.has(eventName)) {
      if (callback) {
        this.handlers.get(eventName).delete(callback);
      } else {
        this.handlers.delete(eventName);
      }
    }

    if (this.connection) {
      if (callback) {
        this.connection.off(eventName, callback);
      } else {
        this.connection.off(eventName);
      }
    }
  }

  // Tham gia vào nhóm phòng chat có chuẩn hóa roomId chữ thường và cơ chế chờ kết nối an toàn
  async joinRoom(roomId) {
    if (!roomId) return;
    const normalizedRoomId = roomId.toString().toLowerCase().trim();
    this.currentRoomId = normalizedRoomId;

    // Đảm bảo kết nối đã được start
    if (!this.connection || this.connection.state !== signalR.HubConnectionState.Connected) {
      try {
        await this.startConnection();
      } catch {
        return;
      }
    }

    // Nếu đang trong quá trình Connecting, đợi tối đa 3 giây
    let retries = 0;
    while (this.connection && this.connection.state === signalR.HubConnectionState.Connecting && retries < 15) {
      await new Promise((res) => setTimeout(res, 200));
      retries++;
    }

    if (this.connection && this.connection.state === signalR.HubConnectionState.Connected) {
      try {
        await this.connection.invoke('JoinRoom', normalizedRoomId);
        console.log(`SignalR: Đã tham gia nhóm phòng ${normalizedRoomId}`);
      } catch (err) {
        console.error(`Không thể join room ${normalizedRoomId}:`, err);
      }
    }
  }

  // Rời khỏi nhóm phòng chat
  async leaveRoom(roomId) {
    if (!roomId) return;
    const normalizedRoomId = roomId.toString().toLowerCase().trim();
    if (this.currentRoomId === normalizedRoomId) {
      this.currentRoomId = null;
    }

    if (this.connection && this.connection.state === signalR.HubConnectionState.Connected) {
      try {
        await this.connection.invoke('LeaveRoom', normalizedRoomId);
        console.log(`SignalR: Đã rời nhóm phòng ${normalizedRoomId}`);
      } catch (err) {
        console.error(`Không thể leave room ${normalizedRoomId}:`, err);
      }
    }
  }

  // Gửi sự kiện đang soạn tin nhắn tới các thành viên khác trong phòng với roomId chuẩn hóa
  async sendTyping(roomId, isTyping) {
    if (!roomId) return;
    const normalizedRoomId = roomId.toString().toLowerCase().trim();

    if (this.connection && this.connection.state === signalR.HubConnectionState.Connected) {
      try {
        await this.connection.invoke('SendTyping', normalizedRoomId, isTyping);
      } catch (err) {
        console.error('Lỗi gửi sự kiện typing:', err);
      }
    }
  }

  // Ngắt kết nối một cách an toàn
  async stopConnection() {
    if (this.connection) {
      try {
        await this.connection.stop();
      } catch {
        // Bỏ qua lỗi khi ngắt kết nối đang dở dang
      }
      this.connection = null;
      this.startPromise = null;
      this.currentRoomId = null;
    }
  }
}

export const signalrService = new SignalRService();
