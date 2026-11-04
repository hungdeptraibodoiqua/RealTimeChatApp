namespace ChatApp.Domain.Enums;

// Định nghĩa các loại tin nhắn trong hệ thống chat:
// Text: Tin nhắn văn bản người dùng
// System: Tin nhắn thông báo hệ thống (ví dụ: User gia nhập, rời phòng, đổi Owner)
// Image: Tin nhắn hình ảnh (đường dẫn URL tới file ảnh)
// Video: Tin nhắn video (đường dẫn URL tới file video)
public enum MessageType
{
    Text = 1,
    System = 2,
    Image = 3,
    Video = 4
}