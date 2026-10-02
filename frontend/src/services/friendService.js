import apiClient from './apiClient';

/**
 * Service giao tiếp với FriendsController của Backend .NET.
 * Phục vụ các nghiệp vụ: Lấy danh sách bạn bè, xem lời mời, gửi lời mời kết bạn, chấp nhận hoặc từ chối.
 */
export const friendService = {
  // Lấy danh sách bạn bè đã được chấp nhận
  async getMyFriends() {
    const res = await apiClient.get('/api/friends');
    return res.data;
  },

  // Lấy danh sách các lời mời kết bạn đang chờ duyệt
  async getPendingRequests() {
    const res = await apiClient.get('/api/friends/requests');
    return res.data;
  },

  // Gửi lời mời kết bạn tới một user theo UserId
  async sendRequest(targetUserId) {
    const res = await apiClient.post('/api/friends/request', { targetUserId });
    return res.data;
  },

  // Chấp nhận lời mời kết bạn
  async acceptRequest(friendshipId) {
    const res = await apiClient.post(`/api/friends/${friendshipId}/accept`);
    return res.data;
  },

  // Từ chối lời mời kết bạn
  async rejectRequest(friendshipId) {
    const res = await apiClient.post(`/api/friends/${friendshipId}/reject`);
    return res.data;
  },
};
