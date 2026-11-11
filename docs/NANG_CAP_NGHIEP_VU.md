# Cải thiện nghiệp vụ và trải nghiệm học

## Phạm vi và cấu trúc

Tiếp tục dùng ASP.NET Core Razor Pages, QuizService/ImportService và AppDbContext/SQL Server. Không đổi luồng chọn môn → tạo Attempt → làm bài → nộp → kết quả → ôn sai. Không thêm frontend framework, dịch vụ ngoài hoặc package runtime mới.

| Vấn đề quan sát | Đã cải thiện | Nơi xử lý |
| --- | --- | --- |
| Đáp án chỉ nằm trong tab; khó tiếp tục bài | Tự lưu lên SQL sau 600 ms, trạng thái lưu, giữ bản tạm khi lỗi mạng; mở lại từ phiên trình duyệt mới | Take PageModel → QuizService.SaveDraftAsync; quiz.js |
| Không thấy bài đang làm | Tổng quan hiện tối đa năm bài chưa nộp gần nhất, số câu đã lưu và link tiếp tục | Index → GetPendingAttemptsAsync |
| Mở hai bài rồi nộp ngược thứ tự cho danh sách sai không đúng mong đợi | Lấy SubmittedAt mới nhất cho từng câu; phân định bằng Id câu trả lời khi trùng thời gian; lịch sử cũng theo lần nộp | QuizService.LatestAnswers/GetHistoryAsync |
| Đồng hồ chỉ ở trình duyệt | Server kiểm tra deadline; quá hạn chỉ chấm bản lưu, có 5 giây đệm truyền request nộp | QuizService và TakeModel |
| Hai request nộp/lưu có thể tranh chấp kết quả | UPDATE có điều kiện và transaction trên cùng Attempt; nộp lại giữ nguyên kết quả; lưu sau khi nộp trả 409 và trang cũ chuyển sang chỉ đọc với link kết quả | QuizService |
| Nhóm bài đọc quá lớn có thể bị cắt | Giữ nguyên nhóm, bỏ qua nhóm không vừa giới hạn 200, báo lỗi nếu không có nhóm phù hợp | QuizService.StartAsync |
| Ôn sai có thể tạo bài quá lớn | Giới hạn 200 câu/lượt, còn lại tiếp tục trong danh sách | QuizService.StartWrongAsync và Wrong UI |
| Xem trước chưa nhận trùng trong file hoặc môn mới | Dùng cùng quy tắc nội dung/môn như khi lưu, đánh dấu từ dòng trùng thứ hai | ImportService.FindExistingDuplicatesAsync |
| Khó tìm các dòng lỗi trong bản nhập | Bộ lọc tất cả/hợp lệ/trùng/lỗi, số dòng hiển thị, trạng thái rỗng; bảng cuộn ngang bằng bàn phím/điện thoại | _ImportPreview + import-preview.js |

Bộ lọc nhập liệu chỉ đổi cách hiển thị; Save vẫn xử lý toàn bộ bản xem trước như trước. Tắt JavaScript vẫn xem tất cả dòng, nộp bài và khôi phục lựa chọn đã lưu trên SQL; tự lưu lựa chọn mới cần JavaScript.

## Quy tắc bản nháp và thời gian

- Dùng AttemptAnswer.SelectedOption có sẵn để lưu bản nháp. Không chấm IsCorrect trước khi nộp, không đưa bài nháp vào thống kê/câu sai.
- Mỗi request gửi toàn bộ lựa chọn hiện tại, chỉ một request lưu đang chạy trên mỗi trang. Cache đã đồng bộ không thay dữ liệu mới hơn trên máy chủ. Nếu mở cùng bài ở nhiều tab, lần lưu được chấp nhận sau cùng thắng; chưa có hòa giải chỉnh sửa nhiều thiết bị.
- Deadline = StartedAt + TimeLimitMinutes. Đáp án mới không được lưu từ deadline trở đi. Nộp sau deadline + 5 giây chỉ chấm đáp án đã lưu; request trong khoảng đệm được chấm payload cuối cùng.
- Đồng hồ dùng số giây còn lại tính bởi máy chủ, không dựa vào đồng hồ lịch của thiết bị người học. TimeProvider cho phép kiểm thử thời gian mà không phải đợi thật.
- Bài hết giờ khi đã đóng tab được chấm lúc mở lại hoặc nộp, chưa có scheduler tự chấm mọi bài nền.
- Giữ cách lưu giờ địa phương tương thích database gốc; không chuyển lịch sử sang UTC trong đợt này.

## Migration

`Data/Migrations/20261110092823_SaveAttemptDrafts.cs` chỉ thêm cột nullable Attempt.DraftSavedAt và index (SubmittedAt, Id). Không xóa/đổi dữ liệu hiện có. Trong Development, start-local.ps1 tự áp dụng theo cơ chế có sẵn. Production cần áp dụng migration bằng cấu hình database triển khai:

```powershell
dotnet ef database update
```

## Kiểm thử

Build thành công với 0 lỗi, 0 cảnh báo. Các bộ sau được chạy trên SQL Server localhost; mọi request ghi thử chạy ở database riêng, không dùng database luyện tập chính.

```powershell
# 7 nhóm tích hợp SQL: tự tạo database RenshuuRegression_<guid>, xóa sau khi chạy
dotnet run --project tests/Renshuu.IntegrationTests --no-launch-profile
# Nếu SQL Express: thêm -- '.\SQLEXPRESS' ở cuối lệnh

# Server kiểm thử riêng
.\scripts\start-local.ps1 -Database RenshuuSmokeTest -Port 5187

# 10 nhóm luồng HTTP, cần database kiểm thử có seed và chưa có bài làm
python scripts/smoke-test.py http://localhost:5187 --allow-test-writes

# Cần playwright-core và Chrome; NODE_PATH trỏ nơi cài playwright-core nếu không ở project
node scripts/verify-study-flow.cjs http://localhost:5187 --allow-test-writes
node scripts/verify-experience.cjs http://localhost:5187
```

- SQL: migration database mới, thứ tự nộp/timestamp trùng, bản nháp độc lập DbContext, deadline và khoảng đệm, năm lượt race lưu/nộp và nộp trùng, biên nhóm đọc 200/201, giới hạn ôn sai và xem trước trùng khớp khi lưu.
- Trình duyệt nghiệp vụ: tám nhóm gồm lưu/tiếp tục bài, phiên mới không storage, sửa nhanh, lỗi mạng/phục hồi, không JavaScript, mobile, antiforgery, đóng draft sau nộp ở tab cũ và upload JSON thật (kể cả toàn dòng lỗi, không cho lưu). Chỉ xem trước JSON, không ghi câu hỏi mẫu vào database.
- HTTP: mười nhóm hồi quy cho tạo bài, nộp trống, chấm, ôn sai, lịch sử và nộp lại.
- Scroll Craft: mười một nhóm hồi quy cho đồng bộ form, cuộn, pause, responsive, menu, quay lại trang, reduced motion và không JavaScript.

Ảnh kiểm tra cục bộ nằm trong artifacts/study-flow (đã gitignore). Chưa kiểm thử tải, mọi định dạng nhập liệu hoặc triển khai nhiều instance.

## Các tính năng nên làm tiếp

1. **Tài khoản và quyền quản trị:** cần thực hiện trước khi dùng nhiều người. Hiện tất cả dữ liệu, kể cả bài nháp, vẫn dùng chung đúng mô hình gốc.
2. **Snapshot câu hỏi cho từng bài:** bảo đảm kết quả cũ giữ nguyên nội dung/đáp án khi quản trị sửa ngân hàng câu hỏi.
3. **Ôn tập giãn cách và đánh dấu câu cần học:** hợp lý sau khi có UserId để lưu lịch ôn riêng từng người.

Chưa thêm các tính năng này vì cần thống nhất mô hình người dùng và phiên bản câu hỏi, tránh ghép nghiệp vụ mới vào luồng dùng chung hiện tại.
