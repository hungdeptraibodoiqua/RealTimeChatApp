# CHANGELOG – bản v3 "vá hoàn chỉnh" (02/11/2026)

Bản v3 = v2 (xem `CHANGELOG-V2.md`) + vá toàn bộ lỗi thật còn mở. Dùng bản này để **chạy demo**.
Trạng thái "trước khi vá" của SEC-05/11/12/13/14 = bản v2 (tag `hardening-v2` nếu làm repo 3 commit theo hướng dẫn).

| Mã | File | Thay đổi |
|---|---|---|
| SEC-05 | `MessagesController.cs`, `Program.cs`, `compose.yaml`, `backend/Dockerfile`, `nginx.conf`, `MessageItem.jsx` | Media lưu ngoài `wwwroot` (`/app/data/uploads`, cấu hình `Media__StoragePath`); bỏ `UseStaticFiles()` và proxy `/uploads/`; thêm `GET /api/messages/{id}/media` có `[Authorize]` + kiểm tra thành viên đang hoạt động + chặn path traversal + `nosniff`; frontend tải media bằng Bearer token thành blob URL |
| SEC-11 | `Services/MediaFileInspector.cs` (mới), `MessagesController.cs` | Nhận diện file bằng magic bytes (JPG/PNG/GIF/WEBP/MP4/WEBM), từ chối SVG/HTML/khác; server tự đặt đuôi file; `FileMode.CreateNew` |
| SEC-12 | `Program.cs`, `AuthController.cs` | Rate limit `auth`: 10 request/phút/IP (theo `X-Real-IP` do nginx đặt) cho `login` và `register`, trả 429 |
| SEC-13 | `frontend/nginx.conf` | CSP (script chỉ `'self'`), `X-Content-Type-Options`, `X-Frame-Options`, `Referrer-Policy`, `Permissions-Policy` |
| SEC-14 | `backend/Dockerfile` | `HEALTHCHECK` kiểm tra cổng 8080 bằng bash `/dev/tcp` |
| Bug | `Rooms/Messages/FriendsController.cs` | 13 chỗ `Forbid("thông báo")` dùng sai (tham số là *authentication scheme*) khiến request bị từ chối trả lỗi 4xx/5xx sai mã; đổi thành `403` + `{ detail }` |
| Policy | `policies/compose.rego` | Sửa lỗi OPA thật báo `rego_unsafe_var_error: var _ is unsafe` (dòng `not x[_] == ...`): chuyển sang hàm phụ trợ dùng `in`; thêm: cổng publish **không có host_ip** cũng bị deny |
| LAB-04 | `frontend/Dockerfile` | `apk upgrade --no-cache` ở stage nginx (vẫn `USER 101` ở cuối): vá 3 CVE Trivy phát hiện khi nhóm chạy thật (brotli-libs CRITICAL, curl/libcurl HIGH) |
| Demo | `scripts/runtime-checks.ps1` (mới), `run-lab.ps1 -Action runtime` | 1 lệnh kiểm tra app đang chạy: cổng, healthcheck, privileged/cap_drop, header, CORS, media 401, token 401, rate limit 429 |

## Đã kiểm chứng trên Linux sandbox

- Biên dịch Domain + Application + toàn bộ Controllers/Middleware + `MediaFileInspector` + cấu hình rate limiter bằng .NET SDK 8 (offline): **0 lỗi**.
- `MediaFileInspector`: JPG/PNG/GIF/WEBP/MP4 nhận đúng; SVG, HTML, file rỗng bị từ chối; `../../etc/passwd` không ra loại hợp lệ.
- Rate limiter chạy thật: request 1–10 đi qua, 11–13 trả **429**; IP khác không bị ảnh hưởng.
- `nginx -t` OK; header trả đúng; `/uploads/*` không còn proxy tới backend.
- Frontend build OK; trang login render trong Chromium với CSP đang bật: **0 vi phạm CSP**.
- Checkov toàn bộ rule (bỏ `training/`): **118 passed, 0 failed**. OPA trên Compose thật: 0 deny. Fixture: 20/20 và 10/10 PASS.

## CHƯA kiểm chứng (cần chạy trên Windows)

Build image backend bằng .NET 9 + NuGet thật, chạy cả stack, luồng upload/xem ảnh end-to-end, `runtime-checks.ps1` (chưa có PowerShell trong sandbox), Gitleaks, Trivy, CI.


## v3.1 (04/11/2026) – giữ nguyên giá trị của nhóm + bổ sung script

Dựng từ bản lab nhóm gửi (`src_code_lab.rar`), **giữ nguyên**:

- Digest SHA-256 của 5 image công cụ trong `scripts/tool-images.ps1` và `.github/workflows/security-lab.yml` (Gitleaks, Checkov, OPA, Trivy, Python) – bản v3 trước đó để `:latest`.
- `policies/compose.rego` của nhóm (đã sửa lỗi unsafe var, kết quả tương đương bản v3).
- Script SonarQube cá nhân ở thư mục gốc: `sonarqube.ps1`, `save_token.ps1`, `create_envar_token.ps1`, `scan_back-end.ps1`, `scan_front-end.ps1`, `run_gitleak.ps1`, `check_token.ps1`; báo cáo cũ trong `security-reports/` (bằng chứng "trước").

Thay đổi:

| File | Thay đổi |
|---|---|
| `scripts/lab-auth.ps1` (mới) | Hàm dùng chung: đăng nhập bằng mật khẩu nhập ẩn, liệt kê và chọn phòng; không in token |
| `scripts/test-signalr-auth.ps1` (mới) | TV2 – SEC-03/04: tài khoản ngoài phòng gọi JoinRoom/SendTyping qua `frontend/scripts/test-room-auth.mjs` |
| `scripts/test-upload-svg.ps1` (mới) | TV3 – SEC-11/05: upload SVG (2 dạng) phải 400; PNG thật 200; `/uploads/<file>` không trả ảnh; API media 401 không token, 200 có token |
| `scripts/chathub-demo.ps1` (mới) + `training/runtime/SEC-03-04/ChatHub.vulnerable.cs` | TV2 – Hình 5.7: chuyển tạm ChatHub.cs sang bản có lỗi để chụp FAIL, rồi khôi phục bản đã vá (có sao lưu, build lại backend) |
| `scripts/evidence-header.ps1` (mới) | In thời gian, commit SHA, phiên bản mã nguồn trước khi chụp bằng chứng |
| `scripts/run-lab.ps1` | Thêm action `signalr`, `upload`; sửa `build` thiếu `--env-file` (trước đây Compose dừng vì thiếu DB_PASSWORD) |
| `scripts/runtime-checks.ps1` | Bảng kết quả in ra màn hình chắc chắn (`Out-Host`) |
| `check_token.ps1` | Đổi `curl -u "token:"` sang header `Authorization: Bearer` – hết dương tính giả `curl-auth-user` của Gitleaks |
| `materials.txt` | **Không đưa vào gói** – chứa token SonarQube dạng trần (SEC-15). Nếu còn trên máy, xóa và revoke token |
| `.sonarqube/`, `.scannerwork/`, `bin/`, `obj/` | Không đưa vào gói (artifact sinh lại khi build/quét; `.sonarqube` từng chứa token) |

Đã kiểm chứng trên Linux sandbox: 15 script PowerShell parse OK (pwsh 7.4); Gitleaks 8.30.1 với `.gitleaks.toml` trên toàn bộ gói: no leaks, exit 0; guard 20/20 + 10/10 PASS; OPA: Compose thật exit 0, `compose.bad.json` exit 1, `compose.good.json` exit 0; Checkov Dockerfile (bỏ `training/`): 136 passed, 0 failed; `test-upload-svg.ps1` chạy với API giả lập: hành vi v3 → 6/6 PASS, hành vi v2 → 5 FAIL; `test-signalr-auth.ps1` chạy đúng luồng (đăng nhập A, chọn phòng, đăng nhập B, gọi script Node, xóa biến môi trường token).
