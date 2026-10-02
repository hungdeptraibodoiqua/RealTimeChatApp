import React, { useState, useEffect } from 'react';
import { friendService } from '../../services/friendService';
import { userService } from '../../services/userService';
import Modal from '../../components/common/Modal';
import Button from '../../components/common/Button';

/**
 * Modal quản lý danh sách bạn bè và lời mời kết bạn:
 * - Xem danh sách bạn bè hiện tại
 * - Xem và duyệt các lời mời kết bạn gửi tới mình
 * - Tìm kiếm người dùng theo username/tên và gửi lời mời kết bạn
 */
export default function FriendModal({ onClose }) {
  const [tab, setTab] = useState('friends'); // 'friends' | 'requests' | 'search'
  const [friends, setFriends] = useState([]);
  const [requests, setRequests] = useState([]);
  const [searchQuery, setSearchQuery] = useState('');
  const [searchResults, setSearchResults] = useState([]);
  const [loading, setLoading] = useState(false);
  const [feedbackMessage, setFeedbackMessage] = useState('');

  const loadData = async () => {
    setLoading(true);
    try {
      const [friendsData, requestsData] = await Promise.all([
        friendService.getMyFriends(),
        friendService.getPendingRequests(),
      ]);
      setFriends(friendsData);
      setRequests(requestsData);
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadData();
  }, []);

  const handleSearch = async (e) => {
    const q = e.target.value;
    setSearchQuery(q);
    if (!q.trim()) {
      setSearchResults([]);
      return;
    }
    try {
      const results = await userService.searchUsers(q);
      setSearchResults(results);
    } catch (err) {
      console.error(err);
    }
  };

  const handleSendRequest = async (targetUserId) => {
    try {
      await friendService.sendRequest(targetUserId);
      setFeedbackMessage('Đã gửi lời mời kết bạn thành công!');
      setTimeout(() => setFeedbackMessage(''), 3000);
    } catch (err) {
      setFeedbackMessage(err.message || 'Không thể gửi lời mời.');
    }
  };

  const handleAccept = async (requestId) => {
    try {
      await friendService.acceptRequest(requestId);
      setRequests((prev) => prev.filter((r) => r.id !== requestId));
      await loadData();
      setFeedbackMessage('Đã đồng ý kết bạn!');
      setTimeout(() => setFeedbackMessage(''), 3000);
    } catch (err) {
      setFeedbackMessage(err.message || 'Lỗi khi chấp nhận.');
    }
  };

  const handleReject = async (requestId) => {
    try {
      await friendService.rejectRequest(requestId);
      setRequests((prev) => prev.filter((r) => r.id !== requestId));
      setFeedbackMessage('Đã từ chối lời mời.');
      setTimeout(() => setFeedbackMessage(''), 3000);
    } catch (err) {
      setFeedbackMessage(err.message || 'Lỗi khi từ chối.');
    }
  };

  return (
    <Modal title="Bạn Bè & Lời Mời Kết Bạn" onClose={onClose}>
      {feedbackMessage && <div style={styles.feedbackBanner}>{feedbackMessage}</div>}

      {/* Tabs */}
      <div style={styles.tabBar}>
        <button
          onClick={() => setTab('friends')}
          style={{ ...styles.tabBtn, borderBottom: tab === 'friends' ? '2px solid #2563eb' : 'none', color: tab === 'friends' ? '#2563eb' : '#64748b' }}
        >
          Bạn bè ({friends.length})
        </button>
        <button
          onClick={() => setTab('requests')}
          style={{ ...styles.tabBtn, borderBottom: tab === 'requests' ? '2px solid #2563eb' : 'none', color: tab === 'requests' ? '#2563eb' : '#64748b' }}
        >
          Lời mời ({requests.length})
        </button>
        <button
          onClick={() => setTab('search')}
          style={{ ...styles.tabBtn, borderBottom: tab === 'search' ? '2px solid #2563eb' : 'none', color: tab === 'search' ? '#2563eb' : '#64748b' }}
        >
          🔍 Thêm bạn mới
        </button>
      </div>

      <div style={styles.contentArea}>
        {/* Tab 1: Danh sách bạn bè */}
        {tab === 'friends' && (
          <div>
            {friends.length === 0 ? (
              <div style={styles.emptyText}>Chưa có bạn bè nào. Hãy sang tab "Thêm bạn mới" để tìm kiếm!</div>
            ) : (
              friends.map((friend) => (
                <div key={friend.id} style={styles.userRow}>
                  <div style={styles.avatar}>{friend.displayName?.[0]?.toUpperCase()}</div>
                  <div style={{ flex: 1 }}>
                    <div style={styles.userName}>{friend.displayName}</div>
                    <div style={styles.userSub}>@{friend.username}</div>
                  </div>
                </div>
              ))
            )}
          </div>
        )}

        {/* Tab 2: Lời mời kết bạn */}
        {tab === 'requests' && (
          <div>
            {requests.length === 0 ? (
              <div style={styles.emptyText}>Không có lời mời kết bạn nào đang chờ.</div>
            ) : (
              requests.map((req) => (
                <div key={req.id} style={styles.userRow}>
                  <div style={styles.avatar}>{req.displayName?.[0]?.toUpperCase()}</div>
                  <div style={{ flex: 1 }}>
                    <div style={styles.userName}>{req.displayName}</div>
                    <div style={styles.userSub}>@{req.username}</div>
                  </div>
                  <div style={{ display: 'flex', gap: '6px' }}>
                    <button onClick={() => handleAccept(req.id)} style={styles.acceptBtn}>
                      Đồng ý
                    </button>
                    <button onClick={() => handleReject(req.id)} style={styles.rejectBtn}>
                      Từ chối
                    </button>
                  </div>
                </div>
              ))
            )}
          </div>
        )}

        {/* Tab 3: Tìm kiếm người dùng để kết bạn */}
        {tab === 'search' && (
          <div>
            <input
              type="text"
              placeholder="Nhập username hoặc tên người dùng..."
              value={searchQuery}
              onChange={handleSearch}
              style={styles.searchInput}
            />
            <div style={{ marginTop: '12px' }}>
              {searchResults.length === 0 ? (
                <div style={styles.emptyText}>
                  {searchQuery ? 'Không tìm thấy người dùng phù hợp.' : 'Gõ từ khóa để bắt đầu tìm kiếm.'}
                </div>
              ) : (
                searchResults.map((user) => (
                  <div key={user.id} style={styles.userRow}>
                    <div style={styles.avatar}>{user.displayName?.[0]?.toUpperCase()}</div>
                    <div style={{ flex: 1 }}>
                      <div style={styles.userName}>{user.displayName}</div>
                      <div style={styles.userSub}>@{user.username}</div>
                    </div>
                    <button onClick={() => handleSendRequest(user.id)} style={styles.addFriendBtn}>
                      + Kết bạn
                    </button>
                  </div>
                ))
              )}
            </div>
          </div>
        )}
      </div>

      <div style={{ display: 'flex', justifyContent: 'flex-end', marginTop: '16px' }}>
        <Button variant="secondary" onClick={onClose}>
          Đóng
        </Button>
      </div>
    </Modal>
  );
}

const styles = {
  tabBar: {
    display: 'flex',
    borderBottom: '1px solid #e2e8f0',
    marginBottom: '14px',
  },
  tabBtn: {
    background: 'none',
    border: 'none',
    padding: '8px 16px',
    cursor: 'pointer',
    fontWeight: '600',
    fontSize: '13px',
  },
  contentArea: {
    minHeight: '200px',
    maxHeight: '340px',
    overflowY: 'auto',
  },
  feedbackBanner: {
    backgroundColor: '#ecfdf5',
    color: '#059669',
    padding: '8px 12px',
    borderRadius: '6px',
    fontSize: '13px',
    marginBottom: '10px',
    border: '1px solid #a7f3d0',
  },
  emptyText: {
    color: '#94a3b8',
    textAlign: 'center',
    padding: '24px 0',
    fontSize: '13px',
  },
  userRow: {
    display: 'flex',
    alignItems: 'center',
    gap: '12px',
    padding: '8px 0',
    borderBottom: '1px solid #f1f5f9',
  },
  avatar: {
    width: '36px',
    height: '36px',
    borderRadius: '50%',
    backgroundColor: '#818cf8',
    color: '#ffffff',
    display: 'flex',
    alignItems: 'center',
    justifyContent: 'center',
    fontWeight: '600',
    fontSize: '14px',
  },
  userName: {
    fontWeight: '500',
    fontSize: '13px',
    color: '#1e293b',
  },
  userSub: {
    fontSize: '12px',
    color: '#64748b',
  },
  acceptBtn: {
    backgroundColor: '#2563eb',
    color: '#ffffff',
    border: 'none',
    borderRadius: '4px',
    padding: '5px 12px',
    fontSize: '12px',
    fontWeight: '500',
    cursor: 'pointer',
  },
  rejectBtn: {
    backgroundColor: '#f1f5f9',
    color: '#64748b',
    border: 'none',
    borderRadius: '4px',
    padding: '5px 12px',
    fontSize: '12px',
    cursor: 'pointer',
  },
  addFriendBtn: {
    backgroundColor: '#10b981',
    color: '#ffffff',
    border: 'none',
    borderRadius: '4px',
    padding: '5px 12px',
    fontSize: '12px',
    fontWeight: '500',
    cursor: 'pointer',
  },
  searchInput: {
    width: '100%',
    padding: '8px 12px',
    borderRadius: '6px',
    border: '1px solid #cbd5e1',
    fontSize: '13px',
    boxSizing: 'border-box',
    outline: 'none',
  },
};
