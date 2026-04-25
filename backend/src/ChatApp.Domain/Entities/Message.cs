namespace ChatApp.Domain.Enums;

//Text: message của người dùng bình thường
//System: System's messages, ví dụ: “user A joined the room”
public enum MessageType
{
    Text = 1,
    System = 2
}

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
        ReplyToMessageId = replyToMessageId;
        CreatedAtUtc = DateTime.UtcNow;
        EditedAtUtc = null;
        DeletedAtUtc = null;
    }

    public void Edit(string newContent)
    {
        if (DeletedAtUtc.HasValue)
            throw new InvalidOperationException("Deleted messages cannot be edited.");

        if (MessageType != MessageType.Text)
            throw new InvalidOperationException("Only text messages can be edited.");

        if (string.IsNullOrWhiteSpace(newContent))
            throw new ArgumentException("New content cannot be empty.", nameof(newContent));

        Content = newContent.Trim();
        EditedAtUtc = DateTime.UtcNow;
    }

    public void Delete()
    {
        if (DeletedAtUtc.HasValue)
            return;

        DeletedAtUtc = DateTime.UtcNow;
    }

    public bool IsSentBy(Guid userId)
    {
        if (userId == Guid.Empty)
            return false;

        return SenderUserId == userId;
    }

    public bool IsReplyMessage()
    {
        return ReplyToMessageId.HasValue;
    }
}

