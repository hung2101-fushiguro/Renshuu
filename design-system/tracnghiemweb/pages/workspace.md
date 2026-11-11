# Renshuu — minimalism theo tác vụ

Override thị giác cho MASTER.md, cập nhật 11/11/2026. Tham khảo nguồn và phân tích tại docs/NGHIEN_CUU_MINIMALISM.md.

- Loại: ứng dụng học tập Razor Pages. Ưu tiên bắt đầu/tiếp tục bài, đọc kết quả và quản lý nội dung. Không dùng quy tắc marketing mỗi khối một màn hình.
- Nội dung #fff; sidebar #fafafa; chữ chính #181818, chữ phụ #6b6b6b; viền #e7e7e7.
- Be Vietnam Pro 400/600 và fallback tiếng Nhật/hệ thống. Không thêm font.
- Sidebar desktop 232px, drawer dưới 761px. Nội dung có khoảng đệm 20–40px; phân nhóm theo nhịp 8/12/16/24/32/48px.
- Form tạo bài đứng đầu, preview nhỏ bên cạnh. Mobile đưa form trước preview. Chọn môn một nơi và đồng bộ dữ liệu thật.
- Hành động chính là nút đen; các thao tác xem, điều chỉnh và xóa dùng nút nhẹ, giữ nhãn/focus/xác nhận xóa.
- Viền dành cho form/field; thư viện, thống kê, lịch sử và câu hỏi dùng hàng/khoảng cách, tránh hộp lồng hộp. Lịch sử là danh sách responsive.
- Góc nút 8px, form 12px, preview 10px. Shadow nhẹ chỉ ở thẻ xem trước.
- Copy ngắn, tên gọi rõ; hướng dẫn dài nằm trong details/summary. Giữ thông tin cần quyết định và lỗi validation.
- Ba lớp preview tạo chiều sâu bằng lớp, shadow và xoay Z nhẹ. Không nghiêng X/Y, không chuyển động theo con trỏ, không đếm số hay reveal form theo cuộn.
- Hover 180ms, trả về 280ms, bấm 120ms; thanh chế độ 280ms, drawer 320ms; cubic-bezier(.22,1,.36,1), không overshoot. Bỏ animation khi tải trang.
- Giữ nguyên bộ motion và Scroll Craft vendor. Chuyển động tùy chọn; reduced motion/pause giữ đủ nội dung.
- Điều khiển chạm 44px, input mobile 16px, focus rõ và nhãn trạng thái bằng chữ. Giữ nguyên URL, field, antiforgery, PageModel, Service, EF và nghiệp vụ.
- Không mô phỏng thành tích, streak hoặc dữ liệu học tập. Số liệu từ Model thật.
