# Bộ 15 tình huống demo – chỉ dùng trong lab cô lập

- `security/SEC-01` đến `security/SEC-10`: mỗi mã có `vulnerable/` và `fixed/`.
- `code-quality/CQ-01` đến `CQ-05`: năm lỗi code quality bổ sung.
- MỌI secret xuất hiện trong `training/` chỉ là chuỗi giả minh họa, **không phải token hay mật khẩu thật**.
- Không được copy phiên bản `vulnerable` vào `backend/`, `frontend/` hoặc `compose.yaml` dùng chung.
- SEC-01/02 và SEC-03/04/05 là vấn đề đã quan sát trong mã gốc. SEC-06 đến SEC-10 là **tình huống dựng có chủ đích**, không khẳng định ứng dụng gốc mắc lỗi.
- `scripts/verify-training-cases.py`: kiểm tra mong đợi 10 FAIL/10 PASS theo rule đơn giản (pattern checking).
- `scripts/verify-quality-cases.py`: kiểm tra 5 FAIL/5 PASS, KHÔNG thay thế SonarQube; chụp kết quả SonarQube nếu thực tế phát hiện.
- SEC-05: file `fixed/Program.cs` chỉ minh họa tắt static uploads. Một bản sửa hoàn chỉnh đòi hỏi endpoint tải file có JWT, kiểm tra thành viên phòng, lưu file bên ngoài `wwwroot` và sửa frontend lấy blob; ứng dụng chính chưa khắc phục.
- Script kết luận `PASS` cho bộ training nghĩa là **kiểm thử fixture hoạt động đúng**, không phải sản phẩm đã không còn lỗ hổng.
