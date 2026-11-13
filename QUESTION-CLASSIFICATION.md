# Phân loại câu hỏi

Admin mở Câu hỏi → Thêm/Sửa, nhập Chương, Chủ đề và chọn độ khó Dễ/Trung bình/Khó rồi lưu. Có thể để trống các trường này với câu chưa phân loại.

Trang danh sách hỗ trợ lọc chương, chủ đề và độ khó kết hợp với môn và từ khóa. Các bộ lọc được giữ khi chuyển trang hoặc xóa câu.

Nếu chưa áp dụng migration `AddQuestionClassification`, cập nhật database theo hướng dẫn trong `database/README.md`. Migration chỉ thêm ba cột nullable; dữ liệu câu hỏi cũ được giữ nguyên. Thay đổi này không tự áp dụng migration lên database.

Chương và chủ đề hiện được nhập tự do; độ khó là đánh giá của người soạn. Luồng import vẫn hoạt động và để trống các trường mới. Chưa bổ sung lựa chọn phân loại ở luồng tạo đề.
