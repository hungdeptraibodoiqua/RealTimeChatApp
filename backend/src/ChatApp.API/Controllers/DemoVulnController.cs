// ================================================================
//  FILE CHỦ Ý CHÈN LỖI AN TOÀN CHO BÀI LAB DEVOPS (SonarQube)
//  Đặt file này tại: backend/src/ChatApp.API/Controllers/DemoVulnController.cs
//  15 lỗi security, tất cả nằm trong file này.
//  Tất cả đều là mẫu tĩnh (AST) nên SonarQube Community phát hiện được,
//  không cần Developer Edition. Đây là code demo để quét, KHÔNG dùng thật.
// ================================================================
using System;
using System.Net.Http;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.API.Controllers;

[ApiController]
[Route("api/demo-vuln")]
public class DemoVulnController : ControllerBase
{
    // LỖI 1 — S2068: mật khẩu hard-code trong source.
    private const string AdminPassword = "Admin@123456";

    // LỖI 2 — S4790: dùng thuật toán băm yếu MD5.
    [HttpGet("hash")]
    public string WeakHash(string input)
    {
        byte[] data = Encoding.UTF8.GetBytes(input ?? AdminPassword);
        byte[] digest = MD5.HashData(data);          // MD5 đã bị phá, không dùng để băm mật khẩu
        return Convert.ToHexString(digest);
    }

    // LỖI 3 — S5547: dùng thuật toán mã hóa yếu DES.
    [HttpGet("encrypt")]
    public string WeakCipher()
    {
        using DES des = DES.Create();                // DES khóa 56-bit, lỗi thời
        des.GenerateKey();
        return Convert.ToBase64String(des.Key);
    }

    // LỖI 4 — S4830: tắt kiểm tra chứng chỉ TLS của server.
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

    // LỖI 5 — S4423: ép dùng phiên bản TLS yếu.
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

    // LỖI 6 — S2245: dùng PRNG không an toàn để sinh token.
    [HttpGet("token")]
    public string InsecureToken()
    {
        var rng = new Random();                        // System.Random không dùng cho mục đích bảo mật
        return rng.Next(100000, 999999).ToString();    // "token" đoán được
    }

    // LỖI 7 — S2092: tạo cookie thiếu cờ Secure (và HttpOnly).
    [HttpGet("set-cookie")]
    public IActionResult InsecureCookie()
    {
        // Không đặt Secure = true nên cookie đi qua cả kết nối HTTP
        Response.Cookies.Append("session_id", Guid.NewGuid().ToString());
        return Ok();
    }

    // LỖI 8 — S5332: truyền dữ liệu qua giao thức rõ (http://).
    [HttpGet("fetch")]
    public string ClearTextProtocol()
    {
        using var client = new HttpClient();
        return client.GetStringAsync("http://api.example.com/data").Result;  // http:// không mã hóa
    }

    // LỖI 9 — S5542: mã hóa AES ở chế độ ECB (lộ cấu trúc dữ liệu).
    [HttpGet("aes-ecb")]
    public string WeakCipherMode(string input)
    {
        using Aes aes = Aes.Create();
        aes.Mode = CipherMode.ECB;                    // ECB: khối giống nhau -> bản mã giống nhau
        aes.Padding = PaddingMode.PKCS7;
        aes.GenerateKey();
        using ICryptoTransform encryptor = aes.CreateEncryptor();
        byte[] plain = Encoding.UTF8.GetBytes(input ?? "secret");
        return Convert.ToBase64String(encryptor.TransformFinalBlock(plain, 0, plain.Length));
    }

    // LỖI 10 — S4426: khóa RSA quá ngắn (1024 bit).
    [HttpGet("rsa-key")]
    public string WeakRsaKey()
    {
        using RSA rsa = RSA.Create(1024);             // tối thiểu phải 2048 bit
        return Convert.ToBase64String(rsa.ExportRSAPublicKey());
    }

    // LỖI 11 — S2053: băm mật khẩu với salt cố định.
    [HttpGet("derive-key")]
    public string ConstantSalt(string password)
    {
        byte[] salt = Encoding.UTF8.GetBytes("fixed-salt-123"); // salt hard-code, mọi user dùng chung
        using var kdf = new Rfc2898DeriveBytes(password ?? AdminPassword, salt, 1000, HashAlgorithmName.SHA256);
        return Convert.ToBase64String(kdf.GetBytes(32));
    }

    // LỖI 12 — S2755: bộ đọc XML cho phép XXE (tải entity bên ngoài).
    [HttpPost("parse-xml")]
    public string XxeParser([FromBody] string xml)
    {
        var doc = new XmlDocument
        {
            XmlResolver = new XmlUrlResolver()        // cho phép đọc file/URL qua external entity
        };
        doc.LoadXml(xml);
        return doc.InnerText;
    }

    // LỖI 13 — S5445: tạo file tạm không an toàn.
    [HttpGet("temp-file")]
    public string InsecureTempFile()
    {
        string path = Path.GetTempFileName();         // tên đoán được, dễ bị chiếm trước (race condition)
        System.IO.File.WriteAllText(path, AdminPassword);
        return path;
    }

    // LỖI 14 — S4790: băm bằng SHA-1 (đã bị phá va chạm).
    [HttpGet("sha1")]
    public string WeakHashSha1(string input)
    {
        byte[] digest = SHA1.HashData(Encoding.UTF8.GetBytes(input ?? string.Empty));
        return Convert.ToHexString(digest);
    }

    // LỖI 15 — S2068: mật khẩu database hard-code trong connection string.
    [HttpGet("db-info")]
    public string HardcodedConnectionString()
    {
        string connectionString = "Host=10.0.0.5;Database=chatapp;Username=sa;Password=P@ssw0rd2026";
        return connectionString.Split(';')[0];
    }
}
