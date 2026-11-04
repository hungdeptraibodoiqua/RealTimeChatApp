import apiClient from './apiClient';

/**
 * Service giao tiếp với UsersController của Backend .NET.
 * Phục vụ tìm kiếm người dùng theo từ khóa để kết bạn hoặc mời vào phòng chat.
 */
export const userService = {
  async searchUsers(query) {
    const res = await apiClient.get(`/api/users/search?q=${encodeURIComponent(query)}`);
    return res.data;
  },
};
