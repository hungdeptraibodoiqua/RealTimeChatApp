import { useState } from 'react'
import { useDispatch } from 'react-redux'
import { createRoom } from './roomsSlice'
import { ROOM_TYPE } from '../../utils/constants'
import Modal from '../../components/common/Modal'
import Input from '../../components/common/Input'
import Button from '../../components/common/Button'

export default function CreateRoomModal({ onClose }) {
  const dispatch = useDispatch()
  const [name, setName] = useState('')
  const [type, setType] = useState(ROOM_TYPE.GROUP)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')

  const handleSubmit = async () => {
    if (!name.trim()) return setError('Vui lòng nhập tên phòng')
    setLoading(true)
    try {
      await dispatch(createRoom({ name: name.trim(), type })).unwrap()
      onClose()
    } catch (err) {
      setError(err)
    } finally {
      setLoading(false)
    }
  }

  return (
    <Modal title="Tạo phòng chat mới" onClose={onClose}>
      <Input label="Tên phòng" value={name} onChange={e => setName(e.target.value)} placeholder="Nhập tên phòng..." />
      <div style={{ margin: '0.75rem 0' }}>
        <label style={{ fontSize: 13, color: '#555' }}>Loại phòng</label>
        <select value={type} onChange={e => setType(e.target.value)} style={styles.select}>
          <option value={ROOM_TYPE.GROUP}>Nhóm</option>
          <option value={ROOM_TYPE.DIRECT}>Chat 1-1</option>
        </select>
      </div>
      {error && <div style={styles.error}>{error}</div>}
      <div style={{ display: 'flex', gap: '0.5rem', justifyContent: 'flex-end', marginTop: '1rem' }}>
        <Button variant="secondary" onClick={onClose}>Hủy</Button>
        <Button onClick={handleSubmit} disabled={loading}>{loading ? 'Đang tạo...' : 'Tạo phòng'}</Button>
      </div>
    </Modal>
  )
}

const styles = {
  select: { display: 'block', width: '100%', padding: '0.5rem', marginTop: 4, borderRadius: 6, border: '1px solid #ddd', fontSize: 14 },
  error: { color: '#dc2626', fontSize: 13, marginTop: 4 },
}
