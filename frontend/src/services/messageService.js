import apiClient from './apiClient';

/**
 * Service giao tiếp với MessagesController của Backend .NET.
 * Phục vụ các nghiệp vụ: Lấy lịch sử tin nhắn, gửi tin nhắn text, upload media (ảnh/video), thu hồi, sửa và xóa tin nhắn.
 */
export const messageService = {
  // Lấy danh sách tin nhắn theo phòng có phân trang
  async getRoomMessages(roomId, skip = 0, take = 50) {
    const res = await apiClient.get(`/api/messages/${roomId}?skip=${skip}&take=${take}`);
    return res.data;
  },

  // Gửi tin nhắn văn bản vào phòng chat
  async sendMessage(data) {
    const res = await apiClient.post('/api/messages', data);
    return res.data;
  },

  // Upload file ảnh hoặc video từ máy client lên server
  async uploadMedia(file, roomId) {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('roomId', roomId);

    const res = await apiClient.post('/api/messages/upload', formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
    return res.data;
  },

  // Thu hồi tin nhắn trong vòng 1 tiếng kể từ lúc gửi
  async recallMessage(messageId) {
    const res = await apiClient.put(`/api/messages/${messageId}/recall`);
    return res.data;
  },

  // Chỉnh sửa nội dung tin nhắn văn bản trong vòng 1 tiếng
  async editMessage(messageId, newContent) {
    const res = await apiClient.put(`/api/messages/${messageId}`, { newContent });
    return res.data;
  },

  // Xóa tin nhắn khỏi cuộc trò chuyện
  async deleteMessage(messageId) {
    const res = await apiClient.delete(`/api/messages/${messageId}`);
    return res.data;
  },
};
