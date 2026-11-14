# Luồng tạo đề và làm bài

## Phạm vi

- ASP.NET Core 8 Razor Pages → `QuizService`/`ImportService` → EF Core 8 → SQL Server.
- Giữ palette xanh `#2563EB`, vàng `#F59E0B`, nền `#EFF6FF` và giao diện workspace hiện tại.
- Không thêm framework giao diện hoặc package chạy ứng dụng.

## Tạo đề

- Chỉ lấy câu thuộc môn đã chọn. Mỗi câu độc lập là một nhóm; các câu cùng `PassageId` là một nhóm bài đọc và giữ thứ tự trong nhóm.
- Trộn các nhóm, sau đó tìm tổ hợp đúng số câu yêu cầu. Nếu không có tổ hợp đúng, chọn tổ hợp gần nhất phía trên; nếu vẫn không thể, chọn phía dưới. Không tách nhóm bài đọc.
- Nhóm hơn 200 câu bị bỏ qua. Nếu không còn nhóm hợp lệ, báo lỗi thay vì tạo đề rỗng.
- Số câu 1–200; thời gian 1–240 phút, để trống khi luyện không giới hạn. Ngân hàng ít câu hơn yêu cầu thì lấy theo số câu thực có.
- Ôn câu sai dùng lần nộp mới nhất cho từng câu, tối đa 200 câu một lượt.

## Làm bài

- Trang làm bài hiện tại có thanh tiến độ, đồng hồ, nút **Nộp bài** và **Lưu & thoát**; không có bảng chuyển câu.
- Lựa chọn dùng cột `AttemptAnswers.SelectedOption` hiện có. Bản nháp chưa được chấm và không đi vào thống kê hoặc danh sách câu sai.
- Tự lưu sau 600 ms không có thay đổi mới; mỗi tab chỉ gửi một lượt lưu tại một thời điểm.
- Bản tạm dùng `sessionStorage` theo tab, chỉ phát lại khi còn thay đổi chưa đồng bộ. Nếu trình duyệt chặn lưu trữ, trang vẫn cho chọn và nộp bài.
- Khi mất mạng, đáp án được giữ trong tab và thử lại khi có mạng hoặc theo chu kỳ 15 giây.
- Trạng thái bài làm được đồng bộ từ server mỗi 30 giây và khi quay lại tab: phát hiện bài đã nộp ở tab khác và cập nhật lại thời gian còn lại.
- Hết giờ khi đang mở trang sẽ tự nộp. Request nộp có 5 giây đệm truyền mạng; sau đó server chỉ chấm các đáp án đã lưu trước hạn.
- **Lưu & thoát** lưu nháp rồi về trang chủ. Nếu bài đã đóng, server chấm bằng các đáp án đã lưu trước khi hết giờ.
- Nộp nhiều lần giữ nguyên kết quả; lưu sau khi nộp bị từ chối.

## Database

- Migration `20261110092823_SaveAttemptDrafts` chỉ thêm cột nullable `DraftSavedAt` và index `(SubmittedAt, Id)` vào bảng `Attempts`.
- Giữ nguyên 7 bảng, quan hệ, ngân hàng câu hỏi và lịch sử làm bài.

## Chạy và kiểm tra

```powershell
# Mặc định: SQL Server localhost, database RenshuuUiDev, web http://localhost:5186
.\scripts\start-local.ps1

# Dùng database khác
.\scripts\start-local.ps1 -Database RenshuuUiTest -Port 5187

# Kiểm thử tích hợp (tạo database tạm rồi xóa)
dotnet run --project tests/Renshuu.IntegrationTests -- "SATORU\SQLEXPRESS"

# Smoke test trên database phát triển đã có dữ liệu mẫu
python scripts/smoke-test.py http://localhost:5186 --allow-test-writes
```

- 8 nhóm kiểm thử SQL: migration, thứ tự nộp, bản nháp, deadline/đệm mạng, tranh chấp lưu/nộp, giới hạn nhóm 200/201, tổ hợp nhóm/ngẫu nhiên, biên đầu vào/bỏ chọn/nộp trống.
- Smoke test kiểm tra dashboard, 8 trang study/admin, asset tĩnh, tạo đề, đồng bộ trạng thái, chấm bài, ôn câu sai và lịch sử.
- Ảnh và báo cáo cục bộ đặt trong `artifacts/`, không đưa vào Git hoặc gói publish.
