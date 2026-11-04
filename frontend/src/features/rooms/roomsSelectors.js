export const selectRooms = (state) => state.rooms.list
export const selectRoomsLoading = (state) => state.rooms.loading
export const selectOnlineUsers = (state) => state.rooms.onlineUsers
export const selectIsUserOnline = (userId) => (state) => !!state.rooms.onlineUsers[userId]
