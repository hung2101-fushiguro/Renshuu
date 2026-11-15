# Áp dụng Scroll Craft vào Renshuu

## Thay đổi

Tích hợp trực tiếp engine từ `nateherkai/scroll-craft`, giữ nguyên nội dung hai file engine JS/CSS và kèm MIT license. Layout chỉ tải engine trên trang tổng quan. Không đổi stack Razor Pages, migration hoặc các handler làm bài/quản trị.

Phần sản phẩm bổ sung:

- Phiên học 3D với ba lớp: ngân hàng câu hỏi, phiên được cấu hình và các câu cần ôn lại. Tất cả số liệu lấy từ subject summaries và form thực.
- Chọn môn nhanh, đổi chế độ luyện tập/ôn câu sai, đồng bộ số câu và thời gian. Liên kết ôn sai chuyển đến đúng SubjectId; nút luyện tập đưa focus đến form.
- Bản tối giản 11/11/2026 giữ parallax nhẹ trong scene 3D; giảm góc xoay, bỏ hiệu ứng đếm số, entrance form và reveal đường phân cách. Số liệu và form luôn hiện sẵn. Không dùng chuyển động theo con trỏ.
- Dừng và tháo engine khi tắt chuyển động hoặc tab bị ẩn; khôi phục mặt phẳng tĩnh hoàn chỉnh. Gắn lại đúng một instance khi bật lại hoặc trở về trang từ browser history.
- Bố cục mobile riêng. Không thêm khoảng cuộn rỗng, không ghim form và không bắt/khóa con trỏ.
- Khi JavaScript bị tắt, giữ form Razor hoạt động và hiển thị điều hướng điện thoại trực tiếp; ẩn các nút phụ thuộc JavaScript.

## Phương án thiết kế

Grammar: **Live surface**. Đây là giao diện ứng dụng đang chạy, có sidebar, dữ liệu và input thật. Không chuyển ứng dụng thành một trang marketing dài. Các lựa chọn thiết kế tiếp tục yêu cầu clean, chuyên nghiệp, animation và 3D của người dùng; không thực hiện một cuộc phỏng vấn thương hiệu mới.

Signature: cấu hình phiên học hiện lên thành các lớp tách biệt khi cuộn; thay đổi môn/số câu/chế độ làm thay đổi chính nội dung trên các lớp đó. Trọng tâm cảm giác là nhìn thấy một phiên học sẵn sàng trước khi bắt đầu. Sau đó các vùng nhập liệu và đọc nội dung giữ bình tĩnh, rõ ràng.

Brief, feeling curve và score được lưu ở [BRIEF.md](../scrollcraft/builds/renshuu-workspace/BRIEF.md). Registry ban đầu chưa có build nào, nên không có hàng cũ cần đối chiếu ở fingerprint gate.

Không tạo ảnh/video, không cần khóa kie.ai và không phát sinh chi phí tạo media. Dùng CSS 3D và markup thực, phù hợp nội dung phần mềm học tập.

## Kiểm tra

`dotnet build --nologo` thành công, 0 warning, 0 error. Script JS mới và script kiểm tra đều qua kiểm tra cú pháp. Hash hai file engine khớp bản skill đã cài.

`scripts/verify-experience.cjs` kiểm tra trên Chrome thật, chế độ headless, với pointer lock/capture native bị vô hiệu hóa trong context kiểm tra:

1. Engine chỉ mount một instance.
2. Môn học, số câu và thời gian đồng bộ với form.
3. Chế độ ôn sai dùng đúng số câu và liên kết môn.
4. Transform các lớp thay đổi theo vị trí cuộn thật.
5. Pause tháo engine, giữ hình tĩnh và resume không tạo instance thừa.
6. Không tràn ngang ở 1440, 1024, 820, 768, 390 và 375px.
7. Menu mobile mở, cô lập nội dung và trả focus khi bấm Escape.
8. Reduced motion không khởi chạy engine và vẫn dùng được form.
9. Tắt JavaScript vẫn có điều hướng mobile và form thực.
10. Không có exception JavaScript hoặc tài nguyên local trả lỗi.
11. Quay lại trang từ browser history khôi phục đúng một engine instance.

Smoke test HTTP tiếp tục qua 10 nhóm chức năng trên database riêng `RenshuuSmokeTest`, bao gồm nộp trống, chấm điểm, luyện lại câu sai, lịch sử và chống chấm lại bài đã nộp. Dữ liệu do smoke test tạo được xóa sau kiểm tra.

Harness chính thức `scripts/shoot.mjs` của Scroll Craft đã đi qua các trạng thái cuộn desktop và mobile, không phát hiện dead scroll; các cue đo được đạt tương phản tối thiểu 4.5:1. Báo cáo cùng contact sheet ở `artifacts/scrollcraft/`. Các ảnh opening, midpoint, full page, motion-off và JavaScript-off được dùng để xem thực tế, không chỉ dựa vào build.

Lượt harness chính thức ở reduced-motion không hoàn tất vì nó bắt buộc chờ `html.sc-ready`, trong khi ứng dụng chủ động không mount engine khi reduced-motion đang bật. Không sửa production để làm xanh checker. Trường hợp này được kiểm tra bằng `verify-experience.cjs` và ảnh full-page reduced-motion: không có engine, form sử dụng được và toàn bộ nội dung hiện đầy đủ.

Phát hiện đã sửa trong lần kiểm tra: `data-sc-copy` của engine mặc định ẩn/inert cho worldflight; lớp sản phẩm đặt rõ readable/clickable để dùng trong live surface. Các mặt phẳng 3D ban đầu giao nhau, đã điều chỉnh stacking context riêng. Capture sau resize phải chờ drawer kết thúc transition để phản ánh trạng thái nghỉ.

## Chạy lại kiểm tra giao diện

Cần Node, `playwright-core` và Chrome. Nếu dùng skill cài cá nhân trên máy này:

```powershell
$env:NODE_PATH = 'C:/Users/ADMIN/.agents/skills/scroll-craft/node_modules'
node scripts/verify-experience.cjs http://localhost:5186
```

Các ảnh và log trong `artifacts/` là bằng chứng local, không được Git theo dõi. Bộ kiểm tra không thay thế trải nghiệm trên điện thoại vật lý; chưa đo thao tác chạm và độ mượt trên thiết bị thật. Các giới hạn backend đã phân tích trước đó, như dữ liệu dùng chung và thời gian do trình duyệt điều khiển, vẫn giữ nguyên.
