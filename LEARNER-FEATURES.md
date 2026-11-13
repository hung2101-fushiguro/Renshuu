# Góc học tập

Sau khi đăng nhập, chọn Góc học tập trên thanh điều hướng:

- Đổi tên hiển thị và mục tiêu từ 1–500 câu/ngày.
- Đổi mật khẩu bằng mật khẩu hiện tại, mật khẩu mới và xác nhận. Sử dụng ASP.NET Core Identity.
- Xem danh sách bài chưa nộp và tiếp tục làm. Đáp án tự lưu lên server; trạng thái lưu hiển thị dưới bài. Cần chờ “Đã lưu trên máy chủ” trước khi rời trang.
- Trong Ôn tập hoặc Kết quả, chọn “Lưu câu / ghi chú”. Yêu thích và ghi chú thuộc riêng tài khoản. Bỏ yêu thích và xóa ghi chú để gỡ khỏi danh sách.
- Mục tiêu ngày đếm số câu khác nhau đã trả lời trong bài đã nộp theo ngày Việt Nam, gồm ôn và làm bài. Xóa lịch sử sẽ thay đổi số đếm.

Danh sách hiển thị tối đa 200 câu đã lưu phù hợp bộ lọc và 100 bài đang làm gần nhất. Ghi chú tối đa 2000 ký tự. Thời gian làm bài tiếp tục chạy khi rời trang; quá hạn, server chấm những đáp án đã lưu (có khoảng đệm 5 giây cho thao tác tự nộp).

Migration `AddLearnerFeatures` thêm LearnerProfiles và SavedQuestions, không xóa dữ liệu cũ. Dùng script `database/update-database.sql` hoặc Development startup trên kết nối database đã xác nhận. Down xóa hai bảng mới cùng dữ liệu của chúng.

Đã build Release thành công. Script Playwright local `output/auth-ui/learner-check.cjs` đã kiểm tra hồ sơ/mục tiêu, đổi mật khẩu, lưu/khôi phục bài, yêu thích/ghi chú, tách dữ liệu hai tài khoản và tiến độ ngày trên database riêng `TracNghiemWeb_LearnerCheck_20261113`. Ảnh mobile ở `output/auth-ui/learner-mobile.png`. Repository chưa có test project .NET; output không được đưa lên Git.
