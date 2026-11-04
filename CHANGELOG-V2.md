# CHANGELOG – bản v2 (02/11/2026)

Bản v2 sửa các lỗi kỹ thuật tìm thấy khi rà soát bộ lab. Ứng dụng, fixture SEC và chính sách Rego giữ nguyên logic.
Hướng dẫn chạy chuẩn: `HUONG_DAN_STEP_BY_STEP_V3.md` (thay thế `README-LAB-VI.md` / `Huong_dan_lab_DevSecOps.md`).

| # | File | Thay đổi | Lý do |
|---|---|---|---|
| 1 | `compose.yaml` | Thêm `user: "70:70"` cho service `db` | Postgres chạy root + `cap_drop: [ALL]` không `chmod`/`su-exec` được -> db crash, backend không khởi động |
| 2 | `.gitignore` | Bỏ dòng `Migrations/` | Push lên GitHub sẽ mất EF migrations -> clone về chạy lỗi `relation ... does not exist` |
| 3 | `.gitleaks.toml` | Allowlist thêm `node_modules/`, `dist/` | Tránh quét thư viện bên thứ ba sau `npm ci` |
| 4 | `.github/workflows/security-lab.yml` | `--skip-path training` cho Checkov | Fixture SEC-08 cố ý `USER root` làm gate CI luôn đỏ |
| 5 | `security-lab.yml` | `shell: pwsh` thay `powershell` | Windows PowerShell 5.1 + `ErrorActionPreference=stop` có thể biến stderr của docker thành lỗi giả |
| 6 | `security-lab.yml`, `scripts/*.ps1` | Ảnh scanner gom vào biến (`env:` của workflow, `scripts/tool-images.ps1`), cảnh báo khi chưa ghim digest | Không dùng `:latest`; sự cố chuỗi cung ứng Trivy 0.69.4–0.69.6 (03/2026) |
| 7 | Trivy (workflow, `scan-local.ps1`) | `--ignore-unfixed --ignorefile /work/.trivyignore`, cache `trivy-cache` | Gate chỉ chặn CVE đã có bản vá; ngoại lệ phải ghi rõ lý do |
| 8 | `.trivyignore` (mới) | Mẫu ghi CVE chấp nhận tạm thời | Quản lý residual risk |
| 9 | `scripts/scan-local.ps1` | `--skip-path training` cho Checkov | Cùng lý do #4 |
| 10 | `training/code-quality/CQ-02..05` | Bọc method vào `public class CQ0x` | Để biên dịch được trong project sandbox khi quét SonarQube |

## Đã kiểm chứng trên Linux sandbox (02/11/2026)

- `verify-training-cases.py`: 20/20 PASS; `verify-quality-cases.py`: 10/10 PASS (sau khi bọc class).
- `docker compose --env-file lab.env.example config`: OK, `db.user = 70:70`; policy Rego: 0 deny.
- Checkov (CKV_DOCKER_3/7/8, `--skip-path training`): 6 passed, 0 failed. Toàn bộ rule: còn CKV_DOCKER_2 ở `backend/Dockerfile` (SEC-14, để nhóm sửa và chụp trước/sau).
- Frontend `npm ci` + `npm run build`: OK.

## CHƯA kiểm chứng – nhóm phải chạy trên Windows

Build backend .NET, chạy cả stack, Gitleaks, Trivy, OPA binary thật, workflow trên self-hosted runner, SonarQube.

## Cố ý KHÔNG sửa sẵn (để nhóm làm và chụp trước/sau)

SEC-05 (media public), SEC-11 (upload SVG/XSS), SEC-12 (rate limit login), SEC-13 (security header nginx), SEC-14 (HEALTHCHECK backend). Xem Bước 8–9 và mục 5 của hướng dẫn.
