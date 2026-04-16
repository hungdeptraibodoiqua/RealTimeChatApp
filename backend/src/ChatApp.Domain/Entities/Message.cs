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
    public Guid UserId { get; private set; }
    public string Content { get; private set; }
    public MessageType MessageType { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public bool IsEdited { get; private set; }
    public bool IsDeleted { get; private set; }
    public Guid? ReplyToMessageId { get; private set; }

    public Message(
        Guid id,
        Guid roomId,
        Guid userId,
        string content,
        MessageType messageType,
        Guid? replyToMessageId = null)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Id cannot be empty.", nameof(id));

        if (roomId == Guid.Empty)
            throw new ArgumentException("RoomId cannot be empty.", nameof(roomId));

        if (userId == Guid.Empty)
            throw new ArgumentException("UserId cannot be empty.", nameof(userId));

        if (!Enum.IsDefined(typeof(MessageType), messageType))
            throw new ArgumentException("Invalid MessageType.", nameof(messageType));

        if (messageType == MessageType.Text && string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content cannot be empty for text messages.", nameof(content));

        Id = id;
        RoomId = roomId;
        UserId = userId;
        Content = content.Trim();
        MessageType = messageType;
        ReplyToMessageId = replyToMessageId;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = null;
        IsEdited = false;
        IsDeleted = false;
    }

    public void Edit(string newContent)
    {
        if (IsDeleted)
            throw new InvalidOperationException("Deleted messages cannot be edited.");

        if (MessageType != MessageType.Text)
            throw new InvalidOperationException("Only text messages can be edited.");

        if (string.IsNullOrWhiteSpace(newContent))
            throw new ArgumentException("New content cannot be empty.", nameof(newContent));

        Content = newContent.Trim();
        UpdatedAt = DateTime.UtcNow;
        IsEdited = true;
    }

    public void Delete()
    {
        if (IsDeleted)
            return;

        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsSentBy(Guid userId)
    {
        if (userId == Guid.Empty)
            return false;

        return UserId == userId;
    }

    public bool IsReplyMessage()
    {
        return ReplyToMessageId.HasValue;
    }
}

