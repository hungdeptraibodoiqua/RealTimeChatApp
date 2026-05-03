using ChatApp.Domain.Enums;

namespace ChatApp.Domain.Entities;

/// <summary>
/// Đại diện một tin nhắn trong room, gồm nội dung, người gửi và trạng thái edit/delete.
/// </summary>
public class Message
{
    public Guid Id { get; private set; }
    public Guid RoomId { get; private set; }
    public Guid SenderUserId { get; private set; }
    public string Content { get; private set; }
    public MessageType MessageType { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? EditedAtUtc { get; private set; }
    public DateTime? DeletedAtUtc { get; private set; }
    public Guid? ReplyToMessageId { get; private set; }

    public Message(
        Guid id,
        Guid roomId,
        Guid senderUserId,
        string content,
        MessageType messageType,
        Guid? replyToMessageId = null)
    {
        // Guid.Empty bị chặn để message có thể được tham chiếu bởi reply/read tracking.
        if (id == Guid.Empty)
            throw new ArgumentException("Id cannot be empty.", nameof(id));

        if (roomId == Guid.Empty)
            throw new ArgumentException("RoomId cannot be empty.", nameof(roomId));

        if (senderUserId == Guid.Empty)
            throw new ArgumentException("SenderUserId cannot be empty.", nameof(senderUserId));

        if (!Enum.IsDefined(typeof(MessageType), messageType))
            throw new ArgumentException("Invalid MessageType.", nameof(messageType));

        if (messageType == MessageType.Text && string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content cannot be empty for text messages.", nameof(content));

        Id = id;
        RoomId = roomId;
        SenderUserId = senderUserId;
        Content = content.Trim();
        MessageType = messageType;
        // ReplyToMessageId là tùy chọn vì không phải message nào cũng là reply.
        ReplyToMessageId = replyToMessageId;
        CreatedAtUtc = DateTime.UtcNow;
        EditedAtUtc = null;
        DeletedAtUtc = null;
    }

    public void Edit(string newContent)
    {
        // Message đã delete không được edit để giữ lịch sử thao tác nhất quán.
        if (DeletedAtUtc.HasValue)
            throw new InvalidOperationException("Deleted messages cannot be edited.");

        // Hiện tại chỉ text message có nội dung editable.
        if (MessageType != MessageType.Text)
            throw new InvalidOperationException("Only text messages can be edited.");

        if (string.IsNullOrWhiteSpace(newContent))
            throw new ArgumentException("New content cannot be empty.", nameof(newContent));

        Content = newContent.Trim();
        EditedAtUtc = DateTime.UtcNow;
    }

    public void Delete()
    {
        // Soft delete giữ lại record để bảo toàn lịch sử room và reply reference.
        if (DeletedAtUtc.HasValue)
            return;

        DeletedAtUtc = DateTime.UtcNow;
    }

    public bool IsSentBy(Guid userId)
    {
        // Guid.Empty không đại diện user thật nên không thể là sender.
        if (userId == Guid.Empty)
            return false;

        return SenderUserId == userId;
    }

    public bool IsReplyMessage()
    {
        return ReplyToMessageId.HasValue;
    }
}
