# Nhật ký cập nhật database theo task

## 2026-11-13 — Phân loại câu hỏi thủ công

- Migration: `20261113055114_AddQuestionClassification`.
- Thêm `Questions.Chapter`, `Questions.Topic`, `Questions.Difficulty`, đều nullable để giữ tương thích dữ liệu cũ.
- SQL: `update-database.sql`; xác minh: `verify-database.sql`.
- Gỡ AI: không cần SQL/migration bổ sung; không có bảng/API key AI được lưu trong database.

## 2026-11-13 — Góc học tập

- Migration: `20261113110547_AddLearnerFeatures`.
- Thêm `LearnerProfiles` (tên hiển thị, mục tiêu ngày) và `SavedQuestions` (yêu thích, ghi chú riêng).
- Khóa ngoại tới Identity/câu hỏi; unique index trên `(UserId, QuestionId)`.
- Lưu bài đang làm dùng cột `AttemptAnswers.SelectedOption` đã có. Đổi mật khẩu dùng Identity hiện có.
- SQL: `update-database.sql`; xác minh: `verify-database.sql`.
- Down xóa bảng mới và dữ liệu trong bảng; không dùng Down để cập nhật bình thường.

## 2026-11-13 — Áp dụng lên database chính trên máy Sơn

- Đích đã xác nhận: database `TracNghiemWeb` trên SQL Server local của máy Sơn. Thành viên khác phải tự xác định đích trên máy mình.
- Tạo backup COPY_ONLY với CHECKSUM và RESTORE VERIFYONLY thành công trước khi cập nhật.
- Đã áp dụng hai migrations ở trên; tổng 6 migrations.
- Sau cập nhật: vẫn 20 câu hỏi, 1 tài khoản, 1 bài làm; có đủ ba cột phân loại và hai bảng mới.
- Task này chỉ triển khai các migrations có sẵn, không phát sinh migration mới.

## 2026-11-13 — Đưa nhánh tính năng người học lên GitHub

- Không cần SQL/migration mới cho thao tác commit/push.
- Nhánh mang theo `update-database.sql`, `verify-database.sql` và hướng dẫn trong README; thành viên khác chạy theo cấu hình database của máy mình.

## Mẫu bắt buộc cho task tiếp theo

- Tên task và ngày:
- Có thay đổi database: Có/Không (nếu không: Không cần SQL/migration).
- Migration và bảng/cột/index thay đổi:
- File SQL và cách chạy:
- Cách kiểm tra sau cập nhật:
- Đích đã thực sự chạy và kết quả, hoặc ghi rõ chưa chạy:
- Ảnh hưởng dữ liệu/rollback nếu có:
