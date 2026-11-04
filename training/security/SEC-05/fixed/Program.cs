// CHỈ LÀ MINH HỌA NGỪNG PHỤC VỤ TĨNH, KHÔNG PHẢI BẢN SỬA ĐẦY ĐỦ
// Không sử dụng UseStaticFiles cho dữ liệu upload riêng tư.
// Cần triển khai GET /api/messages/media/{id} kiểm tra JWT và RoomMember,
// stream file từ thư mục ngoài wwwroot; frontend gửi Authorization header.
