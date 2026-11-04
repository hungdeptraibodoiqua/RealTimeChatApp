// Selectors cho chat state
export const selectActiveRoomId = (state) => state.chat.activeRoomId;
export const selectMessagesByRoom = (roomId) => (state) => state.chat.messagesByRoom[roomId] || [];
export const selectTypingUsersByRoom = (roomId) => (state) => state.chat.typingUsers[roomId] || {};
export const selectChatLoading = (state) => state.chat.loading;
export const selectChatError = (state) => state.chat.error;
