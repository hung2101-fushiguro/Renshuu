# Luồng tạo đề và làm bài

## Phân tích yêu cầu

Đây là ứng dụng luyện tập dùng ngân hàng câu hỏi có sẵn. Giữ ASP.NET Core 8 Razor Pages → QuizService/ImportService → EF Core 8 → SQL Server, các URL hiện có và bộ màu xanh `#2563EB`, vàng `#F59E0B`, nền `#EFF6FF`. Không thêm framework giao diện hay package chạy ứng dụng.

| Yêu cầu | Tình trạng cũ | Triển khai |
| --- | --- | --- |
| Chọn môn, số câu, thời gian | Đã có form cơ bản | Form gọn, nút chọn nhanh, giới hạn theo ngân hàng, xem cấu hình trước khi tạo |
| Ngẫu nhiên theo nhóm | Có nhóm bài đọc nhưng có thể cắt nhóm vượt 200 | Trộn nhóm, tìm tổ hợp đủ số câu nếu có, giữ nguyên nhóm và giới hạn 200 |
| Trả lời, theo dõi tiến độ | Chỉ có thanh phần trăm | Số câu đã trả lời, bảng chuyển câu, tìm câu còn trống, bỏ chọn |
| Lưu tạm, tiếp tục | Chỉ sessionStorage trong một tab | Tự lưu SQL, bản tạm khi lỗi mạng, lưu & thoát, tiếp tục từ trang chủ |
| Đếm giờ | Chỉ đồng hồ thiết bị | Thời gian còn lại từ server, đồng bộ lại khi trở về tab, tự nộp và kiểm tra hạn phía server |
| Nộp và chấm | Có chấm cơ bản | Xác nhận câu trống, trạng thái gửi, thử lại khi lỗi mạng, nộp nhiều lần giữ nguyên điểm |
| Giao diện ít mô tả | Hướng dẫn xen vào phần thao tác | Ưu tiên form và số liệu thật; hướng dẫn nhóm câu nằm trong mục mở khi cần |

Tham khảo nghiệp vụ lưu bài và bố cục của bản Group_Hung trước khi có tài khoản, không đưa hệ thống tài khoản, AI hay chủ đề màu đen trắng của bản đó vào dự án này.

## Quy tắc tạo đề

- Chỉ lấy câu thuộc môn đã chọn. Mỗi câu độc lập là một nhóm; các câu cùng `PassageId` là một nhóm bài đọc, giữ thứ tự trong nhóm.
- Trộn các nhóm trước khi tìm tổ hợp. Ưu tiên chính xác số câu yêu cầu. Nếu không có tổ hợp đủ số câu, chọn tổ hợp gần nhất phía trên; nếu không thể vượt mục tiêu mà vẫn trong 200 câu, chọn phía dưới. Không cắt nhóm.
- Nhóm hơn 200 câu bị bỏ qua. Nếu không còn nhóm hợp lệ, báo lỗi thay vì tạo đề rỗng.
- Số câu: 1–200; thời gian: 1–240 phút, để trống khi luyện không giới hạn. Ngân hàng ít câu hơn yêu cầu thì lấy theo số câu thực có.
- Ôn sai tiếp tục dùng lần nộp mới nhất cho từng câu, tối đa 200 câu một lượt.

## Lưu bài và nộp bài

- Lựa chọn dùng cột `AttemptAnswers.SelectedOption` hiện có. Bản nháp chưa được chấm và không đi vào thống kê hoặc danh sách câu sai.
- Tự lưu sau 600 ms không có thay đổi mới. Mỗi trang chỉ gửi một lượt lưu tại một thời điểm; chỉnh tiếp trong lúc gửi sẽ được lưu ở lượt sau.
- Lưu cả trạng thái bỏ chọn, kể cả khi bỏ lựa chọn cuối cùng. Bản localStorage chỉ được phát lại khi còn thay đổi chưa đồng bộ.
- Nếu mất mạng: giữ bản tạm, báo đúng trạng thái, thử lại sau 5 giây, định kỳ và khi mạng trở lại. Nếu trình duyệt chặn lưu trữ, trang yêu cầu giữ nguyên trang để thử lại.
- Không xóa bản tạm ngay khi nhấn Nộp. Chỉ xóa khi trang kết quả đã được tải. Gửi lỗi vẫn có thể thử nộp lại.
- Nút Lưu & thoát gửi lựa chọn lên server rồi về trang chủ. Không có JavaScript vẫn lưu và nộp qua form được; tự lưu và đồng hồ trực tiếp cần JavaScript.
- Nộp khi còn câu trống mở xác nhận, hiển thị số câu chưa làm. Nộp trống hoàn toàn được chấm 0, không gây lỗi binding.
- Lưu và nộp khóa cùng dòng Attempt trong transaction. Lưu sau khi nộp bị từ chối; POST nộp lặp lại trả kết quả đã có.

## Thời gian

- Deadline = `StartedAt + TimeLimitMinutes`, giữ cách lưu giờ địa phương tương thích dữ liệu cũ. `TimeProvider` cho phép kiểm thử thời gian.
- Bản nháp không được sửa từ deadline trở đi. Kiểm tra lại thời hạn sau khi đã lấy khóa ghi, tránh request chờ khóa vượt hạn.
- Request nộp có 5 giây đệm truyền mạng. Sau đó chỉ chấm lựa chọn đã lưu trước hạn.
- Đồng hồ khởi tạo bằng thời gian còn lại do server tính, không khởi động lại khi tải trang. Đồng bộ trạng thái mỗi 30 giây và khi trở về tab; thời gian trôi qua khi thiết bị ngủ vẫn được tính.
- Hết giờ khi đang mở trang sẽ tự nộp. Bài đóng tab được chốt khi người dùng mở lại hoặc gửi request; chưa thêm tác vụ nền để tự chốt mọi bài đã đóng.
- Thời lượng hiển thị không vượt giới hạn đề. Lịch sử và ôn sai theo lần nộp, không theo thứ tự tạo đề.

## Database và chạy trên máy

Migration `20261110092823_SaveAttemptDrafts` chỉ thêm cột nullable `DraftSavedAt` và index `(SubmittedAt, Id)` vào Attempts. Giữ nguyên 7 bảng, quan hệ, ngân hàng và lịch sử. Không sửa file SQL xuất từ máy SATORU; file đó thiếu dữ liệu và lịch sử migrations nên không dùng để ghi đè database đang chạy.

```powershell
.\scripts\start-local.ps1
# Mặc định: SQL Server localhost, database TracNghiemWeb, web http://localhost:5080
# Kiểm thử bằng database khác:
.\scripts\start-local.ps1 -Database TracNghiemWeb_UiTest_<32-hex-chars> -Port 5081
```

Script đặt cấu hình riêng cho tiến trình và phục hồi biến môi trường khi thoát. Development áp dụng migrations theo cơ chế có sẵn. Dữ liệu mẫu chỉ được thêm nếu ngân hàng đang trống.

Trước khi nâng cấp database chính đã tạo bản backup `COPY_ONLY`, có checksum, trong thư mục Backup mặc định của SQL Server, tên bắt đầu `TracNghiemWeb_before_study_upgrade_`.

## Kiểm chứng

```powershell
dotnet run --project tests/TracNghiemWeb.IntegrationTests --no-launch-profile
# Cần playwright-core trong NODE_PATH và Chrome; chỉ nhận database kiểm thử có tên đúng mẫu:
node scripts/verify-study-flow.cjs http://localhost:5081 TracNghiemWeb_UiTest_<32-hex-chars>
dotnet publish TracNghiemWeb.csproj -c Release -o artifacts/publish
```

- 8 nhóm SQL: nâng cấp giữ dữ liệu, thứ tự nộp, bản nháp, deadline/đệm mạng, tranh chấp lưu/nộp và nộp lặp, giới hạn nhóm 200/201, tổ hợp nhóm/ngẫu nhiên, biên đầu vào/bỏ chọn/nộp trống. Mỗi lần tạo database riêng và xóa đúng database đó khi kết thúc.
- 11 nhóm trình duyệt: chọn cấu hình/tìm môn; tạo đề/tiến độ/lưu/tiếp tục; phiên mới/chỉnh nhanh; mất mạng/bỏ chọn; không JavaScript; điều hướng câu/bàn phím/mobile/reduced motion; xác nhận/chấm/nộp lặp/CSRF/tab cũ; lỗi gửi/thử lại; nộp trống; tự nộp khi hết giờ; các trang chức năng cũ.
- Kiểm tra bố cục ở 390, 768, 820, 1024, 1440 px. Ảnh và báo cáo cục bộ trong `artifacts/`, không đưa vào Git hoặc gói publish.
- Giữ Be Vietnam Pro nhưng chỉ tải hai độ đậm 400/600; chữ Nhật dùng font hệ thống dự phòng, tránh tải toàn bộ stylesheet và font chia nhỏ của Noto Sans JP cho mọi trang.
- Đo cục bộ sau tối ưu, trung vị 3 lượt ở desktop/mobile: khoảng 156–157 KiB và 12 request, so với khoảng 399 KiB và 19 request trước tối ưu; CLS = 0, không có request hỏng. Bộ kiểm tra hiệu năng có 0 FAIL, còn 2 WARN cho sáu file font theo độ đậm/bộ ký tự. Đây là phép đo trên máy phát triển, không phải kết quả tải thực tế ngoài Internet.
- Đây là giao diện ứng dụng: giữ nội dung theo chiều cao tự nhiên, bảng chuyển câu và thanh tiến độ sticky. Các quy tắc hero/section toàn màn hình dành cho landing page không áp dụng vì gây khó thao tác làm bài.
- Checker landing page báo `hero-fold`, `screen-short` và `edge-left` do bố cục ứng dụng không dùng hero toàn màn hình hoặc lưới 12 cột. Đây là khác biệt có chủ đích; cả năm kích thước đều không tràn ngang, lề ngoài giữ chung container.

Mô hình dữ liệu vẫn dùng chung như bản gốc; chưa bổ sung tài khoản, chia sẻ đề hoặc phiên bản đóng băng nội dung câu hỏi. Nếu quản trị sửa câu đang nằm trong đề, bài vẫn dùng nội dung ngân hàng hiện tại. Đây là phạm vi cần mở rộng riêng khi đưa vào sử dụng nhiều người.
