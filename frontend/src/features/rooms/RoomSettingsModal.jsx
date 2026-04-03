import { useState, useEffect } from 'react'
import { useDispatch, useSelector } from 'react-redux'
import { deleteRoom } from './roomsSlice'
import { selectCurrentUser } from '../auth/authSelectors'
import { roomService } from '../../services/roomService'
import { MEMBER_ROLE } from '../../utils/constants'
import Modal from '../../components/common/Modal'
import Button from '../../components/common/Button'

export default function RoomSettingsModal({ room, onClose }) {
  const dispatch = useDispatch()
  const currentUser = useSelector(selectCurrentUser)
  const [members, setMembers] = useState([])
  const myRole = members.find(m => m.userId === currentUser?.id)?.role

  useEffect(() => {
    roomService.getMembers(room.id).then(setMembers)
  }, [room.id])

  const handleRemoveMember = async (userId) => {
    await roomService.removeMember(room.id, userId)
    setMembers(prev => prev.filter(m => m.userId !== userId))
  }

  const handleDeleteRoom = async () => {
    if (!window.confirm('Xóa phòng này? Không thể hoàn tác.')) return
    await dispatch(deleteRoom(room.id)).unwrap()
    onClose()
  }

  const canManage = myRole === MEMBER_ROLE.OWNER || myRole === MEMBER_ROLE.ADMIN

  return (
    <Modal title={`Cài đặt: ${room.name}`} onClose={onClose}>
      <div style={styles.section}>Thành viên ({members.length})</div>
      {members.map(member => (
        <div key={member.userId} style={styles.memberRow}>
          <div style={styles.avatar}>{member.displayName?.[0]?.toUpperCase()}</div>
          <div style={{ flex: 1 }}>
            <div style={styles.name}>{member.displayName}</div>
            <div style={styles.role}>{member.role}</div>
          </div>
          {canManage && member.role !== MEMBER_ROLE.OWNER && member.userId !== currentUser?.id && (
            <Button variant="danger" onClick={() => handleRemoveMember(member.userId)}>Xóa</Button>
          )}
        </div>
      ))}
      {myRole === MEMBER_ROLE.OWNER && (
        <div style={{ marginTop: '1rem', borderTop: '1px solid #fee2e2', paddingTop: '1rem' }}>
          <Button variant="danger" onClick={handleDeleteRoom}>Xóa phòng</Button>
        </div>
      )}
    </Modal>
  )
}

const styles = {
  section: { fontWeight: 600, fontSize: 13, color: '#555', marginBottom: '0.5rem', textTransform: 'uppercase', letterSpacing: 0.5 },
  memberRow: { display: 'flex', alignItems: 'center', gap: '0.75rem', padding: '0.5rem 0' },
  avatar: { width: 36, height: 36, borderRadius: '50%', background: '#818cf8', color: '#fff', display: 'flex', alignItems: 'center', justifyContent: 'center', fontWeight: 600, flexShrink: 0 },
  name: { fontWeight: 500, fontSize: 14 },
  role: { fontSize: 12, color: '#888' },
}
