using ChatApp.Domain.Enums;

namespace ChatApp.API.Services;

/// <summary>
/// SEC-11: xác định loại file upload bằng magic bytes do SERVER đọc, không tin Content-Type hay đuôi file client gửi.
/// Chỉ chấp nhận JPEG, PNG, GIF, WEBP, MP4, WEBM. SVG/HTML/mọi loại khác bị từ chối.
/// Đuôi file lưu trên đĩa do server tự đặt theo kết quả nhận diện.
/// </summary>
public static class MediaFileInspector
{
    public sealed record MediaKind(MessageType MessageType, string Extension, string ContentType);

    private static readonly Dictionary<string, MediaKind> ByExtension = new(StringComparer.OrdinalIgnoreCase)
    {
        [".jpg"] = new MediaKind(MessageType.Image, ".jpg", "image/jpeg"),
        [".png"] = new MediaKind(MessageType.Image, ".png", "image/png"),
        [".gif"] = new MediaKind(MessageType.Image, ".gif", "image/gif"),
        [".webp"] = new MediaKind(MessageType.Image, ".webp", "image/webp"),
        [".mp4"] = new MediaKind(MessageType.Video, ".mp4", "video/mp4"),
        [".webm"] = new MediaKind(MessageType.Video, ".webm", "video/webm"),
    };

    /// <summary>Đọc tối đa 16 byte đầu của stream và trả về loại media, hoặc null nếu không hợp lệ.</summary>
    public static async Task<MediaKind?> DetectAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        var header = new byte[16];
        var read = 0;
        while (read < header.Length)
        {
            var n = await stream.ReadAsync(header.AsMemory(read, header.Length - read), cancellationToken);
            if (n == 0) break;
            read += n;
        }
        return Detect(header.AsSpan(0, read));
    }

    public static MediaKind? Detect(ReadOnlySpan<byte> h)
    {
        if (h.Length >= 3 && h[0] == 0xFF && h[1] == 0xD8 && h[2] == 0xFF)
            return ByExtension[".jpg"];
        if (h.Length >= 8 && h[0] == 0x89 && h[1] == 0x50 && h[2] == 0x4E && h[3] == 0x47 &&
            h[4] == 0x0D && h[5] == 0x0A && h[6] == 0x1A && h[7] == 0x0A)
            return ByExtension[".png"];
        if (h.Length >= 6 && h[0] == (byte)'G' && h[1] == (byte)'I' && h[2] == (byte)'F' && h[3] == (byte)'8' &&
            (h[4] == (byte)'7' || h[4] == (byte)'9') && h[5] == (byte)'a')
            return ByExtension[".gif"];
        if (h.Length >= 12 && h[0] == (byte)'R' && h[1] == (byte)'I' && h[2] == (byte)'F' && h[3] == (byte)'F' &&
            h[8] == (byte)'W' && h[9] == (byte)'E' && h[10] == (byte)'B' && h[11] == (byte)'P')
            return ByExtension[".webp"];
        if (h.Length >= 8 && h[4] == (byte)'f' && h[5] == (byte)'t' && h[6] == (byte)'y' && h[7] == (byte)'p')
            return ByExtension[".mp4"];
        if (h.Length >= 4 && h[0] == 0x1A && h[1] == 0x45 && h[2] == 0xDF && h[3] == 0xA3)
            return ByExtension[".webm"];
        return null;
    }

    /// <summary>Tra loại media theo đuôi của file ĐÃ LƯU (do server đặt), dùng khi trả file về client.</summary>
    public static MediaKind? FromStoredFileName(string fileName)
        => ByExtension.TryGetValue(Path.GetExtension(fileName), out var kind) ? kind : null;
}
