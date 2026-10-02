import React, { useEffect, useState, useRef } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { useNavigate } from 'react-router-dom';
import { logout } from '../features/auth/authSlice';
import { fetchRooms } from '../features/rooms/roomsSlice';
import {
  fetchMessages,
  sendMessage,
  uploadMedia,
  recallMessage,
  editMessage,
  deleteMessage,
  setActiveRoom,
  messageReceived,
  messageRecalledReceived,
  messageEditedReceived,
  messageDeletedReceived,
  userTypingReceived,
} from '../features/chat/chatSlice';
import { userOnlineStatusChanged, ownerTransferredReceived } from '../features/rooms/roomsSlice';
import { signalrService } from '../services/signalrService';

import MessageList from '../features/chat/MessageList';
import MessageInput from '../features/chat/MessageInput';
import TypingIndicator from '../features/chat/TypingIndicator';
import CreateRoomModal from '../features/rooms/CreateRoomModal';
import RoomSettingsModal from '../features/rooms/RoomSettingsModal';
import FriendModal from '../features/friends/FriendModal';

/**
 * Trang chính Real-Time Chat App:
 * - Quản lý kết nối SignalR Hub tập trung và đăng ký toàn bộ listeners.
 * - Sidebar danh sách phòng chat, nút Tạo phòng, nút Quản lý Bạn bè.
 * - Khung chat realtime: gửi tin nhắn text, upload media, hiển thị typing, thu hồi/sửa trong 1 giờ.
 */
export default function ChatPage() {
  const dispatch = useDispatch();
  const navigate = useNavigate();

  const { user } = useSelector((state) => state.auth);
  const { list: rooms, loading: roomsLoading, onlineUsers } = useSelector((state) => state.rooms);
  const { messagesByRoom, activeRoomId, typingUsers } = useSelector((state) => state.chat);

  const [isSignalrConnected, setIsSignalrConnected] = useState(false);
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [showSettingsModal, setShowSettingsModal] = useState(false);
  const [showFriendModal, setShowFriendModal] = useState(false);

  const activeRoom = rooms.find((r) => r.id === activeRoomId);
  const currentMessages = activeRoomId ? messagesByRoom[activeRoomId] || [] : [];
  const currentTyping = activeRoomId ? typingUsers[activeRoomId] || {} : {};

  // 1. Khởi tạo SignalR Connection và đăng ký các listeners realtime
  useEffect(() => {
    let isMounted = true;

    const setupSignalR = async () => {
      try {
        const connection = await signalrService.startConnection();
        if (!isMounted) return;
        setIsSignalrConnected(true);

        // Lắng nghe sự kiện Online/Offline
        connection.on('UserIsOnline', (userId) => {
          dispatch(userOnlineStatusChanged({ userId, isOnline: true }));
        });
        connection.on('UserIsOffline', (userId) => {
          dispatch(userOnlineStatusChanged({ userId, isOnline: false }));
        });

        // Lắng nghe tin nhắn mới từ phòng
        connection.on('ReceiveMessage', (message) => {
          dispatch(messageReceived(message));
        });

        // Lắng nghe thu hồi tin nhắn
        connection.on('MessageRecalled', ({ roomId, messageId }) => {
          dispatch(messageRecalledReceived({ roomId, messageId }));
        });

        // Lắng nghe sửa tin nhắn
        connection.on('MessageEdited', ({ roomId, messageId, newContent, editedAtUtc }) => {
          dispatch(messageEditedReceived({ roomId, messageId, newContent, editedAtUtc }));
        });

        // Lắng nghe xóa tin nhắn
        connection.on('MessageDeleted', ({ roomId, messageId }) => {
          dispatch(messageDeletedReceived({ roomId, messageId }));
        });

        // Lắng nghe typing indicator
        connection.on('UserTyping', ({ roomId, userId, displayName, isTyping }) => {
          dispatch(userTypingReceived({ roomId, userId, displayName, isTyping }));
        });

        // Lắng nghe đổi chủ phòng (Owner Transferred)
        connection.on('OwnerTransferred', ({ roomId, oldOwnerId, newOwnerId }) => {
          dispatch(ownerTransferredReceived({ roomId, newOwnerId }));
        });
      } catch (err) {
        if (!isMounted) return;
        console.error('Không thể kết nối SignalR:', err);
        setIsSignalrConnected(false);
      }
    };

    setupSignalR();
    dispatch(fetchRooms());

    return () => {
      isMounted = false;
      signalrService.stopConnection();
    };
  }, [dispatch]);

  // 2. Chuyển phòng chat: Rời phòng cũ và Join phòng mới trên Hub
  const handleSelectRoom = async (roomId) => {
    if (activeRoomId === roomId) return;

    if (activeRoomId) {
      await signalrService.leaveRoom(activeRoomId);
    }

    dispatch(setActiveRoom(roomId));
    await signalrService.joinRoom(roomId);
    dispatch(fetchMessages({ roomId }));
  };

  // 3. Các handler tương tác tin nhắn
  const handleSendMessage = (content) => {
    if (!activeRoomId) return;
    dispatch(sendMessage({ roomId: activeRoomId, content }));
  };

  const handleUploadMedia = (file, roomId) => {
    dispatch(uploadMedia({ file, roomId }));
  };

  const handleSendTyping = (roomId, isTyping) => {
    signalrService.sendTyping(roomId, isTyping);
  };

  const handleRecall = (messageId) => {
    if (!activeRoomId) return;
    dispatch(recallMessage({ messageId, roomId: activeRoomId }));
  };

  const handleEdit = (messageId, newContent) => {
    if (!activeRoomId) return;
    dispatch(editMessage({ messageId, newContent, roomId: activeRoomId }));
  };

  const handleDelete = (messageId) => {
    if (!activeRoomId) return;
    if (!window.confirm('Bạn có chắc muốn xóa tin nhắn này?')) return;
    dispatch(deleteMessage({ messageId, roomId: activeRoomId }));
  };

  const handleLogout = () => {
    signalrService.stopConnection();
    dispatch(logout());
    navigate('/login');
  };

  return (
    <div style={styles.container}>
      {/* Header thanh điều hướng trên cùng */}
      <header style={styles.header}>
        <div style={styles.headerLeft}>
          <span style={styles.logo}>💬 Real-Time Chat App</span>
          <div style={styles.connStatus}>
            <span
              style={{
                ...styles.connDot,
                backgroundColor: isSignalrConnected ? '#10b981' : '#ef4444',
              }}
            />
            <span style={styles.connText}>
              {isSignalrConnected ? 'Real-Time Connected' : 'Connecting...'}
            </span>
          </div>
        </div>

        <div style={styles.headerRight}>
          <button onClick={() => setShowFriendModal(true)} style={styles.headerBtn}>
            👥 Bạn bè
          </button>
          <button onClick={() => setShowCreateModal(true)} style={styles.headerBtnPrimary}>
            + Tạo phòng
          </button>

          <div style={styles.userInfo}>
            <div style={styles.userAvatar}>
              {user?.displayName?.[0]?.toUpperCase() || 'U'}
            </div>
            <div style={styles.userMeta}>
              <span style={styles.userName}>{user?.displayName || 'Người dùng'}</span>
              <span style={styles.userRole}>@{user?.username || 'user'}</span>
            </div>
          </div>

          <button onClick={handleLogout} style={styles.logoutBtn}>
            Đăng xuất
          </button>
        </div>
      </header>

      {/* Khu vực nội dung chính: Chia 2 cột Sidebar và Chat Box */}
      <div style={styles.mainLayout}>
        {/* Sidebar bên trái: Danh sách phòng chat */}
        <aside style={styles.sidebar}>
          <div style={styles.sidebarHeader}>
            <span style={styles.sidebarTitle}>Phòng Chat Của Bạn</span>
            <span style={styles.roomCountBadge}>{rooms.length}</span>
          </div>

          <div style={styles.roomList}>
            {roomsLoading ? (
              <div style={styles.loadingText}>Đang tải danh sách phòng...</div>
            ) : rooms.length === 0 ? (
              <div style={styles.emptyRooms}>
                <p>Bạn chưa tham gia phòng nào.</p>
                <button
                  onClick={() => setShowCreateModal(true)}
                  style={styles.inlineCreateBtn}
                >
                  Tạo phòng ngay
                </button>
              </div>
            ) : (
              rooms.map((room) => {
                const isActive = room.id === activeRoomId;
                return (
                  <div
                    key={room.id}
                    onClick={() => handleSelectRoom(room.id)}
                    style={{
                      ...styles.roomItem,
                      backgroundColor: isActive ? '#334155' : 'transparent',
                      borderLeft: isActive ? '3px solid #38bdf8' : '3px solid transparent',
                    }}
                  >
                    <div style={styles.roomAvatar}>
                      {room.name?.[0]?.toUpperCase() || '#'}
                    </div>
                    <div style={styles.roomInfo}>
                      <div style={styles.roomName}>{room.name}</div>
                      <div style={styles.roomMeta}>
                        {room.type === 1 ? 'Direct Chat' : 'Nhóm chat'}
                      </div>
                    </div>
                  </div>
                );
              })
            )}
          </div>
        </aside>

        {/* Khung chat bên phải */}
        <section style={styles.chatSection}>
          {activeRoom ? (
            <div style={styles.activeChatContainer}>
              {/* Header của phòng chat đang mở */}
              <div style={styles.chatHeader}>
                <div style={styles.chatHeaderInfo}>
                  <div style={styles.chatRoomName}>{activeRoom.name}</div>
                  <div style={styles.chatRoomSubtitle}>
                    {activeRoom.type === 1 ? 'Cuộc trò chuyện trực tiếp 1-1' : 'Phòng chat nhóm'}
                  </div>
                </div>

                <div style={styles.chatHeaderActions}>
                  <button
                    onClick={() => setShowSettingsModal(true)}
                    style={styles.settingsBtn}
                    title="Cài đặt phòng & Thành viên"
                  >
                    ⚙️ Quản lý phòng
                  </button>
                </div>
              </div>

              {/* Danh sách tin nhắn */}
              <MessageList
                messages={currentMessages}
                currentUserId={user?.id}
                onRecall={handleRecall}
                onEdit={handleEdit}
                onDelete={handleDelete}
              />

              {/* Typing indicator */}
              <TypingIndicator typingUsers={currentTyping} />

              {/* Thanh nhập tin nhắn */}
              <MessageInput
                roomId={activeRoom.id}
                onSendMessage={handleSendMessage}
                onUploadMedia={handleUploadMedia}
                onSendTyping={handleSendTyping}
              />
            </div>
          ) : (
            <div style={styles.noRoomSelected}>
              <div style={styles.noRoomIcon}>💬</div>
              <h2 style={styles.noRoomTitle}>Chào mừng bạn đến với ChatApp!</h2>
              <p style={styles.noRoomText}>
                Chọn một phòng chat từ danh sách bên trái hoặc tạo phòng mới để bắt đầu nhắn tin realtime.
              </p>
              <button
                onClick={() => setShowCreateModal(true)}
                style={styles.heroCreateBtn}
              >
                + Tạo Phòng Mới
              </button>
            </div>
          )}
        </section>
      </div>

      {/* Các Modals chức năng */}
      {showCreateModal && (
        <CreateRoomModal onClose={() => setShowCreateModal(false)} />
      )}

      {showSettingsModal && activeRoom && (
        <RoomSettingsModal
          room={activeRoom}
          onClose={() => setShowSettingsModal(false)}
        />
      )}

      {showFriendModal && (
        <FriendModal onClose={() => setShowFriendModal(false)} />
      )}
    </div>
  );
}

const styles = {
  container: {
    height: '100vh',
    display: 'flex',
    flexDirection: 'column',
    backgroundColor: '#0f172a',
    color: '#f8fafc',
    overflow: 'hidden',
  },
  header: {
    display: 'flex',
    justifyContent: 'space-between',
    alignItems: 'center',
    padding: '12px 24px',
    backgroundColor: '#1e293b',
    borderBottom: '1px solid #334155',
    flexShrink: 0,
  },
  headerLeft: {
    display: 'flex',
    alignItems: 'center',
    gap: '16px',
  },
  logo: {
    fontSize: '18px',
    fontWeight: '700',
    color: '#38bdf8',
  },
  connStatus: {
    display: 'flex',
    alignItems: 'center',
    gap: '6px',
    backgroundColor: '#0f172a',
    padding: '4px 10px',
    borderRadius: '12px',
    border: '1px solid #334155',
  },
  connDot: {
    width: '8px',
    height: '8px',
    borderRadius: '50%',
  },
  connText: {
    fontSize: '11px',
    color: '#cbd5e1',
  },
  headerRight: {
    display: 'flex',
    alignItems: 'center',
    gap: '14px',
  },
  headerBtn: {
    backgroundColor: '#334155',
    color: '#f8fafc',
    border: 'none',
    borderRadius: '6px',
    padding: '8px 14px',
    fontSize: '13px',
    fontWeight: '500',
    cursor: 'pointer',
  },
  headerBtnPrimary: {
    backgroundColor: '#2563eb',
    color: '#ffffff',
    border: 'none',
    borderRadius: '6px',
    padding: '8px 14px',
    fontSize: '13px',
    fontWeight: '600',
    cursor: 'pointer',
  },
  userInfo: {
    display: 'flex',
    alignItems: 'center',
    gap: '10px',
    paddingLeft: '12px',
    borderLeft: '1px solid #334155',
  },
  userAvatar: {
    width: '32px',
    height: '32px',
    borderRadius: '50%',
    backgroundColor: '#6366f1',
    color: '#ffffff',
    display: 'flex',
    alignItems: 'center',
    justifyContent: 'center',
    fontWeight: '600',
    fontSize: '13px',
  },
  userMeta: {
    display: 'flex',
    flexDirection: 'column',
  },
  userName: {
    fontSize: '13px',
    fontWeight: '600',
    color: '#f8fafc',
  },
  userRole: {
    fontSize: '11px',
    color: '#94a3b8',
  },
  logoutBtn: {
    backgroundColor: 'transparent',
    color: '#f87171',
    border: '1px solid #7f1d1d',
    borderRadius: '6px',
    padding: '7px 12px',
    fontSize: '12px',
    cursor: 'pointer',
  },
  mainLayout: {
    flex: 1,
    display: 'flex',
    overflow: 'hidden',
  },
  sidebar: {
    width: '300px',
    backgroundColor: '#1e293b',
    borderRight: '1px solid #334155',
    display: 'flex',
    flexDirection: 'column',
    flexShrink: 0,
  },
  sidebarHeader: {
    display: 'flex',
    justifyContent: 'space-between',
    alignItems: 'center',
    padding: '14px 18px',
    borderBottom: '1px solid #334155',
  },
  sidebarTitle: {
    fontSize: '13px',
    fontWeight: '600',
    color: '#94a3b8',
    textTransform: 'uppercase',
    letterSpacing: '0.5px',
  },
  roomCountBadge: {
    backgroundColor: '#334155',
    color: '#38bdf8',
    padding: '2px 8px',
    borderRadius: '10px',
    fontSize: '11px',
    fontWeight: '600',
  },
  roomList: {
    flex: 1,
    overflowY: 'auto',
  },
  loadingText: {
    padding: '20px',
    textAlign: 'center',
    color: '#64748b',
    fontSize: '13px',
  },
  emptyRooms: {
    padding: '28px 16px',
    textAlign: 'center',
    color: '#64748b',
    fontSize: '13px',
  },
  inlineCreateBtn: {
    marginTop: '10px',
    backgroundColor: '#2563eb',
    color: '#ffffff',
    border: 'none',
    borderRadius: '6px',
    padding: '6px 12px',
    fontSize: '12px',
    cursor: 'pointer',
  },
  roomItem: {
    display: 'flex',
    alignItems: 'center',
    gap: '12px',
    padding: '12px 18px',
    cursor: 'pointer',
    transition: 'background-color 0.15s',
  },
  roomAvatar: {
    width: '38px',
    height: '38px',
    borderRadius: '10px',
    backgroundColor: '#3b82f6',
    color: '#ffffff',
    display: 'flex',
    alignItems: 'center',
    justifyContent: 'center',
    fontWeight: '700',
    fontSize: '15px',
    flexShrink: 0,
  },
  roomInfo: {
    flex: 1,
    overflow: 'hidden',
  },
  roomName: {
    fontSize: '14px',
    fontWeight: '500',
    color: '#f8fafc',
    whiteSpace: 'nowrap',
    overflow: 'hidden',
    textOverflow: 'ellipsis',
  },
  roomMeta: {
    fontSize: '11px',
    color: '#94a3b8',
    marginTop: '2px',
  },
  chatSection: {
    flex: 1,
    display: 'flex',
    flexDirection: 'column',
    backgroundColor: '#0f172a',
    overflow: 'hidden',
  },
  activeChatContainer: {
    flex: 1,
    display: 'flex',
    flexDirection: 'column',
    overflow: 'hidden',
  },
  chatHeader: {
    display: 'flex',
    justifyContent: 'space-between',
    alignItems: 'center',
    padding: '12px 24px',
    backgroundColor: '#1e293b',
    borderBottom: '1px solid #334155',
    flexShrink: 0,
  },
  chatHeaderInfo: {
    display: 'flex',
    flexDirection: 'column',
  },
  chatRoomName: {
    fontSize: '16px',
    fontWeight: '600',
    color: '#f8fafc',
  },
  chatRoomSubtitle: {
    fontSize: '12px',
    color: '#94a3b8',
    marginTop: '2px',
  },
  chatHeaderActions: {
    display: 'flex',
    alignItems: 'center',
    gap: '10px',
  },
  settingsBtn: {
    backgroundColor: '#334155',
    color: '#f8fafc',
    border: '1px solid #475569',
    borderRadius: '6px',
    padding: '6px 14px',
    fontSize: '13px',
    cursor: 'pointer',
  },
  noRoomSelected: {
    flex: 1,
    display: 'flex',
    flexDirection: 'column',
    alignItems: 'center',
    justifyContent: 'center',
    padding: '40px',
    textAlign: 'center',
  },
  noRoomIcon: {
    fontSize: '64px',
    marginBottom: '16px',
  },
  noRoomTitle: {
    fontSize: '22px',
    fontWeight: '700',
    color: '#38bdf8',
    marginBottom: '8px',
  },
  noRoomText: {
    fontSize: '14px',
    color: '#94a3b8',
    maxWidth: '420px',
    lineHeight: '1.5',
    marginBottom: '24px',
  },
  heroCreateBtn: {
    backgroundColor: '#2563eb',
    color: '#ffffff',
    border: 'none',
    borderRadius: '8px',
    padding: '12px 24px',
    fontSize: '14px',
    fontWeight: '600',
    cursor: 'pointer',
  },
};
