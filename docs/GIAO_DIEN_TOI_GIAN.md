# Giao diện tối giản — 11/11/2026

## Thiết kế

- Trắng/đen/xám; primary #181818, nền #fafafa, bề mặt #fff, chữ phụ #6b6b6b. Đổi màu tại token và stylesheet nguồn, không dùng filter grayscale.
- Giữ Be Vietnam Pro và font fallback tiếng Nhật. Không thêm font, package hay thư viện animation.
- Bỏ tagline lặp lại, avatar trang trí và hộp nhắc học ở sidebar. Rút gọn tiêu đề/phụ đề; hướng dẫn nhập file, văn bản và bài đọc nằm trong details/summary để mở khi cần.
- Nút hover/press ngắn; easing mềm không overshoot; thanh chế độ trượt 320ms; drawer 320ms và nền phủ fade 280ms.
- Giữ scene 3D dữ liệu thật nhưng giảm xoay/parallax, bỏ lưới nền, counter chạy số và entrance theo cuộn của form. Giá trị và nội dung form luôn hiện ngay.
- Điều khiển chạm tối thiểu 44px, input mobile 16px, focus rõ, pause và reduced motion giữ hoạt động. Đáp án dài ở trang câu sai được xuống dòng thay vì tràn khỏi thẻ.

Không thay đổi PageModel, Service, model dữ liệu, migration hoặc quy tắc chấm/lưu nháp trong đợt chỉnh giao diện này.

## Xác minh

- dotnet build: 0 lỗi, 0 cảnh báo; kiểm tra cú pháp JavaScript thành công.
- scripts/smoke-test.py: 10 nhóm HTTP qua, dùng database RenshuuSmokeTest và xóa đúng bài thử sau khi chạy.
- scripts/verify-study-flow.cjs: 8 nhóm trình duyệt qua — tạo/lưu/tiếp tục bài, phiên mới, mạng lỗi, không JavaScript, antiforgery, nộp nhiều lần, JSON preview và tab cũ.
- scripts/verify-experience.cjs: 11 nhóm qua — đồng bộ form, cuộn scene, pause/resume, 1440/1024/820/768/390/375, menu/Escape, lịch sử trình duyệt, reduced motion, không JavaScript. Không lỗi JavaScript hoặc resource.
- layout/check.mjs: không tràn ngang ở 1440/1024/820/768/390. Sáu phát hiện edge-left coi đệm trong hero và cột lịch sử là lệch mép; giữ các mép theo thẻ/cột của ứng dụng.
- perf.mjs: 0 FAIL, 2 WARN do sáu file subset/weight của một font đang dùng. Median FCP/LCP local khoảng 308ms desktop và 280ms mobile; CLS median 0.000. Đây là số đo local, không cam kết tốc độ qua mạng thật.

Ảnh và JSON kiểm tra cục bộ: artifacts/minimal, artifacts/scrollcraft và artifacts/study-flow (gitignore).

## Phạm vi bộ kiểm tra thiết kế

Lượt consistency đầu tại năm kích thước có 150 FAIL/57 WARN, gồm các quy tắc marketing lặp ở nhiều kích thước. Những vấn đề thật về tương phản đoạn phụ ở banner, tỷ lệ h1/h2 và form phụ thuộc IntersectionObserver đã được sửa. Không sửa kit hoặc tắt quy tắc để che lỗi.

Lượt cuối tại 1440/390 có 66 FAIL/26 WARN. text-contrast, contrast, hidden-content và scroll-reveal đã PASS. Không tuyên bố consistency đạt toàn bộ; các quy tắc còn lại cần đọc theo ngữ cảnh bên dưới. Đã tách h3 thẻ môn khỏi h2 khu vực trên mobile.

Các nhóm không áp dụng cho workspace hoặc cần đọc theo ngữ cảnh:

| Nhóm | Lý do giữ bố cục ứng dụng |
| --- | --- |
| hero-fill, hero-screen, section-tile, section-height, section-air, one-screen, one-section, one-heading | Ứng dụng cần form, thống kê và lịch sử gần nhau; không ép mỗi khối thành một màn hình hoặc bỏ đường phân cách chức năng. |
| container-edges, row-lines, edge-left, flush, off-grid, split-pair, stranded-action | Sidebar, đệm trong thẻ và dashboard hai cột tạo nhiều đường căn hợp lệ. CTA nhập liệu ở đầu trang và các cột có vai trò riêng. |
| no-visual, cta | Product UI và form là nội dung chính; không thêm ảnh trang trí hoặc CTA marketing ở footer. |
| scroll-linked, tilt, spill, cut-text | Chỉ các lớp trang trí trong scene 3D biến đổi; scene có overflow clip, không kéo rộng trang. Form không còn hiệu ứng theo cuộn. |
| copy-headings, copy-buttons, copy-actions | Từ điển động từ của checker thiên tiếng Anh; UI dùng tiếng Việt. Hero đưa đến form, nút POST tạo bài, CTA ôn sai mở danh sách — các nhãn phản ánh từng thao tác. |
| link-styles, nav-family, button-styles, hover | Có link inline, hàng kết quả và nút hành động; logo khác cỡ nav. Link bỏ qua nội dung có focus ring; nút mobile chính rộng đầy cột, action phụ nhỏ hơn. |
| heading-scale | H2 của bản xem trước lớn hơn h2 khu vực form; số liệu được trình bày lớn hơn nhãn khu vực. H1 trang vẫn lớn hơn hero, h3 thẻ nhỏ hơn h2 khu vực. |

shoot.mjs ghép ảnh trang Wrong ở 390px có cảnh báo 152px cuối chưa chụp được. Đã đối chiếu bằng fullPage của headless Playwright sẵn có và kiểm tra DOM: footer cuộn đến được đầy đủ, đáp án dài không tràn ngang, không lỗi JS. Bản đối chiếu: artifacts/minimal/wrong-390-direct.png và wrong-390-footer.png. Dải đỏ của ảnh ghép là ký hiệu từ harness, không phải giao diện ứng dụng.

MASTER.md được giữ nguyên; quyết định mới nằm trong design-system/tracnghiemweb/pages/workspace.md.
