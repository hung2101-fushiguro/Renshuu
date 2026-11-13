# Cập nhật database SQL Server

`update-database.sql` được xuất từ toàn bộ EF Core migrations, gồm bảng tài khoản/quyền, `Attempts.UserId` và `Attempts.IsPractice`.

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
