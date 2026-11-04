using System.Security.Claims;
using ChatApp.Application.Abstractions.Persistence;
using ChatApp.Application.Abstractions.Realtime;
using ChatApp.API.Services;
using ChatApp.Domain.Entities;
using ChatApp.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.API.Controllers;

/// <summary>
/// Controller quản lý gửi tin nhắn, tải lịch sử, upload ảnh/video, thu hồi và sửa tin nhắn trong 1 giờ.
/// Kết nối với: MessageRepository, RoomRepository, IChatNotifier, AppDbContext/UnitOfWork.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MessagesController : ControllerBase
{
    private readonly IMessageRepository _messageRepository;
    private readonly IRoomRepository _roomRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IChatNotifier _chatNotifier;
    private readonly IWebHostEnvironment _env;
    private readonly IConfiguration _configuration;

    public MessagesController(
        IMessageRepository messageRepository,
        IRoomRepository roomRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IChatNotifier chatNotifier,
        IWebHostEnvironment env,
        IConfiguration configuration)
    {
        _messageRepository = messageRepository;
        _roomRepository = roomRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _chatNotifier = chatNotifier;
        _env = env;
        _configuration = configuration;
    }

    /// <summary>
    /// Lấy danh sách tin nhắn của một phòng chat theo phân trang.
    /// </summary>
    [HttpGet("{roomId:guid}")]
    public async Task<IActionResult> GetRoomMessages(Guid roomId, [FromQuery] int skip = 0, [FromQuery] int take = 50)
    {
        var currentUserId = GetCurrentUserId();
        var member = await _roomRepository.GetMemberAsync(roomId, currentUserId);
        if (member == null || member.LeftAtUtc != null)
            return StatusCode(StatusCodes.Status403Forbidden, new { detail = "Bạn không phải thành viên của phòng này." });

        var messages = await _messageRepository.GetRoomMessagesAsync(roomId, skip, take);
        
        // Map DTO để client hiển thị
        var dtos = messages.Select(m => new
        {
            m.Id,
            m.RoomId,
            m.SenderUserId,
            m.Content,
            MessageType = (int)m.MessageType,
            m.CreatedAtUtc,
            m.EditedAtUtc,
            m.RecalledAtUtc,
            m.ReplyToMessageId
        });

        return Ok(dtos);
    }

    /// <summary>
    /// Gửi tin nhắn văn bản vào phòng chat và phát realtime qua SignalR tới các client.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageDto request)
    {
        var currentUserId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(request.Content))
            return BadRequest(new { detail = "Nội dung tin nhắn không được để trống." });

        var member = await _roomRepository.GetMemberAsync(request.RoomId, currentUserId);
        if (member == null || member.LeftAtUtc != null)
            return StatusCode(StatusCodes.Status403Forbidden, new { detail = "Bạn không phải thành viên hoạt động của phòng này." });

        var message = new Message(
            Guid.NewGuid(),
            request.RoomId,
            currentUserId,
            request.Content,
            MessageType.Text,
            request.ReplyToMessageId);

        await _messageRepository.AddAsync(message);
        await _unitOfWork.SaveChangesAsync();

        var messageDto = new
        {
            message.Id,
            message.RoomId,
            message.SenderUserId,
            message.Content,
            MessageType = (int)message.MessageType,
            message.CreatedAtUtc,
            message.EditedAtUtc,
            message.RecalledAtUtc,
            message.ReplyToMessageId
        };

        // Phát realtime tới toàn bộ thành viên trong phòng đang lắng nghe SignalR
        await _chatNotifier.MessageCreatedAsync(message.RoomId, messageDto);

        return Ok(messageDto);
    }

    /// <summary>
    /// Upload file ảnh hoặc video từ client, lưu trên server và tạo tin nhắn media realtime.
    /// </summary>
    [HttpPost("upload")]
    [RequestSizeLimit(30 * 1024 * 1024)] // Giới hạn kích thước request tối đa 30MB
    public async Task<IActionResult> UploadMedia([FromForm] IFormFile file, [FromForm] Guid roomId)
    {
        var currentUserId = GetCurrentUserId();
        if (file == null || file.Length == 0)
            return BadRequest(new { detail = "Vui lòng chọn file cần gửi." });

        // Giới hạn 25MB cho file media
        if (file.Length > 25 * 1024 * 1024)
            return BadRequest(new { detail = "Kích thước file vượt quá giới hạn 25MB." });

        var member = await _roomRepository.GetMemberAsync(roomId, currentUserId);
        if (member == null || member.LeftAtUtc != null)
            return StatusCode(StatusCodes.Status403Forbidden, new { detail = "Bạn không phải thành viên hoạt động của phòng này." });

        // SEC-11: nhận diện loại file bằng magic bytes, KHÔNG tin Content-Type/đuôi file do client gửi.
        MediaFileInspector.MediaKind? kind;
        await using (var probe = file.OpenReadStream())
        {
            kind = await MediaFileInspector.DetectAsync(probe);
        }
        if (kind is null)
            return BadRequest(new { detail = "Định dạng file không được hỗ trợ. Chỉ chấp nhận JPG, PNG, GIF, WEBP, MP4, WEBM." });
        var messageType = kind.MessageType;

        // SEC-05: lưu file NGOÀI wwwroot để không bị phục vụ công khai qua static files.
        var uploadsDir = GetMediaStoragePath();
        Directory.CreateDirectory(uploadsDir);

        // Tên file do server đặt: GUID + đuôi theo loại đã nhận diện.
        var uniqueFileName = $"{Guid.NewGuid()}{kind.Extension}";
        var physicalPath = Path.Combine(uploadsDir, uniqueFileName);

        await using (var stream = new FileStream(physicalPath, FileMode.CreateNew))
        {
            await file.CopyToAsync(stream);
        }

        var fileUrl = $"/uploads/{uniqueFileName}";
        var message = new Message(
            Guid.NewGuid(),
            roomId,
            currentUserId,
            fileUrl,
            messageType);

        await _messageRepository.AddAsync(message);
        await _unitOfWork.SaveChangesAsync();

        var messageDto = new
        {
            message.Id,
            message.RoomId,
            message.SenderUserId,
            message.Content,
            MessageType = (int)message.MessageType,
            message.CreatedAtUtc,
            message.EditedAtUtc,
            message.RecalledAtUtc,
            message.ReplyToMessageId
        };

        // Phát thông báo realtime tới toàn phòng
        await _chatNotifier.MessageCreatedAsync(roomId, messageDto);

        return Ok(messageDto);
    }

    /// <summary>
    /// Thu hồi tin nhắn trong vòng 1 tiếng kể từ lúc gửi.
    /// </summary>
    [HttpPut("{id:guid}/recall")]
    public async Task<IActionResult> RecallMessage(Guid id)
    {
        var currentUserId = GetCurrentUserId();
        var message = await _messageRepository.GetByIdAsync(id);
        if (message == null)
            return NotFound(new { detail = "Tin nhắn không tồn tại." });

        if (message.SenderUserId != currentUserId)
            return StatusCode(StatusCodes.Status403Forbidden, new { detail = "Chỉ người gửi mới có quyền thu hồi tin nhắn này." });

        try
        {
            // Gọi phương thức Recall trong Domain Entity Message (đã chứa kiểm tra 1 giờ)
            message.Recall();
            await _unitOfWork.SaveChangesAsync();

            // Phát thông báo thu hồi realtime tới các thành viên trong phòng
            await _chatNotifier.MessageRecalledAsync(message.RoomId, message.Id);

            return Ok(new { message = "Tin nhắn đã được thu hồi thành công.", messageId = id });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { detail = ex.Message });
        }
    }

    /// <summary>
    /// Chỉnh sửa nội dung tin nhắn văn bản trong vòng 1 tiếng.
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> EditMessage(Guid id, [FromBody] EditMessageDto request)
    {
        var currentUserId = GetCurrentUserId();
        var message = await _messageRepository.GetByIdAsync(id);
        if (message == null)
            return NotFound(new { detail = "Tin nhắn không tồn tại." });

        if (message.SenderUserId != currentUserId)
            return StatusCode(StatusCodes.Status403Forbidden, new { detail = "Chỉ người gửi mới có quyền chỉnh sửa tin nhắn này." });

        try
        {
            // Gọi Edit trong Domain Entity Message (đã chứa kiểm tra 1 giờ)
            message.Edit(request.NewContent);
            await _unitOfWork.SaveChangesAsync();

            // Phát sự kiện chỉnh sửa realtime tới các thành viên trong phòng
            await _chatNotifier.MessageEditedAsync(
                message.RoomId,
                message.Id,
                message.Content,
                message.EditedAtUtc!.Value);

            return Ok(new
            {
                message.Id,
                message.RoomId,
                message.Content,
                message.EditedAtUtc
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { detail = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { detail = ex.Message });
        }
    }

    /// <summary>
    /// Xóa tin nhắn khỏi cuộc trò chuyện (Soft delete).
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteMessage(Guid id)
    {
        var currentUserId = GetCurrentUserId();
        var message = await _messageRepository.GetByIdAsync(id);
        if (message == null)
            return NotFound(new { detail = "Tin nhắn không tồn tại." });

        // Kiểm tra quyền: Người gửi hoặc Owner/Admin của phòng mới được xóa
        var member = await _roomRepository.GetMemberAsync(message.RoomId, currentUserId);
        var canDelete = message.SenderUserId == currentUserId || (member != null && member.IsOwnerOrAdmin());

        if (!canDelete)
            return StatusCode(StatusCodes.Status403Forbidden, new { detail = "Bạn không có quyền xóa tin nhắn này." });

        message.Delete();
        await _unitOfWork.SaveChangesAsync();

        // Báo cho các client xóa tin nhắn khỏi giao diện
        await _chatNotifier.MessageDeletedAsync(message.RoomId, message.Id);

        return Ok(new { message = "Đã xóa tin nhắn.", messageId = id });
    }

    /// <summary>
    /// SEC-05: tải ảnh/video của một tin nhắn. Chỉ thành viên đang hoạt động của phòng chứa tin nhắn mới tải được.
    /// Thay thế cách cũ phục vụ /uploads/* công khai qua UseStaticFiles().
    /// </summary>
    [HttpGet("{id:guid}/media")]
    public async Task<IActionResult> GetMessageMedia(Guid id)
    {
        var currentUserId = GetCurrentUserId();
        var message = await _messageRepository.GetByIdAsync(id);
        if (message == null || message.DeletedAtUtc != null || message.RecalledAtUtc != null ||
            (message.MessageType != MessageType.Image && message.MessageType != MessageType.Video))
            return NotFound();

        var member = await _roomRepository.GetMemberAsync(message.RoomId, currentUserId);
        if (member == null || member.LeftAtUtc != null)
            return StatusCode(StatusCodes.Status403Forbidden, new { detail = "Bạn không phải thành viên của phòng chứa tin nhắn này." });

        // Chỉ lấy phần tên file, chống path traversal từ dữ liệu Content.
        var fileName = Path.GetFileName(message.Content ?? string.Empty);
        var kind = MediaFileInspector.FromStoredFileName(fileName);
        var physicalPath = Path.Combine(GetMediaStoragePath(), fileName);
        if (kind is null || string.IsNullOrEmpty(fileName) || !System.IO.File.Exists(physicalPath))
            return NotFound();

        Response.Headers["X-Content-Type-Options"] = "nosniff";
        Response.Headers["Cache-Control"] = "private, no-store";
        return PhysicalFile(physicalPath, kind.ContentType, enableRangeProcessing: true);
    }

    private string GetMediaStoragePath()
    {
        var configured = _configuration["Media:StoragePath"];
        return string.IsNullOrWhiteSpace(configured)
            ? Path.Combine(_env.ContentRootPath, "data", "uploads")
            : configured;
    }

    private Guid GetCurrentUserId()
    {
        var sub = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(sub, out var id) ? id : Guid.Empty;
    }
}

public record SendMessageDto(Guid RoomId, string Content, Guid? ReplyToMessageId = null);
public record EditMessageDto(string NewContent);
