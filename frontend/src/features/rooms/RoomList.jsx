import { useEffect } from 'react'
import { useDispatch, useSelector } from 'react-redux'
import { useNavigate, useParams } from 'react-router-dom'
import { fetchRooms } from './roomsSlice'
import { selectRooms, selectRoomsLoading } from './roomsSelectors'

export default function RoomList() {
  const dispatch = useDispatch()
  const navigate = useNavigate()
  const { roomId } = useParams()
  const rooms = useSelector(selectRooms)
  const loading = useSelector(selectRoomsLoading)

  useEffect(() => { dispatch(fetchRooms()) }, [dispatch])

  if (loading) return <div style={styles.loading}>Đang tải...</div>

  return (
    <div style={styles.list}>
      {rooms.map((room) => (
        <div
          key={room.id}
          onClick={() => navigate(`/chat/${room.id}`)}
          style={{
            ...styles.item,
            background: roomId === String(room.id) ? '#e8f0fe' : 'transparent',
          }}
        >
          <div style={styles.avatar}>{room.name?.[0]?.toUpperCase() || '#'}</div>
          <div>
            <div style={styles.name}>{room.name}</div>
            <div style={styles.last}>{room.lastMessage || 'Chưa có tin nhắn'}</div>
          </div>
        </div>
      ))}
    </div>
  )
}

const styles = {
  list: { overflowY: 'auto', flex: 1 },
  loading: { padding: '1rem', color: '#888' },
  item: { display: 'flex', alignItems: 'center', gap: '0.75rem', padding: '0.75rem 1rem', cursor: 'pointer', borderRadius: '8px' },
  avatar: { width: 40, height: 40, borderRadius: '50%', background: '#6366f1', color: '#fff', display: 'flex', alignItems: 'center', justifyContent: 'center', fontWeight: 600, flexShrink: 0 },
  name: { fontWeight: 500, fontSize: 14 },
  last: { fontSize: 12, color: '#888', marginTop: 2 },
}
