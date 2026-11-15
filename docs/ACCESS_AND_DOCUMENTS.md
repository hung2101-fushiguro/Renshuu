# Đăng nhập, phân quyền và tài liệu

## Phạm vi và quyền truy cập

Giữ ASP.NET Core 8 Razor Pages, Service, EF Core 8/SQL Server và các URL nghiệp vụ cũ. Dùng ASP.NET Core Identity EF 8.0.32 cho tài khoản; không thêm frontend framework. Màu xanh `#2563EB`, vàng `#F59E0B`, nền `#EFF6FF` được giữ nguyên.

| Chức năng | User | Admin |
| --- | --- | --- |
| Luyện tập, tạo đề, tiếp tục bài, chấm điểm | Có | Có |
| Lịch sử, câu sai, bản nháp | Chỉ của mình | Chỉ của mình |
| Ngân hàng câu hỏi, thư mục, môn và nhập câu hỏi | Dùng chung, như dự án cũ | Giống User |
| Upload/xem/sửa/xóa tài liệu của mình | Có | Có |
| Xem/sửa/thay tệp/xóa tài liệu người khác | Không | Có |
| Quản lý danh sách, tạo/sửa/ngừng/xóa tài khoản, đổi quyền | Không | Có |

Admin có đúng hai phạm vi bổ sung: quản lý người dùng và tài liệu của tất cả người dùng. Không cho admin đọc bài làm cá nhân của người khác. Thư mục `/Admin` cũ là ngân hàng câu hỏi dùng chung; chức năng quản trị tài khoản mới nằm ở `/Administration/Users`.

## Đăng nhập và tài khoản

- Đăng ký luôn nhận vai trò User, không bind vai trò/IsActive từ request. Không tự phong admin cho tài khoản đăng ký đầu tiên.
- Đăng nhập bằng email; mật khẩu từ 10 ký tự, có chữ hoa, chữ thường và số. Hỗ trợ trình quản lý mật khẩu, dán mật khẩu và bật/tắt hiển thị.
- Cookie HttpOnly, SameSite=Lax, thời hạn 8 giờ, gia hạn khi hoạt động. Production chỉ gửi cookie qua HTTPS; bản chạy local sử dụng Development.
- Năm lần sai mật khẩu khóa đăng nhập 15 phút. Login/register POST có giới hạn theo IP. Tất cả thay đổi dữ liệu cần antiforgery; logout là POST, GET không đăng xuất.
- Kiểm tra trạng thái và security stamp mỗi request. Ngừng/xóa tài khoản, đổi quyền, đổi/reset mật khẩu thu hồi phiên cũ. Người đổi mật khẩu hoặc tự sửa thông tin qua giao diện được làm mới phiên hiện tại. Cách dùng security stamp dựa trên [hướng dẫn Identity của Microsoft](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity-configuration?view=aspnetcore-8.0).
- Chặn tự hạ quyền, tự ngừng/xóa tài khoản đang dùng và việc mất admin hoạt động cuối cùng. Thao tác quản trị dùng transaction và application lock; concurrency stamp phát hiện form đã cũ.
- Xóa tài khoản là ngừng truy cập và ẩn khỏi danh sách, giữ dòng sở hữu, bài làm và tài liệu. Email đã dùng vẫn được giữ. Tài liệu của tài khoản đã xóa vẫn quản lý được bằng admin.
- Trang cá nhân hỗ trợ đổi tên và mật khẩu. Chưa kết nối email, xác nhận email hoặc luồng khôi phục mật khẩu qua email; admin có thể đặt lại mật khẩu trong trang sửa người dùng.

## Tài liệu

- Nhận PDF, DOCX, PPTX và TXT, tối đa 20 MB/tệp. Tiêu đề, ghi chú và môn học là metadata; môn lấy từ ngân hàng hiện có.
- Mặc định lưu tại `%LOCALAPPDATA%\TracNghiemWeb\Documents`, ngoài thư mục ứng dụng/webroot. Có thể đặt `Documents__StoragePath` thành đường dẫn riêng; cấu hình trong thư mục ứng dụng bị từ chối.
- Tên lưu là GUID do server tạo. Kiểm tra đường dẫn, độ dài thực, định dạng PDF, văn bản TXT và cấu trúc Office/ZIP; giới hạn dữ liệu giải nén và XML, cấm DTD. Đây là kiểm tra định dạng/giới hạn, không phải dịch vụ quét malware. Các nguyên tắc lưu riêng và xác thực tệp dựa trên [hướng dẫn upload của Microsoft](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/file-uploads?view=aspnetcore-8.0).
- Không phát tệp bằng StaticFiles. Xem/tải xuống luôn kiểm tra chủ sở hữu hoặc quyền admin, kể cả khi đổi ID. Dùng no-store và nosniff; PDF có hỗ trợ range và viewer trình duyệt.
- DOCX/PPTX có bản xem trước văn bản, không mô phỏng bố cục Office. PDF xem bằng trình đọc tích hợp. TXT tối đa 100.000 ký tự được sửa trực tiếp; các định dạng khác sửa thông tin hoặc thay tệp gốc.
- Rowversion chống ghi đè/xóa từ form cũ. Thay tệp ghi tên mới, cập nhật SQL rồi mới xóa tệp cũ. Lỗi lưu SQL xóa bản mới và giữ bản cũ; lỗi xóa vật lý được log, không mở lại quyền truy cập vào dòng đã xóa.
- Sao lưu cả database và thư mục tài liệu. Nội dung file không đưa vào Git hoặc gói publish.

## Tương thích luồng học

Attempt thêm UserId nullable; toàn bộ đọc/ghi bài làm lọc theo tài khoản hiện tại. Bản nháp browser dùng khóa có UserId; logout hoặc đổi tài khoản xóa bản tạm của tài khoản trước. Hết phiên khi làm bài giữ lựa chọn chưa đồng bộ, dẫn về đăng nhập và phục hồi khi đúng tài khoản quay lại. Không phát lại bản nháp sang tài khoản khác.

Dữ liệu ngân hàng và ID cũ được giữ. Bài làm trước khi có tài khoản được giữ với UserId=null, không tự gán cho người đăng ký đầu tiên. Database chính của workspace trước nâng cấp có 2 môn, 20 câu và 0 bài làm, nên không có bài cũ cần gán. Các giới hạn cũ về phiên bản nội dung câu hỏi trong `STUDY_FLOW.md` vẫn áp dụng; đợt này không bổ sung snapshot ngân hàng.

## Migration và chạy local

Migration `20261114111131_AddAccountsAndPrivateDocuments` thêm 7 bảng Identity, bảng Documents, UserId/index/FK cho Attempts. Giữ nguyên 6 bảng nghiệp vụ và bảng lịch sử migration cũ, tổng 15 bảng. Forward migration không xóa bảng/cột/dữ liệu cũ.

Development tự áp dụng migration như cơ chế cũ. `database-upgrade-accounts.sql` là script idempotent để nâng từ database đã có migration `20261110092823_SaveAttemptDrafts`; không thay bằng file xuất SQL cũ thiếu lịch sử migration.

```powershell
.\scripts\start-local.ps1
# http://localhost:5080, SQL localhost, database TracNghiemWeb
```

Tạo admin bằng CLI riêng sau migration; password chỉ truyền qua biến môi trường của tiến trình, không viết vào cấu hình/source:

```powershell
# Chọn ConnectionStrings__DefaultConnection và ASPNETCORE_ENVIRONMENT phù hợp trước khi chạy.
dotnet build TracNghiemWeb.csproj -c Release
dotnet bin/Release/net8.0/TracNghiemWeb.dll --migrate
# Đặt TRACNGHIEM_PROVISION_PASSWORD riêng trong tiến trình trước lệnh sau:
dotnet bin/Release/net8.0/TracNghiemWeb.dll --provision-user admin@example.com --role Admin --name "Quản trị viên"
# Xóa biến password khi hoàn tất.
```

CLI chỉ tạo tài khoản mới, không ghi đè mật khẩu hoặc nâng quyền ngầm cho tài khoản đã tồn tại. Bản local được tạo admin riêng khi bàn giao; thông tin nằm trong tệp bỏ qua bởi Git, không có mật khẩu mặc định trong repo.

## Kiểm chứng và chạy lại

```powershell
dotnet run --project tests/TracNghiemWeb.IntegrationTests -c Release --no-launch-profile
.\scripts\setup-access-test.ps1
# Ở terminal riêng, chạy host kiểm thử:
$fixture = Get-Content artifacts/access-test.json -Raw | ConvertFrom-Json
.\scripts\start-local.ps1 -Database $fixture.Database -StoragePath $fixture.Storage -Port $fixture.Port -Configuration Release -TestMode -NoBuild
# Ở terminal đầu, cần playwright-core và jszip trong NODE_PATH, Chrome:
$fixture = Get-Content artifacts/access-test.json -Raw | ConvertFrom-Json
node scripts/verify-study-flow.cjs http://localhost:5081 $fixture.Database
node scripts/verify-access-documents.cjs http://localhost:5081 $fixture.Database
# Dừng host bằng Ctrl+C, rồi dọn đúng database/thư mục tệp kiểm thử đã tạo:
.\scripts\cleanup-access-test.ps1
```

- 11 nhóm SQL qua: các kiểm thử luồng học trước đó, nâng cấp giữ dữ liệu, tách bài làm, quyền tài liệu/CRUD/rowversion/định dạng, quản trị tài khoản/security stamp/admin cuối cùng. Tạo và xóa đúng database riêng mỗi lần.
- 11 nhóm trình duyệt luồng học trước đó vẫn qua với đăng nhập.
- 16 nhóm trình duyệt tài khoản/tài liệu qua: ẩn danh/CSRF; login; đăng ký chống nâng quyền; upload/Unicode/XSS; chặn truy cập chéo; PDF/DOCX/PPTX; định dạng; admin sửa/thay tệp; xác nhận xóa; tạo/ngừng user; đổi quyền; xóa giữ sở hữu; return URL/logout; hết phiên/đổi tài khoản/bản nháp; profile/password; responsive/bàn phím/reduced motion.
- Ảnh thật trên Chrome headless ở 390/768/1024/1440 px, có kiểm tra PDF native viewer. Báo cáo và credential fixture chỉ nằm trong `artifacts/`, không publish.

Trước khi cập nhật database chính: sao lưu COPY_ONLY có checksum, xác nhận counts, áp dụng migration và tạo admin được chỉ định. Không chép dữ liệu/tài khoản thử vào database chính.
