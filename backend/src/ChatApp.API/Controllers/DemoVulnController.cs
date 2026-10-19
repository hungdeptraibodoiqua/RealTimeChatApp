// ================================================================
//  FILE CHỦ Ý CHÈN LỖI AN TOÀN CHO BÀI LAB DEVOPS (SonarQube)
//  Đặt file này tại: backend/src/ChatApp.API/Controllers/DemoVulnController.cs
//  8 lỗi bên dưới + 2 lỗi trong Program.cs = 10 lỗi security.
//  Tất cả đều là mẫu tĩnh (AST) nên SonarQube Community phát hiện được,
//  không cần Developer Edition. Đây là code demo để quét, KHÔNG dùng thật.
// ================================================================
using System;
using System.Net.Http;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.API.Controllers;

[ApiController]
[Route("api/demo-vuln")]
public class DemoVulnController : ControllerBase
{
    // LỖI 1 — S2068 (Security Hotspot): mật khẩu hard-code trong source.
    private const string AdminPassword = "Admin@123456";

    // LỖI 2 — S4790 (Security Hotspot): dùng thuật toán băm yếu MD5.
    [HttpGet("hash")]
    public string WeakHash(string input)
    {
        byte[] data = Encoding.UTF8.GetBytes(input ?? AdminPassword);
        byte[] digest = MD5.HashData(data);          // MD5 đã bị phá, không dùng để băm mật khẩu
        return Convert.ToHexString(digest);
    }

    // LỖI 3 — S5547 (Security Hotspot): dùng thuật toán mã hóa yếu DES.
    [HttpGet("encrypt")]
    public string WeakCipher()
    {
        using DES des = DES.Create();                // DES khóa 56-bit, lỗi thời
        des.GenerateKey();
        return Convert.ToBase64String(des.Key);
    }

    // LỖI 4 — S4830 (Vulnerability): tắt kiểm tra chứng chỉ TLS của server.
    [HttpGet("call-insecure")]
    public string DisableCertCheck()
    {
        var handler = new HttpClientHandler
        {
            // Chấp nhận mọi chứng chỉ -> mở đường cho tấn công man-in-the-middle
            ServerCertificateCustomValidationCallback = (msg, cert, chain, errors) => true
        };
        using var client = new HttpClient(handler);
        return client.GetStringAsync("https://self-signed.example.com").Result;
    }

    // LỖI 5 — S4423 (Vulnerability): ép dùng phiên bản TLS yếu.
    [HttpGet("weak-tls")]
    public string WeakTls()
    {
        var handler = new HttpClientHandler
        {
            SslProtocols = SslProtocols.Tls11          // TLS 1.1 đã bị khai tử
        };
        using var client = new HttpClient(handler);
        return client.GetStringAsync("https://example.com").Result;
    }

    // LỖI 6 — S2245 (Security Hotspot): dùng PRNG không an toàn để sinh token.
    [HttpGet("token")]
    public string InsecureToken()
    {
        var rng = new Random();                        // System.Random không dùng cho mục đích bảo mật
        return rng.Next(100000, 999999).ToString();    // "token" đoán được
    }

    // LỖI 7 — S2092 (Security Hotspot): tạo cookie thiếu cờ Secure (và HttpOnly).
    [HttpGet("set-cookie")]
    public IActionResult InsecureCookie()
    {
        // Không đặt Secure = true nên cookie đi qua cả kết nối HTTP
        Response.Cookies.Append("session_id", Guid.NewGuid().ToString());
        return Ok();
    }

    // LỖI 8 — S5332 (Security Hotspot): truyền dữ liệu qua giao thức rõ (http://).
    [HttpGet("fetch")]
    public string ClearTextProtocol()
    {
        using var client = new HttpClient();
        return client.GetStringAsync("http://api.example.com/data").Result;  // http:// không mã hóa
    }
}
