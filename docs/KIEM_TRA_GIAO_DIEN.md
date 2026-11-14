# Kết quả kiểm tra bản nâng cấp

Ngày thực hiện: 10/11/2026. Mã gốc: `d1635b1`. Kiểm tra local trên Windows, .NET SDK 9.0.305 với ASP.NET Core 8, SQL Server và Chrome headless.

## Chức năng và build

- `dotnet build --nologo`: thành công, 0 warning, 0 error.
- `node --check` cho `workspace.js` và `dashboard.js`: thành công.
- Smoke test HTTP chạy trên database riêng `RenshuuSmokeTest`: 10 nhóm kiểm tra thành công; hai bài kiểm thử được xóa sau mỗi lần chạy.
- Đã kiểm tra dashboard; chín trang học tập/quản trị; bảy tài nguyên CSS/JS/SVG; validation số câu; tạo bài; timer và navigation; nộp trống; luyện lại đúng; cập nhật câu sai, lịch sử và dashboard; gửi lại bài đã nộp giữ nguyên điểm.
- Smoke test không tự động kiểm tra hết thao tác CRUD, mọi định dạng nhập câu hỏi, menu bằng bàn phím hay tình huống đếm giờ thực tế. Không có kiểm thử tải/nhiều người dùng.

## Giao diện

Hướng thiết kế: ứng dụng học tập, nền sáng và cobalt, Be Vietnam Pro với fallback tiếng Nhật trên hệ thống, sidebar, thẻ nội dung và CSS 3D nhiều lớp. Không có ảnh mẫu, giá, địa chỉ hoặc số liệu thành tích giả. Logo chữ dùng đúng tên repository Renshuu. Không cần bổ sung ảnh từ người dùng.

Đã chụp và xem trên 1440×900, 1024×768, 820×1180, 768×1024 và 390×844. Bộ kiểm tra layout ghi nhận `overflowX: false` trên cả năm kích thước. Có ảnh reduced motion riêng cho điện thoại. Các khối câu hỏi và bảng quản trị dùng chung hệ màu/typography.

Ảnh và kết quả máy đọc nằm trong `artifacts/` (không theo dõi bởi Git); ảnh trước nâng cấp là `before-desktop.png`, ảnh bàn giao là `final-1440.png` và `final-390.png`.

## Performance

Kết quả median của ba lượt Chrome headless qua HTTP local, trên bản cuối:

| Chỉ số | Desktop 1440 | Điện thoại 390 |
| --- | ---: | ---: |
| Tổng transfer | 226.6 KiB | 225.4 KiB |
| Request | 17 | 16 |
| JavaScript parsed | 19.9 KiB | 19.9 KiB |
| Font | 91.5 KiB / 6 file | 91.5 KiB / 6 file |
| First Contentful Paint | 376 ms | 304 ms |
| Largest Contentful Paint | 376 ms | 304 ms |
| Cumulative Layout Shift | 0.000 | 0.000 |
| WebGL contexts | 0 | 0 |
| Broken requests | 0 | 0 |

Bộ kiểm tra performance không có FAIL; còn WARN về sáu file font (hai weight, các subset Latin/Vietnamese). Đã bỏ tải font Nhật ngoài và giảm weight font để giảm tổng transfer từ khoảng 450 KiB xuống 227 KiB. Thời gian trên là đo ở máy local, không phải cam kết tốc độ khi triển khai qua mạng thật.

## Phạm vi của checker thiết kế

Đã chạy `consistency.mjs`, `layout/check.mjs` và `perf.mjs` từ design-taste-frontend. Không tuyên bố bộ consistency/layout đạt toàn bộ: bộ này được thiết kế cho trang marketing một section mỗi màn hình, còn Renshuu là ứng dụng có sidebar, form và nhiều vùng đồng thời.

Lượt consistency đầu có 178 FAIL, 68 WARN, bao gồm lặp lại cùng quy tắc ở nhiều kích thước; ba lượt sweep phát sinh lỗi kết nối trong lúc restart server. Những phát hiện thực tế về tương phản chữ phụ, cấp heading, action form và nút dừng tại hình đã được chỉnh. Những nhóm sau được ghi nhận là không áp dụng hoặc nhận diện sai với ứng dụng này:

| Nhóm phát hiện | Đánh giá |
| --- | --- |
| hero-fill, hero-screen, section-height, section-air, one-section, one-heading; layout screen-short/screen-height/hero-fold | App cần form, thống kê, thư viện và lịch sử ở gần nhau; ép từng khối thành một màn hình làm giảm khả năng sử dụng. |
| container-edges, row-lines, edge-left, flush | Checker lấy toàn viewport và mép ngoài card làm chuẩn; app có sidebar cố định, card có đệm và dashboard hai cột. Đệm trong thẻ và nhiều đường căn theo cột là chủ ý. |
| spill, stagger | Tọa độ các lớp đã chiếu phối cảnh và hai quỹ đạo bị hiểu là hộp UI tràn. Đây là lớp trang trí nằm trong hero có overflow hidden; trang không tràn ngang trên năm kích thước đã đo. |
| no-visual | Không nhận diện DOM/CSS 3D và form thực là hình/product UI. Trang đã có thẻ 3D; form/thư viện không cần thêm hình trang trí. |
| cta | Link toàn bộ banner ôn lại là hành động hợp lệ; không thêm CTA marketing ở footer ứng dụng. |
| loop-pause | Lượt đầu chưa nhận ra nút pause toàn cục ngoài vùng hình. Bản cuối thêm nút dừng trực tiếp tại hero, đồng bộ với nút toàn cục. |
| heading-scale | Tagline hero đã đổi thành đoạn chữ trình bày, giữ h1 cho tiêu đề trang, h2 cho các khu vực. H3 nhỏ hơn tiêu đề khu vực. |
| text-contrast | Chữ phụ đã được làm đậm màu hơn; đoạn giới thiệu dùng màu #45536a/#405474. Các dòng metadata không bắt buộc tiêu chuẩn 7:1 dành cho body text của checker. |
| forms | Đây là form POST Razor có antiforgery, xử lý tại trang hiện tại. Bản cuối khai báo asp-page rõ ràng; smoke test đã xác minh POST thực sự tạo bài. |

Các lượt kiểm tra bổ sung được ghi tại [SCROLL_CRAFT.md](SCROLL_CRAFT.md): `scripts/verify-experience.cjs` kiểm tra tự động reduced motion, pause, menu/Escape/focus và các trạng thái trình duyệt. Phần lưu nháp/tiếp tục bài và bộ lọc nhập liệu được kiểm tra bằng `scripts/verify-study-flow.cjs`, chi tiết tại [NANG_CAP_NGHIEP_VU.md](NANG_CAP_NGHIEP_VU.md).
