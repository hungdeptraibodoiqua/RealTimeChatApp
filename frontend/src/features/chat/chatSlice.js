import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import { messageService } from '../../services/messageService';

// Tải lịch sử tin nhắn của phòng
export const fetchMessages = createAsyncThunk(
  'chat/fetchMessages',
  async ({ roomId, skip = 0, take = 50 }, { rejectWithValue }) => {
    try {
      const data = await messageService.getRoomMessages(roomId, skip, take);
      return { roomId, messages: data };
    } catch (err) {
      return rejectWithValue(err.message || 'Không thể tải tin nhắn.');
    }
  }
);

// Gửi tin nhắn text
export const sendMessage = createAsyncThunk(
  'chat/sendMessage',
  async (payload, { rejectWithValue }) => {
    try {
      return await messageService.sendMessage(payload);
    } catch (err) {
      return rejectWithValue(err.message || 'Không thể gửi tin nhắn.');
    }
  }
);

// Upload file ảnh hoặc video
export const uploadMedia = createAsyncThunk(
  'chat/uploadMedia',
  async ({ file, roomId }, { rejectWithValue }) => {
    try {
      return await messageService.uploadMedia(file, roomId);
    } catch (err) {
      return rejectWithValue(err.message || 'Không thể tải lên file.');
    }
  }
);

// Thu hồi tin nhắn
export const recallMessage = createAsyncThunk(
  'chat/recallMessage',
  async ({ messageId, roomId }, { rejectWithValue }) => {
    try {
      await messageService.recallMessage(messageId);
      return { messageId, roomId };
    } catch (err) {
      return rejectWithValue(err.message || 'Không thể thu hồi tin nhắn.');
    }
  }
);

// Chỉnh sửa tin nhắn
export const editMessage = createAsyncThunk(
  'chat/editMessage',
  async ({ messageId, newContent, roomId }, { rejectWithValue }) => {
    try {
      const updated = await messageService.editMessage(messageId, newContent);
      return { messageId, roomId, newContent: updated.content, editedAtUtc: updated.editedAtUtc };
    } catch (err) {
      return rejectWithValue(err.message || 'Không thể chỉnh sửa tin nhắn.');
    }
  }
);

// Xóa tin nhắn
export const deleteMessage = createAsyncThunk(
  'chat/deleteMessage',
  async ({ messageId, roomId }, { rejectWithValue }) => {
    try {
      await messageService.deleteMessage(messageId);
      return { messageId, roomId };
    } catch (err) {
      return rejectWithValue(err.message || 'Không thể xóa tin nhắn.');
    }
  }
);

const chatSlice = createSlice({
  name: 'chat',
  initialState: {
    messagesByRoom: {}, // { [roomId]: [ ...messages ] }
    activeRoomId: null,
    typingUsers: {},    // { [roomId]: { [userId]: { displayName, timestamp } } }
    loading: false,
    error: null,
  },
  reducers: {
    setActiveRoom: (state, action) => {
      // Chuẩn hóa activeRoomId sang chữ thường để đồng bộ với state map
      state.activeRoomId = action.payload ? action.payload.toString().toLowerCase() : null;
    },
    // Nhận tin nhắn mới từ SignalR (tương thích cả roomId camelCase và RoomId PascalCase từ server)
    messageReceived: (state, action) => {
      const message = action.payload;
      const rawRoomId = message.roomId || message.RoomId;
      if (!rawRoomId) return;
      const roomId = rawRoomId.toString().toLowerCase();

      if (!state.messagesByRoom[roomId]) {
        state.messagesByRoom[roomId] = [];
      }
      // Tránh duplicate tin nhắn nếu client vừa gửi vừa nhận qua SignalR
      const exists = state.messagesByRoom[roomId].some((m) => m.id === message.id);
      if (!exists) {
        state.messagesByRoom[roomId].push(message);
      }
    },
    // Nhận sự kiện thu hồi tin nhắn từ SignalR
    messageRecalledReceived: (state, action) => {
      const rawRoomId = action.payload.roomId || action.payload.RoomId;
      const messageId = action.payload.messageId || action.payload.MessageId;
      if (!rawRoomId) return;
      const roomId = rawRoomId.toString().toLowerCase();

      const list = state.messagesByRoom[roomId];
      if (list) {
        const msg = list.find((m) => m.id === messageId);
        if (msg) {
          msg.recalledAtUtc = new Date().toISOString();
          msg.content = 'Tin nhắn đã bị thu hồi';
        }
      }
    },
    // Nhận sự kiện sửa tin nhắn từ SignalR
    messageEditedReceived: (state, action) => {
      const rawRoomId = action.payload.roomId || action.payload.RoomId;
      const messageId = action.payload.messageId || action.payload.MessageId;
      const newContent = action.payload.newContent || action.payload.NewContent;
      const editedAtUtc = action.payload.editedAtUtc || action.payload.EditedAtUtc;
      if (!rawRoomId) return;
      const roomId = rawRoomId.toString().toLowerCase();

      const list = state.messagesByRoom[roomId];
      if (list) {
        const msg = list.find((m) => m.id === messageId);
        if (msg) {
          msg.content = newContent;
          msg.editedAtUtc = editedAtUtc;
        }
      }
    },
    // Nhận sự kiện xóa tin nhắn từ SignalR
    messageDeletedReceived: (state, action) => {
      const rawRoomId = action.payload.roomId || action.payload.RoomId;
      const messageId = action.payload.messageId || action.payload.MessageId;
      if (!rawRoomId) return;
      const roomId = rawRoomId.toString().toLowerCase();

      if (state.messagesByRoom[roomId]) {
        state.messagesByRoom[roomId] = state.messagesByRoom[roomId].filter(
          (m) => m.id !== messageId
        );
      }
    },
    // Cập nhật trạng thái typing của user khác với roomId và fields chuẩn hóa
    userTypingReceived: (state, action) => {
      const payload = action.payload;
      const rawRoomId = payload.roomId || payload.RoomId;
      if (!rawRoomId) return;
      const roomId = rawRoomId.toString().toLowerCase();
      const userId = payload.userId || payload.UserId;
      const displayName = payload.displayName || payload.DisplayName;
      const isTyping = payload.isTyping ?? payload.IsTyping;

      if (!state.typingUsers[roomId]) {
        state.typingUsers[roomId] = {};
      }
      if (isTyping) {
        state.typingUsers[roomId][userId] = {
          displayName,
          timestamp: Date.now(),
        };
      } else {
        delete state.typingUsers[roomId][userId];
      }
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(fetchMessages.pending, (state) => {
        state.loading = true;
      })
      .addCase(fetchMessages.fulfilled, (state, action) => {
        state.loading = false;
        const { roomId: rawRoomId, messages } = action.payload;
        const roomId = rawRoomId ? rawRoomId.toString().toLowerCase() : '';
        state.messagesByRoom[roomId] = messages;
      })
      .addCase(fetchMessages.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      })
      .addCase(sendMessage.fulfilled, (state, action) => {
        const message = action.payload;
        const rawRoomId = message.roomId || message.RoomId;
        if (!rawRoomId) return;
        const roomId = rawRoomId.toString().toLowerCase();

        if (!state.messagesByRoom[roomId]) {
          state.messagesByRoom[roomId] = [];
        }
        const exists = state.messagesByRoom[roomId].some((m) => m.id === message.id);
        if (!exists) {
          state.messagesByRoom[roomId].push(message);
        }
      })
      .addCase(uploadMedia.fulfilled, (state, action) => {
        const message = action.payload;
        const rawRoomId = message.roomId || message.RoomId;
        if (!rawRoomId) return;
        const roomId = rawRoomId.toString().toLowerCase();

        if (!state.messagesByRoom[roomId]) {
          state.messagesByRoom[roomId] = [];
        }
        const exists = state.messagesByRoom[roomId].some((m) => m.id === message.id);
        if (!exists) {
          state.messagesByRoom[roomId].push(message);
        }
      });
  },
});

export const {
  setActiveRoom,
  messageReceived,
  messageRecalledReceived,
  messageEditedReceived,
  messageDeletedReceived,
  userTypingReceived,
} = chatSlice.actions;

export default chatSlice.reducer;
