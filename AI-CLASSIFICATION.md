# Phân loại câu hỏi với AI local

Nhánh `codex/tranhongson-ai-question-classification` bổ sung chương, chủ đề và độ khó dự kiến (Dễ/Trung bình/Khó). Các câu cũ và câu import chưa phân loại vẫn hoạt động; có thể bổ sung tại trang Sửa câu hỏi. Bộ lọc ở trang quản trị hỗ trợ cả ba trường.

## Cài model miễn phí phí API

1. Cài [Ollama cho Windows](https://ollama.com/download/windows), mở Ollama.
2. Chạy `ollama pull qwen3:4b` (model khoảng 2,5 GB tải xuống; bộ nhớ khi chạy cao hơn). Máy ít RAM có thể thử `qwen3:1.7b`, nhưng cần kiểm tra chất lượng đề xuất.
3. Nếu dùng model khác, chạy `dotnet user-secrets set "QuestionAi:Model" "qwen3:1.7b"`.
4. Cập nhật database theo migrations, hoặc chạy script trong `database/` theo hướng dẫn ở đó. Development startup cũng áp dụng migration khi chạy ứng dụng với kết nối đã cấu hình đúng.

Mặc định ứng dụng gọi `http://localhost:11434/api/chat`, model `qwen3:4b`; không cần API key. Cấu hình `QuestionAi:BaseUrl` chỉ chấp nhận HTTP loopback. Ollama cần chạy trên cùng máy với backend. Dữ liệu gửi gồm môn, câu hỏi, đáp án, giải thích và đoạn đọc liên quan; không gửi tài khoản người học. Không dùng model có hậu tố cloud.

## Sử dụng

Đăng nhập Admin → Câu hỏi → Thêm/Sửa → nhập câu hỏi và đáp án → AI đề xuất phân loại → đọc lý do → Áp dụng đề xuất → Lưu.

Đề xuất chỉ ở màn hình hiện tại, không tự lưu; chỉ ba trường phân loại được điền khi bấm Áp dụng. Nếu thay đổi nội dung trước khi áp dụng, cần phân tích lại. Lý do AI không được lưu vào database trong bản này. Có thể nhập thủ công hoặc chỉnh sửa bất kỳ đề xuất nào. Khi Ollama chưa chạy, chưa có model hoặc hết thời gian chờ, giao diện báo lỗi và vẫn cho lưu thủ công. Mỗi lần phân tích có thời gian chờ tối đa 3 phút.

Độ khó được ước lượng cho sinh viên đã học nội dung môn, không phải đo từ kết quả thực nghiệm. Cần người soạn kiểm tra, nhất là câu có code, thiếu dữ kiện hoặc nghi đáp án sai. Nên đối chiếu một bộ câu được nhóm tự gán nhãn trước khi dùng rộng.

Phạm vi hiện tại là từng câu và bộ lọc quản trị. Chưa tự phân loại hàng loạt, chưa có danh mục chương/chủ đề chuẩn hóa và chưa lọc đề thi theo độ khó. Các trường là tùy chọn để tương thích luồng nhập dữ liệu hiện có.

## Nguồn

- [Qwen3 4B, dung lượng và giấy phép Apache 2.0](https://ollama.com/library/qwen3:4b).
- [Ollama chat API và JSON schema](https://docs.ollama.com/api/chat).

Không tính phí API khi chạy model local; sử dụng tài nguyên và điện của máy.
