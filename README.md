# Renshuu

Không gian luyện trắc nghiệm bằng ASP.NET Core 8 Razor Pages, EF Core 8 và SQL Server. Giao diện tiếng Việt, hỗ trợ nội dung tiếng Nhật.

## Chạy trên Windows

Cần .NET SDK 8 trở lên, ASP.NET Core runtime 8 và SQL Server có Windows Authentication. Tài khoản chạy ứng dụng cần quyền tạo database khi khởi tạo lần đầu.

```powershell
.\scripts\start-local.ps1
```

Mở [http://localhost:5186](http://localhost:5186). Script dùng SQL Server `localhost`, database riêng `RenshuuUiDev`. Trong môi trường Development, ứng dụng tự chạy migration và thêm dữ liệu mẫu khi có migration chờ áp dụng. Không sửa connection string của repository.

Với SQL Express hoặc LocalDB:

```powershell
.\scripts\start-local.ps1 -Server '.\SQLEXPRESS' -Database Renshuu
.\scripts\start-local.ps1 -Server '(localdb)\MSSQLLocalDB' -Database Renshuu
```

Hoặc tự cấu hình bằng biến môi trường:

```powershell
$env:ConnectionStrings__DefaultConnection = 'Server=localhost;Database=Renshuu;Trusted_Connection=True;TrustServerCertificate=True'
$env:ASPNETCORE_ENVIRONMENT = 'Development'
dotnet run --no-launch-profile --urls http://localhost:5186
```

## Luồng sử dụng

1. Chọn môn trong form hoặc mở thư mục để chọn môn học.
2. Chọn số câu và thời gian; để trống thời gian để luyện không giới hạn.
3. Trả lời các câu hỏi. Đợi trạng thái **Đã lưu trên máy chủ** trước khi rời trang; vào **Bài chưa nộp** trên tổng quan để tiếp tục. Các nút số câu cho biết câu đã trả lời và cho phép chuyển nhanh.
4. Nộp bài để xem tỷ lệ đúng, đáp án và giải thích.
5. Vào **Ôn lại câu sai** để làm lại; trả lời đúng sẽ gỡ câu khỏi danh sách.
6. Vào **Quản lý nội dung** để quản lý thư mục, môn học, câu hỏi hoặc nhập CSV/JSON/văn bản/bài đọc.

## Giao diện mới

Bản nghiên cứu minimalism đưa form học lên đầu, thu nhỏ 3D thành preview hỗ trợ và chuyển lịch sử sang danh sách responsive. Phân tích, nguồn tham khảo và lý do: [docs/NGHIEN_CUU_MINIMALISM.md](docs/NGHIEN_CUU_MINIMALISM.md).

Bản 11/11/2026 chuyển sang trắng–đen tối giản: chữ ngắn hơn, hướng dẫn dài mở khi cần, nút có phản hồi mềm và thanh chế độ trượt nhẹ. Số liệu hiện ngay; chỉ phần xem trước 3D chuyển động theo cuộn. Cấu trúc và luồng nghiệp vụ được giữ nguyên.

- Sidebar và menu di động có điều khiển bàn phím, Escape và quản lý focus.
- Dashboard dùng số liệu thật từ database, tỷ lệ đúng tính trên toàn bộ câu của các bài đã nộp; lịch sử gần đây hiển thị ba bài.
- Thẻ ôn tập CSS 3D: không cần WebGL, thư viện 3D hay tải mô hình ngoài. Có nút dừng, lưu tùy chọn và hỗ trợ `prefers-reduced-motion`; dừng khi tab ẩn hoặc hình ra khỏi màn hình.
- Tìm môn/thư mục không phân biệt dấu tiếng Việt; tìm tên môn trong thư mục sẽ hiện thư mục tương ứng.
- Dùng chung màu, form, bảng và kiểu chữ trên các trang học tập và quản trị.
- Sửa lỗi nộp toàn bộ đáp án trống và giữ bản nháp trong trình duyệt cho đến khi tải trang kết quả thành công.

### Scroll Craft

Trang tổng quan đã tích hợp engine Scroll Craft nguyên bản ở `wwwroot/assets/scrollcraft/`, kèm giấy phép MIT. Hiệu ứng riêng của Renshuu nằm trong `wwwroot/css/learning-experience.css` và `wwwroot/js/learning-experience.js`.

Phần xem trước 3D đồng bộ với môn, số câu và thời gian trong form. Chế độ ôn câu sai dùng số liệu thật và mở đúng môn. Các lớp 3D thay đổi theo cuộn; phần nhập liệu luôn ở luồng trang bình thường. Motion-off, reduced-motion và menu di động khi không có JavaScript đều có bố cục sử dụng được.

Quy trình và kết quả kiểm tra: [docs/SCROLL_CRAFT.md](docs/SCROLL_CRAFT.md).

## Kiểm tra

Nâng cấp nghiệp vụ giữ nguyên Razor Pages → Service → EF Core: tự lưu/tiếp tục bài, kiểm soát deadline trên máy chủ, chấm một lần khi có request đồng thời, ôn sai theo lần nộp mới nhất và xem trước nhập liệu có bộ lọc. Chi tiết và migration: [docs/NANG_CAP_NGHIEP_VU.md](docs/NANG_CAP_NGHIEP_VU.md).

```powershell
dotnet build --nologo
node --check wwwroot/js/workspace.js
node --check wwwroot/js/dashboard.js
```

Kiểm thử luồng HTTP phải dùng database riêng và chưa có bài làm để không ảnh hưởng danh sách câu sai:

```powershell
# Terminal 1
.\scripts\start-local.ps1 -Database RenshuuSmokeTest -Port 5187
# Terminal 2 — Python 3, chỉ dùng thư viện chuẩn
python scripts/smoke-test.py http://localhost:5187 --allow-test-writes
```

Kiểm thử tạo hai bài làm, kiểm tra nộp trống, chấm điểm, luyện lại câu sai, lịch sử và gửi lại bài đã nộp; xóa các bài kiểm thử sau khi hoàn thành. Chỉ chạy trên loopback và database kiểm thử.

Phân tích kiến trúc, các giới hạn vận hành và hướng mở rộng: [docs/PHAN_TICH_LUONG.md](docs/PHAN_TICH_LUONG.md). Kết quả kiểm tra giao diện: [docs/KIEM_TRA_GIAO_DIEN.md](docs/KIEM_TRA_GIAO_DIEN.md).
