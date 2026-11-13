# Cập nhật database SQL Server

`update-database.sql` được xuất từ toàn bộ EF Core migrations, gồm tài khoản, chế độ ôn, phân loại câu hỏi và tính năng người học. Xem `TASK-LOG.md` để biết thay đổi theo task.

## Hướng dẫn cho agent của thành viên trong nhóm

Có thể gửi agent yêu cầu sau:

> Đọc AGENTS.md, database/README.md và database/TASK-LOG.md. Xác định SQL Server và database mà ứng dụng trên máy tôi đang dùng từ cấu hình hiệu lực, không in mật khẩu hoặc connection string. Kiểm tra migration đã áp dụng, đối chiếu file SQL với migrations của nhánh hiện tại. Sao lưu database có dữ liệu, kiểm tra backup, rồi chạy database/update-database.sql trên đúng database. Chạy database/verify-database.sql, đối chiếu số bản ghi trước/sau và báo kết quả. Không xóa/tạo lại database, không chạy Down, không đổi tên server theo máy của thành viên khác.

1. Lấy đầy đủ code và migrations của nhánh cần chạy trước khi cập nhật. File SQL không thay thế code ứng dụng.
2. Xác định đúng server/database. `localhost` bên dưới chỉ là ví dụ; máy dùng SQL Express có thể có instance khác. Nếu chưa rõ đích, cần hỏi chủ máy.
3. Kiểm tra `SELECT MigrationId FROM __EFMigrationsHistory ORDER BY MigrationId;`. Nếu bảng lịch sử không tồn tại trong database đã có bảng ứng dụng, dừng để đối chiếu; không tự giả lập lịch sử.
4. Sao lưu và xác minh bản sao lưu trước khi cập nhật database có dữ liệu. Tạm ngừng thao tác ghi trong lúc cập nhật.
5. Chạy SQL bằng `sqlcmd -b` để lỗi làm lệnh thất bại. Nếu lỗi, kiểm tra lịch sử migration trước khi chạy lại, vì các migration trước đó có thể đã commit.
6. Chạy lệnh xác minh bên dưới, kiểm tra lại dữ liệu và khởi động ứng dụng bằng code mới. Development startup có thể áp dụng migrations còn thiếu nên không chạy trước khi xác định đúng database.

```powershell
sqlcmd -S localhost -E -d TracNghiemWeb -b -i database/update-database.sql
if ($LASTEXITCODE -ne 0) { throw 'Database update failed' }
sqlcmd -S localhost -E -d TracNghiemWeb -b -i database/verify-database.sql
if ($LASTEXITCODE -ne 0) { throw 'Database verification failed' }
```

Script xác minh hiện kỳ vọng 6 migrations đến `20261113110547_AddLearnerFeatures`. Mỗi task thay đổi schema phải cập nhật đồng thời SQL, xác minh và nhật ký. Task không đổi schema ghi rõ “Không cần SQL/migration”.

## Chạy bằng SSMS

1. Kết nối SQL Server trên máy bạn. Tạo database `TracNghiemWeb` nếu chưa có.
2. Mở `update-database.sql`, chọn đúng database `TracNghiemWeb` trên thanh công cụ rồi Execute.
3. Cấu hình kết nối ứng dụng theo [LOGIN.md](../LOGIN.md).

Script kiểm tra `__EFMigrationsHistory` và chỉ áp dụng migration còn thiếu; có thể chạy lại trên database được quản lý bằng migrations của dự án. Nếu database có sẵn bảng nhưng không có lịch sử migrations tương ứng, cần đối chiếu cấu trúc trước khi chạy. Sao lưu database có dữ liệu quan trọng trước khi cập nhật.

Hoặc dùng PowerShell tại thư mục dự án (đổi server/database theo máy):

```powershell
sqlcmd -S localhost -E -d TracNghiemWeb -b -i database/update-database.sql
```

Script chỉ cập nhật cấu trúc và lịch sử migrations, không chứa dữ liệu câu hỏi, tài khoản hoặc mật khẩu local. Database mới tạo bằng script sẽ chưa có câu hỏi; có thể nhập câu hỏi qua giao diện Admin. Đăng ký tài khoản rồi cấp quyền Admin theo hướng dẫn trong LOGIN.md.

## Xuất lại khi thêm migration

```powershell
dotnet ef migrations script --idempotent --project TracNghiemWeb.csproj --output database/update-database.sql
```
