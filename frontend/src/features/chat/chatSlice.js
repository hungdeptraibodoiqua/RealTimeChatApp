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
      state.activeRoomId = action.payload;
    },
    // Nhận tin nhắn mới từ SignalR
    messageReceived: (state, action) => {
      const message = action.payload;
      const roomId = message.roomId;
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
      const { roomId, messageId } = action.payload;
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
      const { roomId, messageId, newContent, editedAtUtc } = action.payload;
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
      const { roomId, messageId } = action.payload;
      if (state.messagesByRoom[roomId]) {
        state.messagesByRoom[roomId] = state.messagesByRoom[roomId].filter(
          (m) => m.id !== messageId
        );
      }
    },
    // Cập nhật trạng thái typing của user khác
    userTypingReceived: (state, action) => {
      const { roomId, userId, displayName, isTyping } = action.payload;
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
        const { roomId, messages } = action.payload;
        state.messagesByRoom[roomId] = messages;
      })
      .addCase(fetchMessages.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      })
      .addCase(sendMessage.fulfilled, (state, action) => {
        const message = action.payload;
        const roomId = message.roomId;
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
        const roomId = message.roomId;
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
