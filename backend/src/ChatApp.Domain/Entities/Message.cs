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
        public string MessageId { get; private set; }
        public string RoomId { get; private set; }
        public string SenderId { get; private set; }
        public string Content { get; private set; }
        public MessageType MessageType { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }
        public bool IsEdited { get; private set; }
        public bool IsDeleted { get; private set; }
        public string? ReplyToMessageId { get; private set; }

        public Message(
            string messageId,
            string roomId,
            string senderId,
            string content,
            MessageType messageType,
            string? replyToMessageId = null)
        {
            if (string.IsNullOrWhiteSpace(messageId))
                throw new ArgumentException("MessageId cannot be empty.", nameof(messageId));

            if (string.IsNullOrWhiteSpace(roomId))
                throw new ArgumentException("RoomId cannot be empty.", nameof(roomId));

            if (string.IsNullOrWhiteSpace(senderId))
                throw new ArgumentException("SenderId cannot be empty.", nameof(senderId));

            if (!Enum.IsDefined(typeof(MessageType), messageType))
                throw new ArgumentException("Invalid MessageType.", nameof(messageType));

            if (messageType == MessageType.Text && string.IsNullOrWhiteSpace(content))
                throw new ArgumentException("Content cannot be empty for text messages.", nameof(content));

            MessageId = messageId.Trim();
            RoomId = roomId.Trim();
            SenderId = senderId.Trim();
            Content = content.Trim();
            MessageType = messageType;
            ReplyToMessageId = string.IsNullOrWhiteSpace(replyToMessageId) ? null : replyToMessageId.Trim();
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

        public bool IsSentBy(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return false;

            return SenderId == userId.Trim();
        }

        public bool IsReplyMessage()
        {
            return !string.IsNullOrWhiteSpace(ReplyToMessageId);
        }
    }

