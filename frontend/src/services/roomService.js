import apiClient from './apiClient';

/**
 * Service giao tiếp với RoomsController của Backend .NET.
 * Phục vụ các nghiệp vụ: Lấy phòng, tạo phòng, xem thành viên, thêm/xóa thành viên, chuyển Owner, rời phòng.
 */
export const roomService = {
  // Lấy danh sách các phòng mà user hiện tại đang tham gia
  async getMyRooms() {
    const res = await apiClient.get('/api/rooms');
    return res.data;
  },

  // Tạo phòng mới (Public/Group/Direct)
  async createRoom(data) {
    const res = await apiClient.post('/api/rooms', data);
    return res.data;
  },

  // Lấy danh sách thành viên trong phòng kèm vai trò (Owner/Admin/Member)
  async getMembers(roomId) {
    const res = await apiClient.get(`/api/rooms/${roomId}/members`);
    return res.data;
  },

  // Thêm thành viên mới vào phòng (chỉ Owner/Admin)
  async addMember(roomId, userId) {
    const res = await apiClient.post(`/api/rooms/${roomId}/members`, { userId });
    return res.data;
  },

  // Xóa (kick) thành viên khỏi phòng (chỉ Owner/Admin)
  async removeMember(roomId, userId) {
    const res = await apiClient.delete(`/api/rooms/${roomId}/members/${userId}`);
    return res.data;
  },

  // Chủ phòng chủ động chuyển quyền Owner cho một thành viên khác
  async transferOwner(roomId, newOwnerUserId) {
    const res = await apiClient.post(`/api/rooms/${roomId}/transfer-owner`, { newOwnerUserId });
    return res.data;
  },

  // Rời phòng (nếu là Owner, hệ thống tự động gán Owner mới)
  async leaveRoom(roomId) {
    const res = await apiClient.post(`/api/rooms/${roomId}/leave`);
    return res.data;
  },

  // Xóa phòng hoàn toàn (chỉ Owner)
  async deleteRoom(roomId) {
    const res = await apiClient.delete(`/api/rooms/${roomId}`);
    return res.data;
  },
};
