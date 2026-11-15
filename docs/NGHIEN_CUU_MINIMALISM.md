# Minimalism cho Renshuu

Nghiên cứu và áp dụng ngày 11/11/2026. Phạm vi: Razor views, stylesheet và JavaScript giao diện; giữ nguyên PageModel, Service, EF Core và quy tắc nghiệp vụ.

## Nguyên tắc tham khảo

- [Nielsen Norman Group — Aesthetic and Minimalist Design](https://www.nngroup.com/articles/aesthetic-minimalist-design/): ưu tiên thông tin phục vụ tác vụ; loại chi tiết không cần và mở dần hướng dẫn ít dùng. Giữ đủ nhãn, trạng thái, phản hồi để người dùng thao tác được.
- [Linear — A calmer interface for a product in motion](https://linear.app/now/behind-the-latest-design-refresh): giảm độ nổi của điều hướng, số biểu tượng và đường phân cách; dùng thứ bậc để nội dung chính được chú ý. Đây là tham khảo về tổ chức, không sao chép component hoặc nhận diện của Linear.
- [Dieter Rams / Vitsœ — Good design](https://www.vitsoe.com/gb/about/good-design): tập trung vào giá trị sử dụng và phần thiết yếu; kiểm soát cả các chi tiết nhỏ của sản phẩm.
- [Apple HIG — Motion](https://developer.apple.com/design/human-interface-guidelines/motion): chuyển động có mục đích, phản hồi ngắn và chính xác, cho phép giảm/tắt chuyển động.

Từ các nguyên tắc đó, hướng phù hợp cho Renshuu là một workspace học tập rõ ràng: trắng–đen tiết chế, nội dung dễ đọc, ít khối trang trí, khoảng cách phân nhóm và hành động chính dễ nhận ra. Sự giản lược được đánh giá bằng khả năng bắt đầu/tiếp tục học, đọc kết quả và quản lý nội dung.

## Đối chiếu và quyết định triển khai

| Quan sát trước đợt này | Áp dụng |
| --- | --- |
| Form tạo bài nằm sau hero lớn và thống kê | Đưa form lên đầu, có một nút đen chính để bắt đầu. Mobile hiển thị form trước preview. |
| Chọn môn lặp ở hero và form | Giữ một select môn chính; preview tự đồng bộ. |
| Slogan, wordmark và các chi tiết không có thao tác trong hero | Thay bằng nhãn Xem trước, dữ liệu thật và điều khiển chế độ. |
| Ba lớp 3D chiếm phần lớn vùng đầu trang | Thu nhỏ thành bản xem trước hỗ trợ. Giảm chuyển động, giữ pause/reduced motion; bỏ nghiêng X/Y để nội dung rõ và ổn định hơn. |
| Thư viện, kết quả gần đây và thống kê đều thành nhiều thẻ | Dùng hàng và khoảng cách; giữ viền ở form/field cần nhận diện thao tác. |
| Lịch sử có nhiều cột và các nút đóng khung | Danh sách semantic, mỗi bài một hàng: môn, lúc nộp, điểm, thời gian, xem/xóa. Mobile chia thành hai cột, không phải kéo bảng ngang. |
| Mỗi câu hỏi là một thẻ, giải thích là hộp trong hộp | Phân câu bằng khoảng cách/đường mảnh; giữ rõ đáp án và giải thích, không đổi nội dung học. |
| Chuyển trang luôn có animation | Bỏ animation khi tải trang; giữ hover/press, thanh chế độ và menu để phản hồi thao tác. |

## Hệ thống áp dụng

- Nền nội dung #fff; sidebar #fafafa; chữ chính #181818, chữ phụ #6b6b6b.
- Cùng Be Vietnam Pro 400/600 và fallback tiếng Nhật; không thêm font hoặc thư viện.
- Khoảng cách chủ đạo 8/12/16/24/32/48px; góc điều khiển 8px, form 12px. Không bắt mọi vùng thành card.
- Điều khiển tối thiểu 44px, input mobile 16px; giữ focus, label, validation và thông báo lưu bài.
- Primary button dành cho thao tác tạo/nộp. Hành động xem/điều chỉnh/xóa dùng kiểu nhẹ hơn và giữ xác nhận xóa hiện có.
- Đáp án đúng/sai luôn có nhãn chữ; thông tin không chỉ truyền đạt bằng sắc độ.

Tra cứu cục bộ ui-ux-pro-max không có kết quả chính xác cho từ khóa proximity/grouping; không lưu kết quả không phù hợp. Các quyết định phân nhóm ở trên dựa vào nguồn NN/g và đánh giá trực tiếp trên ứng dụng.

## Ranh giới kỹ thuật

- Các URL, tên field, antiforgery token và POST handlers được giữ nguyên.
- Không đổi database, migration, tính điểm, thời gian hay cách chọn câu sai.
- Không đổi các file vendor Scroll Craft/motion. CSS của trang tổng quan và lịch sử nằm tại learning-experience.css và history.css; CSS dùng chung ở workspace.css.
- Số liệu hiển thị từ Model hiện tại; không thêm streak, huy hiệu hoặc thành tích giả.

Kết quả kiểm tra thực tế được ghi sau khi chạy tại artifacts/minimal-study (gitignore). Báo cáo giao diện trước đó: GIAO_DIEN_TOI_GIAN.md.


## Kết quả xác minh

- Build: 0 lỗi, 0 cảnh báo; JavaScript hợp lệ, git diff không có lỗi whitespace.
- Hồi quy HTTP 10 nhóm, luồng học bằng trình duyệt 8 nhóm, Scroll Craft/menu/reduced motion 11 nhóm đều qua. Request ghi thử chỉ dùng RenshuuSmokeTest; bài thử được xóa sau khi chạy.
- Kiểm tra bổ sung 18 tổ hợp trang/kích thước cho Home, History và Wrong tại 1440/1024/820/768/390/375: không tràn ngang, không lỗi JavaScript. Lịch sử có đúng link xem cho mỗi hàng.
- Nút bắt đầu kết thúc ở y=593/900px trên desktop và y=689/844px trên mobile 390/375, nằm trong viewport đầu khi không có bài đang làm. Bài chưa nộp vẫn được ưu tiên ở đầu trang khi có dữ liệu.
- layout/check.mjs: History sạch ở cả năm viewport. Home có bốn edge-left cho tiêu đề của cột phải, không tràn ngang; đây là các đường căn của grid hai cột.
- perf.mjs: 0 FAIL, 2 WARN do sáu file subset/weight của font hiện có. Median FCP/LCP local 344ms desktop, 396ms mobile, CLS 0.000, không request hỏng. Số đo local không thay cho đo triển khai mạng thật.
- consistency.mjs: các mục contrast, text-contrast, hidden-content, scroll-reveal, reduced-motion, nav-covers, overlap-text, off-viewport, forms và button-contrast PASS.

Bộ consistency ghi 43 FAIL/18 WARN ở hai viewport. Không tuyên bố đạt toàn bộ: bộ này được xây cho landing page. Đánh giá các nhóm còn lại:

| Nhóm | Đánh giá trong ứng dụng |
| --- | --- |
| hero-screen, section-height, section-air, one-section, one-heading | Workspace có nhiều vùng chức năng trong một viewport; không ép mỗi vùng thành màn hình hoặc bỏ đường phân tách danh sách. |
| container-edges, split-pair, edge-left | Sidebar 232px và grid tỷ lệ với gap 48px/28px tạo các đường căn nội bộ hợp lệ; tiêu đề cột phải không nằm trên mép trái toàn trang. |
| scroll-linked, tilt, spill, cut-text | Lớp xem trước có chủ đích chồng/di chuyển nhẹ trong vùng clip. Chỉ cảnh trang trí aria-hidden bị tác động; form luôn tĩnh. Đã kiểm tra trang không tràn ngang. |
| no-visual | Checker tìm ảnh/mock; nội dung chính là form và dữ liệu thực, không thêm ảnh trang trí. |
| copy-headings, copy-buttons, copy-length | UI tiếng Việt, tên môn và dữ liệu thay đổi theo Model; từ điển tiếng Anh/ngân sách chữ quảng cáo không dùng để cắt nội dung học tập. |
| heading-scale | Nhãn preview được đặt nhẹ hơn tiêu đề tạo bài; số liệu có cỡ chữ riêng. Không buộc mọi h2 và figure cùng độ nổi thị giác. |
| link-styles, nav-family, hover | Logo, skip-link, hàng dữ liệu và nút có vai trò khác nhau; skip-link dùng focus rõ. Nút và điều hướng có phản hồi hover/press. |

Không sửa hoặc tắt quy tắc của kit. Các kết quả JSON và ảnh nằm trong artifacts/minimal-study; report chức năng ở artifacts/scrollcraft và artifacts/study-flow. Các thư mục này được gitignore.
