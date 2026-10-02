import React, { useState, useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { deleteRoom, leaveRoom } from './roomsSlice';
import { selectCurrentUser } from '../auth/authSelectors';
import { roomService } from '../../services/roomService';
import { userService } from '../../services/userService';
import { MEMBER_ROLE } from '../../utils/constants';
import Modal from '../../components/common/Modal';
import Button from '../../components/common/Button';

/**
 * Modal quản lý cài đặt phòng chat:
 * - Xem danh sách thành viên và vai trò
 * - Thêm thành viên mới vào phòng
 * - Xóa thành viên (kick)
 * - Chuyển quyền Chủ phòng (Owner) cho thành viên khác
 * - Rời phòng (tự động chuyển quyền Owner cho người kế nhiệm)
 * - Xóa phòng (dành riêng cho Owner)
 */
export default function RoomSettingsModal({ room, onClose }) {
  const dispatch = useDispatch();
  const currentUser = useSelector(selectCurrentUser);
  const [members, setMembers] = useState([]);
  const [searchUserQuery, setSearchUserQuery] = useState('');
  const [searchResults, setSearchResults] = useState([]);
  const [isSearching, setIsSearching] = useState(false);
  const [actionError, setActionError] = useState('');

  const myRole = members.find((m) => m.userId === currentUser?.id)?.role;
  const isOwner = myRole === MEMBER_ROLE.OWNER;
  const canManage = isOwner || myRole === MEMBER_ROLE.ADMIN;

  const loadMembers = async () => {
    try {
      const data = await roomService.getMembers(room.id);
      setMembers(data);
    } catch (err) {
      setActionError(err.message || 'Không thể tải danh sách thành viên.');
    }
  };

  useEffect(() => {
    loadMembers();
  }, [room.id]);

  const handleSearchUsers = async (e) => {
    const q = e.target.value;
    setSearchUserQuery(q);
    if (!q.trim()) {
      setSearchResults([]);
      return;
    }
    setIsSearching(true);
    try {
      const results = await userService.searchUsers(q);
      // Lọc các user đã có trong phòng
      const existingIds = new Set(members.map((m) => m.userId));
      setSearchResults(results.filter((u) => !existingIds.has(u.id)));
    } catch (err) {
      console.error(err);
    } finally {
      setIsSearching(false);
    }
  };

  const handleAddMember = async (userId) => {
    setActionError('');
    try {
      await roomService.addMember(room.id, userId);
      setSearchUserQuery('');
      setSearchResults([]);
      await loadMembers();
    } catch (err) {
      setActionError(err.message || 'Không thể thêm thành viên.');
    }
  };

  const handleRemoveMember = async (userId) => {
    if (!window.confirm('Bạn có chắc muốn xóa thành viên này khỏi phòng?')) return;
    setActionError('');
    try {
      await roomService.removeMember(room.id, userId);
      setMembers((prev) => prev.filter((m) => m.userId !== userId));
    } catch (err) {
      setActionError(err.message || 'Không thể xóa thành viên.');
    }
  };

  const handleTransferOwner = async (newOwnerUserId, newOwnerName) => {
    if (!window.confirm(`Bạn có chắc muốn chuyển quyền Chủ phòng cho ${newOwnerName}? Bạn sẽ trở thành Quản trị viên.`)) {
      return;
    }
    setActionError('');
    try {
      await roomService.transferOwner(room.id, newOwnerUserId);
      await loadMembers();
      alert('Đã chuyển quyền Chủ phòng thành công!');
    } catch (err) {
      setActionError(err.message || 'Không thể chuyển quyền chủ phòng.');
    }
  };

  const handleLeaveRoom = async () => {
    const confirmMsg = isOwner
      ? 'Bạn là Chủ phòng. Khi bạn rời phòng, quyền Chủ phòng sẽ được tự động chuyển cho Quản trị viên hoặc thành viên tham gia lâu nhất. Bạn có chắc muốn tiếp tục?'
      : 'Bạn có chắc chắn muốn rời khỏi phòng chat này?';

    if (!window.confirm(confirmMsg)) return;

    setActionError('');
    try {
      await dispatch(leaveRoom(room.id)).unwrap();
      onClose();
    } catch (err) {
      setActionError(err || 'Không thể rời phòng.');
    }
  };

  const handleDeleteRoom = async () => {
    if (!window.confirm('CẢNH BÁO: Xóa phòng sẽ xóa toàn bộ tin nhắn vĩnh viễn. Bạn có chắc chắn muốn xóa?')) return;
    setActionError('');
    try {
      await dispatch(deleteRoom(room.id)).unwrap();
      onClose();
    } catch (err) {
      setActionError(err || 'Không thể xóa phòng.');
    }
  };

  const getRoleBadge = (role) => {
    switch (role) {
      case MEMBER_ROLE.OWNER:
        return <span style={styles.badgeOwner}>👑 Chủ phòng</span>;
      case MEMBER_ROLE.ADMIN:
        return <span style={styles.badgeAdmin}>⭐ Quản trị viên</span>;
      default:
        return <span style={styles.badgeMember}>Thành viên</span>;
    }
  };

  return (
    <Modal title={`Cài đặt: ${room.name}`} onClose={onClose}>
      {actionError && <div style={styles.errorBanner}>{actionError}</div>}

      {/* Phần thêm thành viên mới */}
      {canManage && (
        <div style={styles.addSection}>
          <div style={styles.sectionHeader}>Thêm thành viên mới</div>
          <input
            type="text"
            placeholder="Tìm theo username hoặc tên..."
            value={searchUserQuery}
            onChange={handleSearchUsers}
            style={styles.searchInput}
          />
          {searchResults.length > 0 && (
            <div style={styles.searchResultsList}>
              {searchResults.map((user) => (
                <div key={user.id} style={styles.searchResultItem}>
                  <div>
                    <span style={styles.searchName}>{user.displayName}</span>
                    <span style={styles.searchUsername}> (@{user.username})</span>
                  </div>
                  <button onClick={() => handleAddMember(user.id)} style={styles.addBtn}>
                    + Thêm
                  </button>
                </div>
              ))}
            </div>
          )}
        </div>
      )}

      {/* Danh sách thành viên */}
      <div style={styles.sectionHeader}>Thành viên trong phòng ({members.length})</div>
      <div style={styles.memberList}>
        {members.map((member) => (
          <div key={member.userId} style={styles.memberRow}>
            <div style={styles.avatar}>{member.displayName?.[0]?.toUpperCase() || 'U'}</div>
            <div style={{ flex: 1 }}>
              <div style={styles.memberNameRow}>
                <span style={styles.name}>{member.displayName}</span>
                {getRoleBadge(member.role)}
              </div>
              <div style={styles.subText}>@{member.username}</div>
            </div>

            {/* Các hành động quản trị */}
            <div style={styles.memberActions}>
              {/* Nút chuyển quyền Owner: Chỉ Owner hiện tại thấy trên các thành viên khác */}
              {isOwner && member.userId !== currentUser?.id && (
                <button
                  onClick={() => handleTransferOwner(member.userId, member.displayName)}
                  style={styles.transferBtn}
                  title="Chuyển quyền Chủ phòng"
                >
                  👑 Nhượng quyền
                </button>
              )}

              {/* Nút xóa thành viên: Owner/Admin xóa member, Admin không thể xóa Owner */}
              {canManage && member.role !== MEMBER_ROLE.OWNER && member.userId !== currentUser?.id && (
                <button
                  onClick={() => handleRemoveMember(member.userId)}
                  style={styles.kickBtn}
                  title="Xóa khỏi phòng"
                >
                  Xóa
                </button>
              )}
            </div>
          </div>
        ))}
      </div>

      {/* Footer các hành động nguy hiểm: Rời phòng & Xóa phòng */}
      <div style={styles.footerActions}>
        <Button variant="secondary" onClick={handleLeaveRoom}>
          🚪 Rời phòng
        </Button>

        {isOwner && (
          <Button variant="danger" onClick={handleDeleteRoom}>
            🗑️ Xóa phòng
          </Button>
        )}
      </div>
    </Modal>
  );
}

const styles = {
  errorBanner: {
    backgroundColor: '#fef2f2',
    color: '#dc2626',
    padding: '8px 12px',
    borderRadius: '6px',
    fontSize: '13px',
    marginBottom: '12px',
    border: '1px solid #fecaca',
  },
  sectionHeader: {
    fontWeight: '600',
    fontSize: '13px',
    color: '#64748b',
    marginBottom: '8px',
    marginTop: '12px',
    textTransform: 'uppercase',
    letterSpacing: '0.5px',
  },
  addSection: {
    marginBottom: '16px',
    paddingBottom: '12px',
    borderBottom: '1px solid #e2e8f0',
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
  searchResultsList: {
    maxHeight: '140px',
    overflowY: 'auto',
    backgroundColor: '#f8fafc',
    border: '1px solid #e2e8f0',
    borderRadius: '6px',
    marginTop: '6px',
  },
  searchResultItem: {
    display: 'flex',
    alignItems: 'center',
    justifyContent: 'space-between',
    padding: '8px 12px',
    borderBottom: '1px solid #f1f5f9',
  },
  searchName: {
    fontWeight: '500',
    fontSize: '13px',
    color: '#1e293b',
  },
  searchUsername: {
    fontSize: '12px',
    color: '#64748b',
  },
  addBtn: {
    backgroundColor: '#2563eb',
    color: '#ffffff',
    border: 'none',
    borderRadius: '4px',
    padding: '4px 10px',
    fontSize: '12px',
    cursor: 'pointer',
  },
  memberList: {
    maxHeight: '260px',
    overflowY: 'auto',
  },
  memberRow: {
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
    backgroundColor: '#6366f1',
    color: '#ffffff',
    display: 'flex',
    alignItems: 'center',
    justifyContent: 'center',
    fontWeight: '600',
    flexShrink: 0,
    fontSize: '14px',
  },
  memberNameRow: {
    display: 'flex',
    alignItems: 'center',
    gap: '8px',
  },
  name: {
    fontWeight: '500',
    fontSize: '14px',
    color: '#1e293b',
  },
  subText: {
    fontSize: '12px',
    color: '#94a3b8',
  },
  badgeOwner: {
    fontSize: '10px',
    backgroundColor: '#fef3c7',
    color: '#b45309',
    padding: '2px 6px',
    borderRadius: '4px',
    fontWeight: '600',
  },
  badgeAdmin: {
    fontSize: '10px',
    backgroundColor: '#e0e7ff',
    color: '#4338ca',
    padding: '2px 6px',
    borderRadius: '4px',
    fontWeight: '600',
  },
  badgeMember: {
    fontSize: '10px',
    backgroundColor: '#f1f5f9',
    color: '#64748b',
    padding: '2px 6px',
    borderRadius: '4px',
  },
  memberActions: {
    display: 'flex',
    gap: '6px',
    alignItems: 'center',
  },
  transferBtn: {
    backgroundColor: '#fef3c7',
    color: '#b45309',
    border: '1px solid #fde68a',
    borderRadius: '4px',
    padding: '4px 8px',
    fontSize: '11px',
    cursor: 'pointer',
    fontWeight: '500',
  },
  kickBtn: {
    backgroundColor: '#fee2e2',
    color: '#dc2626',
    border: '1px solid #fecaca',
    borderRadius: '4px',
    padding: '4px 8px',
    fontSize: '11px',
    cursor: 'pointer',
  },
  footerActions: {
    display: 'flex',
    justifyContent: 'space-between',
    marginTop: '20px',
    paddingTop: '16px',
    borderTop: '1px solid #e2e8f0',
  },
};
