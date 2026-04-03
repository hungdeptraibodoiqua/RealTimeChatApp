import { createSlice, createAsyncThunk } from '@reduxjs/toolkit'
import { roomService } from '../../services/roomService'

export const fetchRooms = createAsyncThunk('rooms/fetchAll', async (_, { rejectWithValue }) => {
  try {
    return await roomService.getMyRooms()
  } catch (err) {
    return rejectWithValue(err.response?.data?.detail || 'Không tải được danh sách phòng')
  }
})

export const createRoom = createAsyncThunk('rooms/create', async (data, { rejectWithValue }) => {
  try {
    return await roomService.createRoom(data)
  } catch (err) {
    return rejectWithValue(err.response?.data?.detail || 'Tạo phòng thất bại')
  }
})

export const deleteRoom = createAsyncThunk('rooms/delete', async (roomId, { rejectWithValue }) => {
  try {
    await roomService.deleteRoom(roomId)
    return roomId
  } catch (err) {
    return rejectWithValue(err.response?.data?.detail || 'Xóa phòng thất bại')
  }
})

const roomsSlice = createSlice({
  name: 'rooms',
  initialState: { list: [], loading: false, error: null, onlineUsers: {} },
  reducers: {
    userOnlineStatusChanged: (state, action) => {
      const { userId, isOnline } = action.payload
      state.onlineUsers[userId] = isOnline
    },
    roomDeleted: (state, action) => {
      state.list = state.list.filter(r => r.id !== action.payload)
    },
    memberRemoved: (state, action) => {
      const { roomId, userId } = action.payload
      const room = state.list.find(r => r.id === roomId)
      if (room) room.members = room.members?.filter(m => m.userId !== userId)
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(fetchRooms.pending, (state) => { state.loading = true })
      .addCase(fetchRooms.fulfilled, (state, action) => { state.loading = false; state.list = action.payload })
      .addCase(fetchRooms.rejected, (state, action) => { state.loading = false; state.error = action.payload })
      .addCase(createRoom.fulfilled, (state, action) => { state.list.unshift(action.payload) })
      .addCase(deleteRoom.fulfilled, (state, action) => {
        state.list = state.list.filter(r => r.id !== action.payload)
      })
  },
})

export const { userOnlineStatusChanged, roomDeleted, memberRemoved } = roomsSlice.actions
export default roomsSlice.reducer
