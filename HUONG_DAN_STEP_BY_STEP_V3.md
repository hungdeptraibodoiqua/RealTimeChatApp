# Chuẩn hóa lab DevSecOps RealTimeChatApp – Đánh giá & hướng dẫn step by step

Nov 2, 2026 · @Huunt

## 1. Kết luận nhanh

**Chưa nộp được ở trạng thái hiện tại.** Khung bài (kiến trúc, 10 SEC + 5 CQ, phân công, tính trung thực về bằng chứng) đã tốt khoảng 70%. Nhưng còn 4 điểm chặn việc nộp:

1. **Lab chưa chạy được trọn vẹn:** service `db` (PostgreSQL) gần như chắc chắn crash vì `cap_drop: [ALL]` khi container chạy root. Mình đã mô phỏng đúng thao tác của entrypoint Postgres trong Docker: `chmod` và `setuid` đều bị "Operation not permitted". DB không healthy thì backend không khởi động, toàn bộ phần demo đổ.
2. **Báo cáo chưa có minh chứng:** 15 chỗ `[CHÈN ẢNH]`, 0 ảnh, chưa có tên/mã học viên, chưa có ảnh SonarQube dù tiêu đề ghi "SAST".
3. **Không tái hiện được "trước khi sửa":** bộ ZIP không kèm source gốc (`RealTimeChatApp.rar`) và không có thư mục `.git`. Vì vậy không có commit SHA, không chạy được `gitleaks git` và không có ảnh FAIL của SEC-01..05.
4. **Hạn nộp:** báo cáo ghi hạn "trước 31/10/2026", hôm nay đã là 02/11/2026. Cần hỏi giảng viên ngay xem còn được nộp bổ sung không.

Sau khi sửa các mục P0 ở mục 3 và chạy đủ các bước ở mục 4, bài **đủ điều kiện nộp ở mức khá**. Muốn đạt mức tốt cho tiểu luận cao học, nên thay một phần lỗi "mô phỏng" bằng 3–4 lỗi **thật** mình tìm thấy trong source. Đồng thời bổ sung cơ sở lý thuyết, mô hình mối đe dọa và căn cứ pháp lý hiện hành (Luật 91/2025/QH15, Nghị định 356/2025/NĐ-CP).

## 2. Hiện trạng bộ tài liệu và kết quả chạy thử

Mình đã giải nén hai file ZIP và chạy thử mọi thứ chạy được trên Linux sandbox. File Word trong ZIP giống hệt file Word đính kèm. Mạng sandbox chặn Docker Hub, ghcr.io và NuGet, nên Gitleaks, Trivy và bước build backend .NET **chưa được kiểm chứng**. Nhóm phải tự chạy các phần này trên Windows.

| Thành phần | Đánh giá | Kết quả mình chạy thử (02/11/2026) |
| --- | --- | --- |
| `scripts/verify-training-cases.py` | Tốt, có kiểm soát âm/dương | 20/20 PASS (10 SEC × vulnerable/fixed) |
| `scripts/verify-quality-cases.py` | Chỉ là regex guard, không phải SAST | 10/10 PASS (5 CQ × 2) |
| Checkov trên Dockerfile thật (CKV\_DOCKER\_3/7/8) | Đúng | 6 passed, 0 failed |
| Checkov toàn bộ rule trên Dockerfile thật | Có 1 lỗi thật chưa được nêu trong báo cáo | 117 passed, **1 failed: CKV\_DOCKER\_2** (backend thiếu HEALTHCHECK) |
| Checkov SEC-08 | Đúng, nhưng chỉ CKV\_DOCKER\_8 fail | vulnerable: exit 1 (CKV\_DOCKER\_8 FAIL, CKV\_DOCKER\_3 PASS); fixed: exit 0 |
| `docker compose config --format json` | Đúng | Render OK; chỉ frontend publish `127.0.0.1:3000`; mạng `private` có `internal: true` |
| Rego `policies/compose.rego` | Logic đúng | compose thật: 0 deny; bad.json: 5 deny; SEC-09/10 vulnerable: có deny; fixed: 0 deny (chạy bằng engine regopy, nhóm vẫn phải chụp bằng OPA thật) |
| Frontend `npm ci` + `npm run build` | Đúng, đã bỏ `@esbuild/win32-x64` | 117 package, build OK 2,5 giây |
| Service `db` với `cap_drop: [ALL]` | **Lỗi chặn** | Mô phỏng: root + cap\_drop ALL thì `chmod` và `setresuid` bị từ chối; chạy `--user 70:70` thì OK |
| Workflow `security-lab.yml` | Cấu trúc tốt, có 3 rủi ro (mục 3) | Chưa chạy được, cần runner Windows |
| Báo cáo Word (\~12 trang, 53 KB) | Khung tốt, nội dung lặp, chưa có bằng chứng | 15 placeholder ảnh, 0 ảnh |

**Điểm mạnh nên giữ khi bảo vệ:**

- Tách rõ lỗi thật (SEC-01..05) và lỗi mô phỏng (SEC-06..10, CQ-01..05).
- Mỗi lỗi có cặp vulnerable/fixed, đây là cách làm chuẩn để chứng minh một policy có tác dụng.
- Không tự ghi PASS, không dùng `continue-on-error`, không chạy PR từ fork trên self-hosted runner, secret nằm ngoài repo.
- Thừa nhận SEC-05 còn mở và ghi nhận thay đổi pháp lý 2026.

## 3. Lỗi và điểm chưa chuẩn cần sửa trước khi nộp

Có 15 điểm: 5 điểm P0 bắt buộc sửa, 7 điểm P1 nên sửa để bài đạt chuẩn cao học, 3 điểm P2 làm sạch. Cách sửa chi tiết nằm ở Bước 1 của mục 4.

| # | Mức | Vấn đề | Cách sửa |
| --- | --- | --- | --- |
| 1 | P0 | `compose.yaml`: `db` chạy root với `cap_drop: [ALL]`, nên entrypoint Postgres không `chmod`/`su-exec` được và container restart liên tục | Thêm `user: "70:70"` (UID postgres của image alpine), giữ `cap_drop: [ALL]`. Viết thành bài học "hardening cũng cần test" trong báo cáo |
| 2 | P0 | Báo cáo có 15 placeholder ảnh, chưa có tên/mã HV, giảng viên, lớp | Chạy mục 4 và chụp theo danh mục bằng chứng ở mục 5 |
| 3 | P0 | Không có source gốc và `.git`, nên không có trạng thái "trước", commit SHA hay quét lịch sử | Tạo repo 2 commit: `baseline` (source gốc) và `hardening` (bản lab), xem Bước 2 |
| 4 | P0 | Hạn 31/10/2026 đã qua | Liên hệ giảng viên ngay; sửa các dòng ngày tháng và mục "Tiến độ" trong báo cáo |
| 5 | P0 | Pháp lý: NĐ 13/2023 đã hết hiệu lực từ 01/01/2026, thay bằng NĐ 356/2025/NĐ-CP (ban hành 31/12/2025); Luật 91/2025/QH15 hiệu lực 01/01/2026 | Lấy Luật 91/2025 + NĐ 356/2025 làm căn cứ chính, NĐ 13 chỉ để đối chiếu nếu đề bắt buộc. Tra số điều từ văn bản chính thức, không lấy từ bài tóm tắt |
| 6 | P1 | Scanner dùng tag `:latest` (gitleaks, checkov, opa, trivy), mâu thuẫn với chính rule CKV\_DOCKER\_7. Ngày 19/03/2026 Trivy bị tấn công chuỗi cung ứng: các bản 0.69.4–0.69.6 trên Docker Hub bị nhiễm, `latest` từng trỏ vào 0.69.6 | Ghim version + digest cho cả 4 image; đối chiếu bản Trivy với advisory GHSA-69fq-xp46-6x23. Đưa sự cố này vào phần thảo luận, đây là điểm cộng lớn |
| 7 | P1 | Trivy `--exit-code 1` cho HIGH/CRITICAL gần như chắc chắn đỏ trên base image node/nginx/aspnet, nhưng chưa có quy trình xử lý | Thêm `--ignore-unfixed` và `.trivyignore` có lý do, người duyệt, hạn; bảng residual risk trong báo cáo |
| 8 | P1 | Workflow dùng `shell: powershell` (Windows PowerShell 5.1). GitHub chèn `$ErrorActionPreference = 'stop'`, nên stderr của docker/gitleaks có thể bị coi là lỗi và làm job đỏ giả (cần kiểm chứng trên runner) | Đổi sang `shell: pwsh` (PowerShell 7) hoặc thêm `$ErrorActionPreference='Continue'` đầu mỗi step, chỉ dựa vào `$LASTEXITCODE` |
| 9 | P1 | `.gitignore` có dòng `Migrations/`, nên khi push lên GitHub thì mất migrations. Clone về chạy sẽ lỗi `relation ... does not exist` | Xóa dòng `Migrations/` khỏi `.gitignore` |
| 10 | P1 | Phần "Tác động/rủi ro" và "Liên hệ NĐ 13" lặp nguyên văn 10 lần, trông như sinh tự động | Viết riêng cho từng SEC: kịch bản tấn công, CWE, OWASP Top 10, mức độ (CVSS hoặc likelihood × impact) |
| 11 | P1 | 5/10 SEC là mô phỏng. Source thật có lỗi thật chưa được khai thác (xem bảng ngay dưới) | Bổ sung ít nhất 2–3 lỗi thật; giữ SEC-06..10 làm "regression test cho policy" |
| 12 | P1 | Không có kết quả SonarQube thật, dù tiêu đề ghi "SAST hiện có" | Chạy `sonar-scan.yml` và chụp Issues/Security Hotspots; quét CQ trong project sandbox (Bước 12) |
| 13 | P2 | Gitleaks `dir /work` sẽ quét cả `frontend/node_modules` sau khi `npm ci` (bước test SignalR): chậm và dễ báo nhầm | Chạy Gitleaks trước `npm ci`, hoặc thêm `node_modules/`, `dist/` vào allowlist paths |
| 14 | P2 | Hướng dẫn ghi kỳ vọng `PASS: server denied non-member JoinRoom`, nhưng script thực tế in `PASS SEC-03: JoinRoom denied for non-member`. Hướng dẫn yêu cầu chụp "Checkov CKV\_DOCKER\_3" FAIL, nhưng SEC-08 vulnerable chỉ fail CKV\_DOCKER\_8 | Sửa câu chữ trong hướng dẫn và báo cáo cho khớp output thật |
| 15 | P2 | Báo cáo thiếu phần học thuật: cơ sở lý thuyết, threat model, phương pháp, bảng kết quả định lượng, thảo luận | Theo cấu trúc đề xuất ở mục 6 |

**#16 (P0, phát hiện khi đóng gói bản v2):** bước Checkov trong `security-lab.yml` và `scan-local.ps1` quét cả thư mục `training/`. Fixture `SEC-08/vulnerable/Dockerfile` (`USER root`) vì vậy luôn làm gate **đỏ**, kể cả khi code thật sạch. Mình đã chạy thử: 11 passed, 1 failed (CKV\_DOCKER\_8 ở fixture). Thêm `--skip-path training` thì 6 passed, 0 failed. Bản v2 đã sửa.

**Lỗi thật mình tìm thấy trong source (nên đưa vào báo cáo):**

| Mã đề xuất | Vị trí | Lỗi | CWE | Cách chứng minh |
| --- | --- | --- | --- | --- |
| SEC-11 | `MessagesController.UploadMedia` | Tin `Content-Type` do client gửi và giữ nguyên đuôi file gốc. File `.svg` hoặc `.html` khai `image/...` được lưu và phục vụ cùng origin `localhost:3000`. Kết hợp với token trong `localStorage`, đây là chuỗi stored XSS có thể đánh cắp phiên | CWE-434, CWE-79 | Bước 8: upload SVG chứa `alert(document.domain)` bằng tài khoản giả, mở URL |
| SEC-12 | `AuthController` / `Program.cs` | Không có rate limiting cho đăng nhập | CWE-307 | Gửi 30 request login sai liên tiếp, không có HTTP 429 |
| SEC-13 | `frontend/nginx.conf` | Không có security header (CSP, X-Content-Type-Options, X-Frame-Options, Referrer-Policy) | CWE-693 | `curl.exe -I http://localhost:3000/` |
| SEC-14 | `backend/Dockerfile` | Thiếu HEALTHCHECK | CIS Docker 4.6 | Checkov toàn bộ rule: CKV\_DOCKER\_2 FAILED (đã xác nhận) |

Nguồn về sự cố Trivy: [Infosecurity Magazine](https://www.infosecurity-magazine.com/news/trivy-supply-chain-attack-expands/), [CVE-2026-33634](https://stack.watch/vuln/CVE-2026-33634/). Nguồn pháp lý: [NĐ 356/2025/NĐ-CP trên LuatVietnam](https://luatvietnam.vn/dan-su/nghi-dinh-356-2025-nd-cp-quy-dinh-chi-tiet-luat-bao-ve-du-lieu-ca-nhan-422896-d1.html), [Luật 91/2025/QH15 trên LuatVietnam](https://luatvietnam.vn/dan-su/luat-bao-ve-du-lieu-ca-nhan-2025-so-91-2025-qh15-405135-d1.html).

## 4. Quy trình lab chuẩn hóa – step by step

Chạy đúng thứ tự từ Bước 0 đến Bước 14 trên một máy Windows có Docker Desktop. Mỗi bước ghi rõ: lệnh, kết quả kỳ vọng, ảnh cần chụp. Mọi lệnh chạy trong **PowerShell 7** (`pwsh`), trừ khi ghi khác.

### Bản v3 – vá hoàn chỉnh, dùng để chạy demo

`RealTimeChatApp_DevSecOps_Lab_v3.zip` đã vá toàn bộ lỗi thật còn mở: SEC-05, SEC-11, SEC-12, SEC-13, SEC-14. Bản này cũng sửa 13 chỗ `Forbid("...")` dùng sai, vốn khiến request bị từ chối trả sai mã thay vì 403. Chi tiết nằm trong `CHANGELOG-V3.md`.

| Mã | Trước (bản v2) | Sau (bản v3) |
| --- | --- | --- |
| SEC-05 | `/uploads/<file>` tải được không cần token | `/uploads/*` không còn tới backend; `GET /api/messages/{id}/media` không token trả 401, người ngoài phòng trả 403 |
| SEC-11 | Upload SVG chạy được `alert` | Upload SVG/HTML trả 400; server tự đặt đuôi file theo magic bytes |
| SEC-12 | 30 lần login sai không bị chặn | Từ lần thứ 11 trong 1 phút trả 429 |
| SEC-13 | Không có CSP và các header bảo mật | Có CSP và 4 header bảo mật khác |
| SEC-14 | Checkov CKV\_DOCKER\_2 FAILED | Checkov 118 passed, 0 failed; backend `healthy` |

Mình đã kiểm chứng được các phần sau:

- Biên dịch code backend đã sửa bằng .NET 8: 0 lỗi.
- Rate limiter: request thứ 11 trả 429.
- Bộ nhận diện file: từ chối SVG và HTML.
- `nginx -t` OK; trang login chạy dưới CSP với 0 vi phạm.
- Checkov 118 passed, 0 failed.

Chưa kiểm chứng: build image bằng .NET 9 và luồng upload/xem ảnh end-to-end. Đây là việc cần chạy thử trên Windows trước buổi demo.

**Demo nhanh với bản v3** (sau Bước 0, 3):

```powershell
cd C:\DevSecOps\RealTimeChatApp_DevSecOps_Lab
pwsh -File .\scripts\run-lab.ps1 -Action up        # build + chạy 3 service
pwsh -File .\scripts\run-lab.ps1 -Action status    # db healthy, backend không lỗi
pwsh -File .\scripts\run-lab.ps1 -Action runtime   # 15 kiểm tra runtime, kỳ vọng 0 FAIL
pwsh -File .\scripts\run-lab.ps1 -Action demo      # 15 fixture + Gitleaks/Checkov/OPA
pwsh -File .\scripts\run-lab.ps1 -Action scan      # quét app thật + Trivy
```

Bước `runtime` gửi 12 lần login sai, nên máy đó bị khóa đăng nhập khoảng 60 giây. Muốn bỏ bước này thì chạy trực tiếp `.\scripts\runtime-checks.ps1 -SkipRateLimit`.

**Bổ sung v3.1 – kiểm thử runtime cho TV2 và TV3** (script hỏi username/mật khẩu tài khoản test, nhập ẩn, không in token):

```powershell
pwsh -File .\scripts\run-lab.ps1 -Action signalr   # TV2: B (không thuộc phòng) gọi JoinRoom/SendTyping -> PASS SEC-03/04
pwsh -File .\scripts\run-lab.ps1 -Action upload    # TV3: SVG bị 400, /uploads không trả ảnh, API media 401/200
```

- `signalr` cần Node.js 18+ (`node -v`); lần đầu script tự chạy `npm ci` trong `frontend`. Chuẩn bị 2 tài khoản: A tạo phòng riêng **không** thêm B. Script đăng nhập A để chọn phòng, đăng nhập B rồi chạy `frontend/scripts/test-room-auth.mjs`.
- `upload` tạo vài tin nhắn ảnh thử trong phòng được chọn – dùng phòng test.
- Chạy 2 script này trên bản v2 (tag `hardening-v2`) sẽ ra FAIL ở SEC-05/SEC-11 – đó chính là ảnh "trước khi vá".
- Mỗi lần đăng nhập tính vào rate limit 10 lần/phút; nếu vừa chạy `runtime` (có bước 12 lần login sai) thì đợi 60 giây.

**Lấy ảnh trước/sau bằng repo 3 commit.** Ở Bước 2, sau commit `baseline`, làm thêm:

1. `robocopy` bản v2 vào repo, commit, gắn tag `hardening-v2`.
2. `robocopy` bản v3 vào repo, commit, gắn tag `hardening-v3`.
3. Khi cần ảnh "trước" của SEC-11..14: `git checkout hardening-v2 -- <file>`, chạy `docker compose ... up -d --build`, test và chụp. Xong thì `git checkout hardening-v3 -- <file>` và build lại.

Danh sách file cần checkout nằm trong bảng của `CHANGELOG-V3.md`. Bước 8–9 bên dưới mô tả trạng thái "trước" (bản v2); trạng thái "sau" theo bảng trên.

### Bước 0 – Chuẩn bị máy và thư mục

Cài: Docker Desktop (WSL2, Linux containers), Git, PowerShell 7 (`winget install Microsoft.PowerShell`), Node.js 22, .NET 9 SDK (cho SonarQube sandbox), SonarQube đang chạy ở `http://localhost:9000`, GitHub self-hosted runner bản 2.327.1 trở lên.

Bố trí 3 thư mục:

```text
C:\DevSecOps\RealTimeChatApp                  <- source GỐC giải nén từ RAR (trạng thái "trước")
C:\DevSecOps\RealTimeChatApp_DevSecOps_Lab    <- bản lab giải nén từ ZIP (trạng thái "sau")
C:\DevSecOps\chatapp-repo                     <- repo Git dùng để chạy lab (tạo ở Bước 2)
```

Kiểm tra:

```powershell
git --version; node --version; dotnet --version; $PSVersionTable.PSVersion
docker version
docker compose version
docker info --format '{{.OSType}}'     # phải ra: linux
```

**Đạt khi:** `docker info` trả `linux`, PowerShell ≥ 7. **Ảnh LAB-01:** kết quả các lệnh trên.

### Bước 1 – Sửa các lỗi kỹ thuật của bản lab trước khi chạy

Làm trong `C:\DevSecOps\RealTimeChatApp_DevSecOps_Lab`. Đây là các mục P0/P1 ở mục 3.

> **Dùng bản `v2 hoặc v3`:** các mục 1a, 1b, 1d, 1e và lỗi #16 đã được áp dụng sẵn. Chỉ còn **1c** phải tự làm: ghi digest vào `scripts/tool-images.ps1` (cho script local) và khối `env:` đầu `security-lab.yml` (cho CI). Khi chưa ghim, script sẽ in cảnh báo. Danh sách thay đổi nằm trong `CHANGELOG-V2.md`. Nếu dùng bản ZIP cũ, làm đủ 1a–1e bên dưới.

**1a. Sửa service `db` trong `compose.yaml`** (mục 3, #1). Thêm dòng `user`:

```yaml
  db:
    image: postgres:16-alpine
    user: "70:70"        # UID/GID postgres trong image alpine -> giữ được cap_drop: [ALL]
    environment:
      ...
```

Nếu đã từng `up` và volume `pgdata` được tạo dở dang, xóa volume demo một lần: `docker compose --env-file "$env:USERPROFILE\chatapp-lab.env" down -v`.

**1b. Bỏ `Migrations/` khỏi `.gitignore`** (mục 3, #9):

```powershell
(Get-Content .gitignore) | Where-Object { $_.Trim() -ne 'Migrations/' } | Set-Content .gitignore
```

**1c. Ghim phiên bản scanner** (mục 3, #6). Chọn phiên bản cụ thể cho từng tool. Với Trivy, **không** dùng 0.69.4, 0.69.5, 0.69.6 và phải đối chiếu advisory GHSA-69fq-xp46-6x23. Sau đó lấy digest:

```powershell
$GITLEAKS = 'ghcr.io/gitleaks/gitleaks:<phien-ban>'
$CHECKOV  = 'bridgecrew/checkov:<phien-ban>'
$OPA      = 'openpolicyagent/opa:<phien-ban>'
$TRIVY    = 'aquasec/trivy:<phien-ban-da-xac-minh>'
foreach ($i in $GITLEAKS,$CHECKOV,$OPA,$TRIVY) {
  docker pull $i | Out-Null
  docker image inspect $i --format '{{index .RepoDigests 0}}'
}
```

Ghi 4 digest vào bảng "Công cụ và phiên bản" trong báo cáo. Thay mọi chỗ `:latest` trong `scripts/*.ps1` và `.github/workflows/security-lab.yml` bằng `image@sha256:...`. Để các lệnh bên dưới dùng bản đã ghim, gán lại biến bằng digest, ví dụ `$GITLEAKS = 'ghcr.io/gitleaks/gitleaks@sha256:...'`.

**1d. Workflow** (mục 3, #7 và #8). Trong cả hai job của `security-lab.yml`:

```yaml
    defaults:
      run:
        shell: pwsh          # thay cho: powershell
```

Thêm `--ignore-unfixed` vào hai lệnh Trivy. Tạo `.trivyignore` rỗng; mỗi CVE chấp nhận sau này phải có dòng chú thích: lý do, người duyệt, hạn xem lại.

**1e. Allowlist Gitleaks** (mục 3, #13). Trong `.gitleaks.toml`:

```toml
paths = ['''training/''', '''security-reports/''', '''node_modules/''', '''(^|/)dist/''']
```

**Đạt khi:** `docker compose --env-file lab.env.example config --quiet` không lỗi.

### Bước 2 – Tạo repo Git có trạng thái "trước" và "sau"

Bước này tạo ra commit SHA, diff trước/sau và lịch sử Git có secret thật để Gitleaks bắt. Đây là bằng chứng mạnh nhất của bài.

```powershell
$orig = 'C:\DevSecOps\RealTimeChatApp'
$lab  = 'C:\DevSecOps\RealTimeChatApp_DevSecOps_Lab'
$repo = 'C:\DevSecOps\chatapp-repo'

robocopy $orig $repo /E /XD node_modules bin obj .vs .git dist /NFL /NDL /NJH
Set-Location $repo
git init -b main
git add -A
git commit -m "baseline: source goc RealTimeChatApp (chua sua)"
git tag baseline

robocopy $lab $repo /MIR /XD .git node_modules bin obj dist /NFL /NDL /NJH
git add -A
git commit -m "hardening: secrets, SignalR authz, Docker non-root, Compose policy"
git tag hardening

git log --oneline --decorate
git diff --stat baseline hardening
```

`robocopy` trả mã 1–7 là thành công. Nếu source gốc đã có `.git` với lịch sử thật, dùng luôn lịch sử đó thay cho commit baseline.

**Cảnh báo:**

- Commit `baseline` chứa JWT secret và mật khẩu DB cũ. Chỉ push lên repo **private**.
- Coi hai giá trị đó là đã lộ, không dùng lại.
- Không chụp `git diff` của `appsettings.json`, vì diff in ra secret cũ. Chỉ chụp `--stat` hoặc diff của `ChatHub.cs`.

Từ đây, mọi bước chạy trong `$repo`. **Ảnh LAB-01b:** `git log --oneline` (có 2 tag) và `git diff --stat`.

### Bước 3 – Tạo secret cục bộ ngoài repo

```powershell
$rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
$db = New-Object byte[] 24;  $rng.GetBytes($db)
$jw = New-Object byte[] 48;  $rng.GetBytes($jw)
$envf = Join-Path $env:USERPROFILE 'chatapp-lab.env'
@("DB_PASSWORD=$([Convert]::ToBase64String($db))",
  "JWT_SECRET=$([Convert]::ToBase64String($jw))") | Set-Content -Encoding ascii $envf
Remove-Variable db,jw
git status --short      # không được thấy file .env nào
```

JWT 48 byte (Base64 64 ký tự) đáp ứng kiểm tra `>= 32 byte` trong `Program.cs`. Không mở file này khi đang chia sẻ màn hình.

### Bước 4 – Build và chạy ứng dụng

```powershell
$envf = "$env:USERPROFILE\chatapp-lab.env"
docker compose --env-file $envf config --quiet
docker compose --env-file $envf up -d --build
docker compose --env-file $envf ps
docker compose --env-file $envf logs --tail 60 db backend
curl.exe -s -o NUL -w "%{http_code}`n" http://localhost:3000/
docker ps --format "table {{.Names}}`t{{.Ports}}"
Test-NetConnection 127.0.0.1 -Port 5432 | Select-Object TcpTestSucceeded
docker inspect chatapp-devsecops-db-1 --format "user={{.Config.User}} capdrop={{.HostConfig.CapDrop}} secopt={{.HostConfig.SecurityOpt}}"
```

**Đạt khi:**

- 3 service đều `Up`, `db` ở trạng thái `(healthy)`.
- Log backend có dòng apply migration, không có exception.
- `curl` trả `200`.
- Chỉ frontend có cổng `127.0.0.1:3000->8080/tcp`.
- `TcpTestSucceeded : False` cho cổng 5432.
- `db` có `user=70:70 capdrop=[ALL]`.

Nếu `db` restart liên tục với lỗi "Operation not permitted" thì chưa làm Bước 1a. **Ảnh LAB-02:** `docker compose ps`, `docker ps` (cổng), `Test-NetConnection`.

### Bước 5 – Chạy 2 luồng nghiệp vụ bằng dữ liệu giả

1. Trình duyệt thường: đăng ký A (`alice_test`, `alice@example.com`). Trình duyệt InPrivate hoặc profile khác: đăng ký B (`bob_test`, `bob@example.com`).
2. A tạo phòng `Phong-Test-A`, **không** thêm B. A gửi 2 tin nhắn văn bản và 1 ảnh PNG bất kỳ.
3. Lấy RoomId: DevTools của A → Network → request `/api/rooms` → tab Response → trường `id` của phòng.
4. Lấy token của B (chỉ trong lab): DevTools của B → Application → Local Storage → `http://localhost:3000` → key `token`. Token sống 30 phút, nên làm Bước 7 ngay sau đó.

**Ảnh LAB-03:** màn hình đăng ký/đăng nhập, phòng chat có tin nhắn và ảnh. Làm mờ mọi token nếu lọt vào ảnh.

### Bước 6 – TV1: Secret scanning với Gitleaks (SEC-01, SEC-02)

Chạy **trước** khi `npm ci` ở Bước 7.

```powershell
$root = (Get-Location).Path

# (a) Working tree đã sửa -> kỳ vọng exit 0
docker run --rm --mount "type=bind,source=$root,target=/work,readonly" $GITLEAKS `
  dir /work --config /work/.gitleaks.toml --redact --exit-code 1
"exit=$LASTEXITCODE"

# (b) Toàn bộ lịch sử Git -> kỳ vọng exit 1, finding nằm ở commit baseline
docker run --rm -e GIT_CONFIG_COUNT=1 -e GIT_CONFIG_KEY_0=safe.directory -e GIT_CONFIG_VALUE_0='*' `
  --mount "type=bind,source=$root,target=/work,readonly" $GITLEAKS `
  git /work --config /work/.gitleaks.toml --redact --exit-code 1 -v
"exit=$LASTEXITCODE"

# (c) Fixture đào tạo -> vulnerable exit 1, fixed exit 0
foreach ($id in 'SEC-01','SEC-02') { foreach ($m in 'vulnerable','fixed') {
  docker run --rm --mount "type=bind,source=$root,target=/work,readonly" $GITLEAKS `
    dir "/work/training/security/$id/$m" --config /work/training/gitleaks-demo.toml --redact --exit-code 1
  "$id $m exit=$LASTEXITCODE" } }
```

**Kết quả kỳ vọng:**

- (b) báo rule `chatapp-jwt-secret-in-appsettings` và `chatapp-postgresql-password-in-appsettings`, giá trị hiển thị `REDACTED`, kèm commit SHA của baseline.
- Nếu (a) có finding từ rule mặc định, ví dụ `generic-api-key` bắt placeholder trong `lab.env.example`: đọc từng dòng. Chỉ allowlist đúng file placeholder đó và ghi lý do vào báo cáo.
- Nếu (b) báo `dubious ownership` thì kiểm tra lại 3 biến `GIT_CONFIG_*`.

**Trong báo cáo, viết quy trình xử lý sự cố:** phát hiện → xóa khỏi code → **rotate** (đã làm ở Bước 3) → vô hiệu hóa refresh token cũ → cân nhắc viết lại lịch sử. Sửa file không xóa được commit cũ.

**Ảnh SEC-01/02:** (a) exit 0, (b) finding đã redact và exit 1, (c) 4 dòng exit.

## 5. Phân công 5 thành viên và danh mục minh chứng

Giữ phân công cũ và gán thêm 4 lỗi thật (SEC-11..14). Như vậy mỗi thành viên có ít nhất **một lỗi tìm thấy trong source thật**, không chỉ lỗi mô phỏng. Đây là thay đổi quan trọng nhất để bài đạt chuẩn cao học.

| TV | Lỗi thật | Lỗi mô phỏng | CQ | Công cụ | Bước |
| --- | --- | --- | --- | --- | --- |
| TV1 (trưởng nhóm) | SEC-01, SEC-02 (secret trong lịch sử Git) | — | CQ-01 | Gitleaks (dir + git) | 2, 3, 6 |
| TV2 | SEC-03, SEC-04 (SignalR thiếu kiểm tra thành viên) | — | CQ-02 | Script SignalR, git diff | 7 |
| TV3 | SEC-05 (upload public), **SEC-11** (SVG XSS) | SEC-06 (JWT lifetime) | CQ-03 | curl, trình duyệt, guard | 8 |
| TV4 | **SEC-13** (thiếu header), **SEC-14** (CKV\_DOCKER\_2) | SEC-07 (CORS), SEC-08 (USER root) | CQ-04 | Checkov, curl | 9 |
| TV5 | **SEC-12** (không rate limit login); bài học Postgres `cap_drop` | SEC-09 (privileged), SEC-10 (port DB) | CQ-05 | OPA/Rego, docker inspect | 4, 10 |
| Cả nhóm | Trivy CVE thật, SonarQube thật | Regression 15 fixture | — | Trivy, SonarQube, GitHub Actions | 11, 12, 13 |

Kiểm chứng SEC-12 (TV5) bằng 30 lần đăng nhập sai vào tài khoản giả:

```powershell
1..30 | ForEach-Object {
  curl.exe -s -o NUL -w "%{http_code} " -X POST http://localhost:3000/api/auth/login `
    -H "Content-Type: application/json" -d '{"emailOrUserName":"alice_test","password":"Sai-mat-khau-1"}' }
```

Không thấy `429` nghĩa là chưa có giới hạn. Vá bằng `builder.Services.AddRateLimiter(...)` (fixed window cho `/api/auth/login`) và `app.UseRateLimiter()`, sau đó chạy lại và phải thấy `429`. Mở `LoginCommand` (tên trường là emailOrUserName); lệnh cần PowerShell 7.3 trở lên để truyền chuỗi JSON đúng.

**Mỗi lỗi trong báo cáo cần đủ 9 thành phần, theo đúng thứ tự:**

1. Mã lỗi, nguồn gốc (thật hoặc mô phỏng), vị trí file:dòng, commit `baseline`.
2. Kịch bản tấn công riêng cho lỗi đó (2–3 câu, không dùng câu chung).
3. Phân loại: CWE, OWASP Top 10, pha DevSecOps (Code/Build/Deploy/Operate), mức độ.
4. Công cụ + phiên bản/digest + rule ID.
5. Ảnh FAIL hoặc ảnh khai thác, đã che token.
6. Đoạn code sửa (diff).
7. Ảnh PASS sau khi sửa, hoặc ghi "chưa khắc phục" kèm kế hoạch.
8. Giới hạn của kiểm thử (guard theo pattern ≠ chứng minh runtime).
9. Liên hệ pháp lý: nguyên tắc hoặc nghĩa vụ bảo vệ nào trong Luật 91/2025 / NĐ 356/2025 bị đe dọa. Viết là "nguy cơ không đáp ứng", không viết "vi phạm".

**Danh mục ảnh cần có** (đặt tên file đúng mã để chèn vào Word):

- [ ] LAB-01 môi trường và phiên bản tool; LAB-01b git log với 2 tag
- [ ] LAB-02 compose ps, cổng, Test-NetConnection 5432
- [ ] LAB-03 hai luồng nghiệp vụ (đăng ký/đăng nhập; chat + media)
- [ ] SEC-01/02 Gitleaks: working tree exit 0, lịch sử exit 1 đã redact, fixture
- [ ] SEC-03/04 SignalR: FAIL trên baseline, diff, PASS trên hardening
- [ ] SEC-05 curl 200 không token; SEC-11 alert SVG; SEC-06 401 token hết hạn
- [ ] SEC-07 CORS 2 origin; SEC-08 Checkov vulnerable/fixed; SEC-13 header trước/sau; SEC-14 CKV\_DOCKER\_2 trước/sau
- [ ] SEC-09/10 OPA deny/pass; SEC-12 chuỗi 401 → 429
- [ ] LAB-04 Trivy 2 image, trước/sau
- [ ] CQ-01..05 SonarQube hoặc guard, trước/sau; SonarQube Overview của source thật
- [ ] LAB-05 GitHub Actions: run xanh, run đỏ do policy, artifact

## 6. Checklist trước khi nộp và cấu trúc tiểu luận đề xuất

Bài chỉ được nộp khi mọi ô dưới đây đã tick.

**Checklist nộp bài:**

- [ ] Đã xác nhận với giảng viên hạn nộp mới (hạn cũ 31/10/2026 đã qua)
- [ ] Đã sửa 5 mục Bước 1 (db `user: 70:70`, Migrations, ghim image, `pwsh`, allowlist)
- [ ] Repo có tag `baseline` và `hardening`; mọi ảnh có commit SHA và thời gian
- [ ] Đủ ảnh theo danh mục ở mục 5; 15 placeholder `[CHÈN ẢNH]` đã thay hết
- [ ] Bìa đã điền tên, mã học viên 5 người, giảng viên, lớp
- [ ] Phần "Tác động" và "Liên hệ pháp lý" viết riêng cho từng SEC, không còn đoạn lặp nguyên văn
- [ ] Căn cứ pháp lý chính là Luật 91/2025/QH15 và NĐ 356/2025/NĐ-CP; số điều đã tra từ văn bản chính thức
- [ ] Có số liệu SonarQube thật; CQ chỉ ghi rule ID mà Sonar đã hiển thị
- [ ] Không có câu "bảo mật 100%", "đáp ứng hoàn toàn", "đã khắc phục SEC-05" nếu chưa làm
- [ ] Hướng dẫn lab và báo cáo khớp output thật (chuỗi PASS của script SignalR, CKV\_DOCKER\_8)
- [ ] Word ≤ 30 trang, ≤ 10 MB (ảnh JPG \~150 KB/ảnh, chỉ cắt vùng cần thiết)
- [ ] ZIP nộp kèm không có `.env`, token, `node_modules`, `bin/obj`, file `.tar`, dữ liệu chat; có `.git` nếu giảng viên cần lịch sử (secret cũ đã vô hiệu)
- [ ] Mỗi thành viên tự demo lại được phần của mình trong 5 phút

**Cấu trúc tiểu luận đề xuất (\~25–30 trang):**

| Chương | Nội dung | Số trang |
| --- | --- | --- |
| Mở đầu | Bối cảnh, mục tiêu, phạm vi, đóng góp của nhóm, bảng phân công | 1–2 |
| 1. Cơ sở lý thuyết | DevSecOps và shift-left; NIST SP 800-218 (SSDF); OWASP Top 10, ASVS, DSOMM; Security/Policy-as-Code; supply-chain security (ví dụ sự cố Trivy 03/2026) | 4–5 |
| 2. Phân tích hệ thống và mối đe dọa | Kiến trúc, 2 luồng nghiệp vụ, sơ đồ luồng dữ liệu và biên tin cậy, threat model STRIDE, dữ liệu cá nhân cần bảo vệ | 4 |
| 3. Thiết kế pipeline DevSecOps | Môi trường local, pipeline 2 job, các gate và tiêu chí fail, bảng công cụ + digest, phương pháp kiểm soát âm/dương | 3–4 |
| 4. Thực nghiệm và kết quả | Mỗi SEC/CQ theo 9 thành phần ở mục 5; bảng tổng hợp trước/sau (số finding Gitleaks, Checkov, OPA, Trivy, Sonar) | 10–12 |
| 5. Đối chiếu pháp lý | Luật 91/2025 + NĐ 356/2025, NĐ 13/2023 đối chiếu lịch sử; ma trận SEC → nghĩa vụ bảo vệ; ranh giới "nguy cơ" và "vi phạm" | 2–3 |
| 6. Thảo luận | Giới hạn (guard theo pattern, chưa có DAST/pentest, SEC-05 mở), rủi ro còn lại, bài học (hardening làm hỏng Postgres, scanner bị tấn công chuỗi cung ứng) | 1–2 |
| Kết luận và hướng phát triển | DAST (OWASP ZAP), SBOM (Syft), ký image (Cosign), BFF/cookie thay localStorage, media API có phân quyền | 1 |
| Phụ lục | Nhật ký bằng chứng, lệnh chạy, ảnh phụ | ngoài giới hạn nếu được phép |

**Nguồn đã tra:**

- [NĐ 356/2025/NĐ-CP – LuatVietnam](https://luatvietnam.vn/dan-su/nghi-dinh-356-2025-nd-cp-quy-dinh-chi-tiet-luat-bao-ve-du-lieu-ca-nhan-422896-d1.html)
- [Luật Bảo vệ dữ liệu cá nhân 91/2025/QH15 – LuatVietnam](https://luatvietnam.vn/dan-su/luat-bao-ve-du-lieu-ca-nhan-2025-so-91-2025-qh15-405135-d1.html)
- [Trivy Supply Chain Attack Expands – Infosecurity Magazine](https://www.infosecurity-magazine.com/news/trivy-supply-chain-attack-expands/)
- [CVE-2026-33634 – stack.watch](https://stack.watch/vuln/CVE-2026-33634/)
- [Checkov Dockerfile policy index](https://www.checkov.io/5.Policy%20Index/dockerfile.html)

### Bước 7 – TV2: Phân quyền SignalR (SEC-03, SEC-04): trước và sau khi vá

**7a. Trên bản đã vá. Kỳ vọng: 2 dòng PASS, exit 0.**

```powershell
Set-Location "$repo\frontend"
npm ci
$env:DEMO_TOKEN_B = Read-Host 'Dan token cua B'        # không lưu vào history
$env:DEMO_ROOM_A  = '<GUID-phong-cua-A>'
node .\scripts\test-room-auth.mjs; "exit=$LASTEXITCODE"
```

Output đúng: `PASS SEC-03: JoinRoom denied for non-member` và `PASS SEC-04: SendTyping denied for non-member`.

**7b. Tái hiện lỗi bằng `ChatHub.cs` gốc. Kỳ vọng: FAIL, exit 1.**

```powershell
Set-Location $repo
git checkout baseline -- backend/src/ChatApp.API/Hubs/ChatHub.cs
docker compose --env-file $envf up -d --build backend
Set-Location frontend; node .\scripts\test-room-auth.mjs; "exit=$LASTEXITCODE"
Set-Location $repo
git checkout hardening -- backend/src/ChatApp.API/Hubs/ChatHub.cs
docker compose --env-file $envf up -d --build backend
git status --short        # phải sạch
Remove-Item Env:DEMO_TOKEN_B, Env:DEMO_ROOM_A
```

Output đúng ở 7b: `FAIL SEC-03: non-member was allowed to invoke JoinRoom`. Nếu ra `INCONCLUSIVE` + 401 thì token đã hết hạn: đăng nhập lại B và lấy token mới. Nếu build backend lỗi vì `ChatHub.cs` gốc không tương thích, dùng fixture SEC-03/04 và ghi rõ giới hạn này.

**Ảnh SEC-03/04:** 7b FAIL → `git diff baseline hardening -- backend/src/ChatApp.API/Hubs/ChatHub.cs` → 7a PASS.

**Giới hạn cần nêu:** `OnConnectedAsync` tự add connection vào mọi phòng. User bị kick vẫn nhận sự kiện cho tới khi kết nối lại. Đây là đề xuất cải tiến.

### Bước 8 – TV3: Media và JWT (SEC-05, SEC-06, SEC-11 mới)

**8a. SEC-05: file upload đọc được không cần đăng nhập.** Chuột phải vào ảnh A đã gửi → Copy image address → lấy đường dẫn `/uploads/<guid>.png`:

```powershell
curl.exe -s -o NUL -w "%{http_code} %{content_type}`n" http://localhost:3000/uploads/<guid>.png
```

Kết quả `200 image/png` khi **không gửi token** là bằng chứng rò rỉ: bất kỳ ai biết URL đều tải được, kể cả B. Ghi "chưa khắc phục" và trình bày thiết kế vá:

- API `GET /api/media/{messageId}` có `[Authorize]`.
- Kiểm tra `RoomMember` của `Message.RoomId`.
- Lưu file ngoài `wwwroot`.
- Frontend tải bằng Bearer token rồi hiển thị qua blob URL.

**8b. SEC-11 (lỗi thật mới): upload SVG được thực thi script.** Chỉ làm trên lab, bằng tài khoản giả:

```powershell
'<svg xmlns="http://www.w3.org/2000/svg"><script>alert(document.domain)</script></svg>' |
  Set-Content -Encoding utf8 "$env:TEMP\xss-test.svg"
```

A gửi file `xss-test.svg` vào phòng qua giao diện. Trình duyệt gửi `image/svg+xml` nên backend chấp nhận và giữ đuôi `.svg`. Mở URL `/uploads/<guid>.svg` ở tab mới. Hiện hộp thoại `localhost` nghĩa là script chạy trên cùng origin với app, nên đọc được `localStorage.token`. Chuỗi rủi ro: CWE-434 → CWE-79 → chiếm phiên.

Bản vá đề xuất:

- Whitelist đuôi file `.jpg/.png/.gif/.webp/.mp4`, kiểm tra magic bytes, server tự đặt đuôi file.
- Loại bỏ SVG.
- Header `X-Content-Type-Options: nosniff` và CSP (Bước 9c).
- Về lâu dài, chuyển token khỏi `localStorage`.

Nếu kịp code bản vá, chạy lại: upload SVG phải bị `400`.

**8c. SEC-06: JWT hết hạn phải bị từ chối.** Chạy guard (`python scripts/verify-training-cases.py` hoặc Bước 11a). Kiểm chứng runtime: giữ một token cũ hơn 30 phút rồi gọi:

```powershell
$old = Read-Host 'Token da qua 30 phut'
curl.exe -s -i http://localhost:3000/api/rooms -H "Authorization: Bearer $old" | Select-String 'HTTP/|WWW-Authenticate'
```

Kỳ vọng `401` và `error="invalid_token"`, mô tả token expired. Điều này chứng minh `ValidateLifetime = true` và `ClockSkew = 0` trong `Program.cs`.

**Ảnh SEC-05/06/11:** curl 200 không token, hộp thoại alert (che token nếu có), 401 token hết hạn, fixture vulnerable/fixed SEC-06.

### Bước 9 – TV4: CORS, Dockerfile, security header (SEC-07, SEC-08, SEC-13, SEC-14)

**9a. SEC-07 CORS kiểm chứng runtime** (bổ sung cho fixture):

```powershell
foreach ($o in 'http://evil.example','http://localhost:3000') {
  "--- Origin: $o"
  curl.exe -s -i -X OPTIONS http://localhost:3000/api/auth/login `
    -H "Origin: $o" -H "Access-Control-Request-Method: POST" | Select-String 'HTTP/|Access-Control'
}
```

Kỳ vọng: `evil.example` **không** có `Access-Control-Allow-Origin`; `localhost:3000` có header này và `Access-Control-Allow-Credentials: true`.

**9b. SEC-08 và SEC-14 bằng Checkov:**

```powershell
$root = (Get-Location).Path
foreach ($m in 'vulnerable','fixed') {
  docker run --rm --mount "type=bind,source=$root,target=/work,readonly" $CHECKOV `
    --directory "/work/training/security/SEC-08/$m" --framework dockerfile --check CKV_DOCKER_3,CKV_DOCKER_8 --compact
  "SEC-08 $m exit=$LASTEXITCODE" }
# Dockerfile thật, toàn bộ rule, bỏ qua training
docker run --rm --mount "type=bind,source=$root,target=/work,readonly" $CHECKOV `
  --directory /work --framework dockerfile --skip-path training --compact
```

Kết quả mình đã chạy được:

- SEC-08 vulnerable: exit 1, chỉ **CKV\_DOCKER\_8** FAILED, CKV\_DOCKER\_3 PASS vì có `USER root`.
- SEC-08 fixed: exit 0.
- Dockerfile thật: 117 passed, 1 failed **CKV\_DOCKER\_2** ở `backend/Dockerfile`. Đây là SEC-14.

Vá SEC-14 bằng cách thêm vào `backend/Dockerfile` trước `USER app` (image aspnet không có curl, nên dùng bash):

```dockerfile
HEALTHCHECK --interval=30s --timeout=5s --start-period=30s --retries=3 \
  CMD ["bash", "-c", "exec 3<>/dev/tcp/127.0.0.1/8080"]
```

Commit, chạy lại Checkov (kỳ vọng 0 failed), rồi chạy `docker compose ... up -d --build backend` và `docker inspect chatapp-devsecops-backend-1 --format '{{.State.Health.Status}}'`. Kỳ vọng `healthy`.

**9c. SEC-13 security header:**

```powershell
curl.exe -s -I http://localhost:3000/
```

Trước khi sửa: không có CSP, `X-Content-Type-Options`, `X-Frame-Options`. Thêm vào khối `server` của `frontend/nginx.conf`:

```nginx
add_header X-Content-Type-Options "nosniff" always;
add_header X-Frame-Options "DENY" always;
add_header Referrer-Policy "no-referrer" always;
add_header Content-Security-Policy-Report-Only "default-src 'self'; connect-src 'self' ws://localhost:3000; img-src 'self' blob: data:; media-src 'self' blob:; object-src 'none'; frame-ancestors 'none'" always;
```

Dùng `Report-Only` trước. Kiểm tra Console không có vi phạm khi chat, rồi mới đổi thành `Content-Security-Policy`. Rebuild frontend và chạy lại `curl -I`.

**Ảnh SEC-07/08/13/14:** kết quả CORS 2 origin, Checkov vulnerable/fixed, CKV\_DOCKER\_2 trước/sau, header trước/sau.

### Bước 10 – TV5: Policy-as-Code với OPA cho Compose (SEC-09, SEC-10)

```powershell
$root = (Get-Location).Path
New-Item -ItemType Directory -Force security-reports | Out-Null
function Render($file, $out, $extra) {
  $raw = docker compose @extra -f $file config --format json
  [IO.File]::WriteAllText((Join-Path $root $out), ($raw -join "`n"), (New-Object Text.UTF8Encoding($false)))
}
function Opa($in) {
  docker run --rm --mount "type=bind,source=$root,target=/work,readonly" $OPA `
    eval --fail-defined -d /work/policies/compose.rego -i "/work/$in" 'data.chatapp.compose.deny[_]'
  "$in exit=$LASTEXITCODE"
}
# Compose thật -> exit 0
Render 'compose.yaml' 'security-reports/compose.rendered.json' @('--env-file','lab.env.example'); Opa 'security-reports/compose.rendered.json'
# Fixture tổng hợp -> bad exit 1 (5 deny), good exit 0
Opa 'fixtures/compose.bad.json'; Opa 'fixtures/compose.good.json'
# SEC-09, SEC-10 -> vulnerable exit 1, fixed exit 0
foreach ($c in 'SEC-09','SEC-10') { foreach ($m in 'vulnerable','fixed') {
  Render "training/security/$c/$m/compose.yaml" "security-reports/$c-$m.json" @(); Opa "security-reports/$c-$m.json" } }
```

Kết quả kỳ vọng (đã khớp với lần chạy logic của mình):

- SEC-09 vulnerable: `worker: privileged containers are forbidden`.
- SEC-10 vulnerable: `db: do not publish PostgreSQL ports to the host` và `db: published ports must bind to 127.0.0.1`.
- Các bản fixed và Compose thật: không có deny.

Chứng minh runtime: `docker inspect` ở Bước 4 (đang chạy, không privileged, cap\_drop ALL). Đưa bài học phần 1a vào đây: policy yêu cầu `cap_drop: ALL` cho mọi service, nhưng image Postgres chạy root cần quyền `CHOWN/SETUID`. Giải pháp là chạy bằng user không phải root, không phải nới lỏng policy.

**Ảnh SEC-09/10:** output 4 lần OPA của fixture, output Compose thật.

### Bước 11 – Cả nhóm: regression 15 fixture và quét image bằng Trivy

**11a. Regression 10 SEC + 5 CQ:**

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File .\scripts\run-lab.ps1 -Action demo
```

Kỳ vọng: dòng cuối `Overall: 0 unexpected results`, có `security-reports/training-validation.json`. Giải thích trong báo cáo rằng PASS nghĩa là "bộ kiểm thử hoạt động đúng", không phải "code an toàn".

**11b. Trivy:**

```powershell
docker volume create trivy-cache | Out-Null
foreach ($img in 'backend','frontend') {
  docker save -o "security-reports\$img.tar" "chatapp-$img`:lab"
  docker run --rm -v trivy-cache:/root/.cache --mount "type=bind,source=$root,target=/work" $TRIVY `
    image --input "/work/security-reports/$img.tar" --scanners vuln --severity HIGH,CRITICAL --ignore-unfixed --format table
  docker run --rm -v trivy-cache:/root/.cache --mount "type=bind,source=$root,target=/work" $TRIVY `
    image --input "/work/security-reports/$img.tar" --scanners vuln --severity HIGH,CRITICAL --ignore-unfixed `
    --exit-code 1 --format json --output "/work/security-reports/trivy-$img.json"
  "$img exit=$LASTEXITCODE"
  Remove-Item "security-reports\$img.tar"
}
```

Với mỗi CVE, ghi vào bảng: image, package, installed, fixed, quyết định (nâng base image, nâng package, hoặc chấp nhận có hạn). Sau đó chạy `docker compose ... build --pull` và quét lại. Ghi số CVE trước/sau. Không tắt gate để có ảnh xanh.

**Ảnh LAB-04:** bảng Trivy của 2 image (trước/sau) và exit code.

### Bước 12 – SAST với SonarQube: source thật và 5 CQ

**12a. Source thật:** chạy workflow `SonarQube Scan` (Bước 13), hoặc chạy local theo đúng các lệnh trong `sonar-scan.yml`. Chụp Overview, Issues, Security Hotspots của `chatapp-backend` và `chatapp-frontend`. Ghi số Bugs, Vulnerabilities, Code Smells, Hotspots và trạng thái Quality Gate. Đây mới là số liệu SAST thật của bài.

**12b. 5 CQ trong project sandbox ngoài repo.** CQ-02..05 hiện chỉ là method trần, không có class, nên **không biên dịch được**. Phải bọc mỗi file trong `public class CQ0x { ... }`:

```powershell
$sb = 'C:\DevSecOps\cq-sandbox'; New-Item -ItemType Directory -Force $sb | Out-Null; Set-Location $sb
dotnet new classlib -n CQSandbox -f net9.0 --force; Remove-Item .\CQSandbox\Class1.cs
foreach ($n in 1..5) {
  $f = Get-ChildItem "$repo\training\code-quality\CQ-0$n\vulnerable\*.cs"
  $body = Get-Content $f -Raw
  if ($body -notmatch 'class ') { $body = "public class CQ0$n {`n$body`n}" }
  Set-Content ".\CQSandbox\CQ0$n.cs" $body
}
dotnet build .\CQSandbox          # phải build được trước khi scan
$env:SONAR_TOKEN = Read-Host 'Sonar token'
dotnet tool install dotnet-sonarscanner --tool-path .sonar
.\.sonar\dotnet-sonarscanner begin /k:"chatapp-cq-sandbox" /d:sonar.host.url="http://localhost:9000" /d:sonar.token="$env:SONAR_TOKEN"
dotnet build .\CQSandbox
.\.sonar\dotnet-sonarscanner end /d:sonar.token="$env:SONAR_TOKEN"
```

Sau đó thay bằng bản `fixed` và scan lại. Issue phải chuyển sang Closed. Chỉ ghi rule ID mà Sonar thực sự hiển thị.

Lưu ý: với profile "Sonar way" mặc định, CQ-04 (magic number) và CQ-05 (2 dòng lặp, dưới ngưỡng duplication) nhiều khả năng **không** bị bắt. Nếu vậy, hoặc ghi trung thực "guard của nhóm phát hiện, Sonar way không phát hiện" (cũng là một kết quả đáng bàn), hoặc đổi ví dụ sang mẫu Sonar chắc chắn bắt: khối catch rỗng, biến cục bộ không dùng, method private không dùng, if lồng có thể gộp.

**Ảnh CQ-01..05:** issue trên SonarQube (hoặc output guard) trước/sau, kèm rule ID.

### Bước 13 – Pipeline GitHub Actions và chứng minh gate chặn lỗi

1. Tạo repo **private**, rồi: `git remote add origin <url>; git push -u origin main --tags`.
2. Settings → Actions → Runners: runner `Idle`, có label `self-hosted, Windows, X64`. Tài khoản chạy runner phải dùng được Docker Desktop và có `pwsh`.
3. Settings → Secrets: `SONAR_TOKEN`, `SONAR_HOST_URL` = `http://localhost:9000`.
4. Actions → `DevSecOps Local Security Gates` → Run workflow. Kỳ vọng: Gitleaks, Checkov, OPA, build xanh. Trivy xanh hoặc đỏ có lý do đã ghi ở Bước 11. Job `training-regressions` xanh.
5. **Chứng minh gate chặn lỗi:**

```powershell
git switch -c feature/demo-gate-fail
# thêm  privileged: true  vào service frontend trong compose.yaml
git commit -am "demo: vi pham policy privileged"; git push -u origin feature/demo-gate-fail
```

Step OPA phải đỏ với `frontend: privileged containers are forbidden`. Chụp xong thì xóa nhánh.

6. Tải artifact `devsecops-scan-results` và `training-15-case-regression` để đưa vào phụ lục.

**Ảnh LAB-05:** danh sách run (xanh và đỏ), chi tiết step bị chặn, SonarQube Quality Gate.

### Bước 14 – Ghi nhật ký bằng chứng và dọn dẹp

Mỗi lần chạy, ghi một dòng vào bảng nhật ký trong phụ lục:

| Mã | Thời gian | Commit SHA | Tool + digest | Lệnh | Exit | Kết quả | Ảnh |
| --- | --- | --- | --- | --- | --- | --- | --- |
| SEC-01 | 2026-11-\_\_ **:** | `git rev-parse --short HEAD` | gitleaks@sha256:… | gitleaks git | 1 | 2 finding, đã redact | H-06 |

Dọn dẹp:

- `docker compose --env-file $envf down`: giữ dữ liệu. Chỉ dùng `down -v` khi cố ý xóa dữ liệu demo.
- Xóa biến môi trường chứa token và xóa profile trình duyệt test.
- Sau khi có điểm, xóa `chatapp-lab.env`.
- Kiểm tra thư mục `evidence/` và file Word không chứa token, mật khẩu, email thật.
