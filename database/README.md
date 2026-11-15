# Database handoff — TracNghiemWeb

Thư mục này chứa phần bàn giao database cho đồng đội. Mỗi thành viên tự xác định
SQL Server/database của mình; **không giả định** dùng chung instance với người khác.

| Tệp | Nội dung |
| --- | --- |
| `update-database.sql` | Script idempotent do `dotnet ef migrations script --idempotent` sinh ra. Chạy được nhiều lần, chỉ áp dụng migration còn thiếu. |
| `verify-database.sql` | Script chỉ đọc, kiểm tra migration, bảng/cột/index/khóa ngoại và đếm dòng. |
| `TASK-LOG.md` | Nhật ký thay đổi database theo từng task. |

## 1. Xác định server/database hiệu lực

Ứng dụng đọc `ConnectionStrings:DefaultConnection`. Thứ tự ưu tiên của .NET:

1. Biến môi trường `ConnectionStrings__DefaultConnection`
2. `dotnet user-secrets` (chỉ Development)
3. `appsettings.Development.json`
4. `appsettings.json` (giá trị mặc định trong repo)

Xem giá trị đang dùng mà **không in mật khẩu**:

```powershell
# Chỉ in server + database, không in mật khẩu/tài khoản
$raw = $env:ConnectionStrings__DefaultConnection
if (-not $raw) {
    $raw = (Get-Content appsettings.json -Raw | ConvertFrom-Json).ConnectionStrings.DefaultConnection
}
$cs = New-Object System.Data.SqlClient.SqlConnectionStringBuilder $raw
"$($cs.DataSource) / $($cs.InitialCatalog)"
```

Hoặc dùng chính EF Core (in ra migration sẽ áp dụng, không đổi dữ liệu):

```powershell
dotnet ef migrations list --project TracNghiemWeb.csproj
```

## 2. Sao lưu trước khi cập nhật

```powershell
# Đổi <server>/<database> cho đúng môi trường của bạn
sqlcmd -S "<server>" -E -Q "BACKUP DATABASE [<database>] TO DISK = N'D:\backup\<database>_truoc-cap-nhat.bak' WITH INIT, COMPRESSION" -b
```

Kiểm tra lịch sử migration hiện có:

```powershell
sqlcmd -S "<server>" -d "<database>" -E -Q "SELECT MigrationId FROM __EFMigrationsHistory ORDER BY MigrationId" -b
```

## 3. Áp dụng thay đổi

Chọn **một** trong hai cách.

Cách A — EF Core (khuyến nghị khi máy có SDK):

```powershell
dotnet ef database update --project TracNghiemWeb.csproj `
  --connection "Server=<server>;Database=<database>;Trusted_Connection=True;TrustServerCertificate=True"
```

Cách B — chạy SQL đã sinh sẵn (máy chỉ có `sqlcmd`, không cần SDK):

```powershell
sqlcmd -S "<server>" -d "<database>" -E -b -I -i database\update-database.sql
```

`-b` dừng ngay khi có lỗi; `-I` bật `QUOTED_IDENTIFIER`. Script chạy trong transaction
và ghi từng migration vào `__EFMigrationsHistory`, nên chạy lại là an toàn.

## 4. Kiểm tra sau khi cập nhật

```powershell
sqlcmd -S "<server>" -d "<database>" -E -b -i database\verify-database.sql
```

Kết quả mong đợi:

- `CHECK OK` cho migration `20261114025237_AddIdentityAccountsAndOwnership`,
  7 bảng Identity, cột `OwnerId`/`CreatedAt`, 3 khóa ngoại `OwnerId`.
- Bảng đếm dòng khớp với bản sao lưu (trừ bảng mới được tạo).
- Database trống: `CHECK INFO` về vai trò Admin là bình thường — lần chạy app
  Development đầu tiên sẽ tạo vai trò và tài khoản quản trị theo cấu hình.

## 5. Sau khi có schema: tạo tài khoản quản trị đầu tiên

Chỉ trong Development, đặt user secrets rồi khởi động lại app:

```powershell
dotnet user-secrets set "DevelopmentAdmin:Email" "admin@ten-mien-cua-ban"
dotnet user-secrets set "DevelopmentAdmin:Password" "MatKhauManh#2026"
dotnet user-secrets set "DevelopmentAdmin:ShowLoginCredentials" "true"   # tùy chọn, chỉ hiện gợi ý ở trang đăng nhập
```

- Email đã tồn tại: app chỉ **gán** vai trò Admin, không đổi mật khẩu.
- Email chưa tồn tại + có mật khẩu: app tạo tài khoản mới với vai trò Admin.
- Khóa cũ `DevelopmentAdmin:LoginPassword` vẫn được chấp nhận để không phá cấu hình local cũ.

Tài khoản sau đó quản lý người dùng tại `/Admin/Accounts`. Không có mật khẩu mặc định
trong mã nguồn; không commit user secrets hay connection string.

## 6. Migration hiện có

| Migration | Nội dung |
| --- | --- |
| `20261019064138_InitialCreate` | Bảng môn học, câu hỏi, bài làm. |
| `20261023051104_AddReadingPassages` | Bài đọc và liên kết câu hỏi. |
| `20261110061027_AddSubjectFolders` | Thư mục môn học, gộp `JLPT-N3*`. |
| `20261110092823_SaveAttemptDrafts` | Lưu nháp, deadline, chỉ mục nộp bài. |
| `20261114025237_AddIdentityAccountsAndOwnership` | Identity (`AspNet*`), `OwnerId`/`CreatedAt`, khóa ngoại và index chủ sở hữu. |

Sinh lại script sau mỗi lần đổi schema:

```powershell
dotnet ef migrations script --idempotent --project TracNghiemWeb.csproj --output database/update-database.sql
```
