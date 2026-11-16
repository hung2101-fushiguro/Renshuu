# Database handoff — TracNghiemWeb

Thư mục này chứa phần bàn giao database cho đồng đội. Mỗi thành viên tự xác định
SQL Server/database của mình; **không giả định** dùng chung instance với người khác.

| Tệp | Nội dung |
| --- | --- |
| `update-database.sql` | Script idempotent đã lưu trong repo, kết thúc ở `20261115155351_AddPrivateDocuments`. |
| `verify-database.sql` | Script chỉ đọc dành cho schema ở mốc `AddPrivateDocuments`. |
| `TASK-LOG.md` | Nhật ký thay đổi database theo từng task. |

Migration mới nhất là `20261115163138_RemoveDocumentsAndAddPublicSubjects`. Khi triển khai
mã hiện tại, ưu tiên cập nhật bằng EF Core ở mục 3. Nếu triển khai bằng SQL, hãy sinh lại
script từ mã hiện tại theo mục 6. Migration này xóa bảng `Documents`; nếu database từng
dùng chức năng tài liệu, hãy sao lưu trước khi áp dụng.

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

Cách B — chạy script SQL đã sinh lại theo mục 6 (máy đích chỉ cần `sqlcmd`):

```powershell
sqlcmd -S "<server>" -d "<database>" -E -b -I -i "$env:TEMP\Renshuu-update-current.sql"
```

`-b` dừng ngay khi có lỗi; `-I` bật `QUOTED_IDENTIFIER`. Script chạy trong transaction
và ghi từng migration vào `__EFMigrationsHistory`, nên chạy lại là an toàn.

## 4. Kiểm tra sau khi cập nhật

Chạy các truy vấn chỉ đọc sau trên database đích:

```sql
SELECT [MigrationId]
FROM [dbo].[__EFMigrationsHistory]
WHERE [MigrationId] = N'20261115163138_RemoveDocumentsAndAddPublicSubjects';

SELECT COL_LENGTH(N'dbo.Subjects', N'IsPublic') AS IsPublicColumnBytes,
       OBJECT_ID(N'dbo.Documents', N'U') AS DocumentsTableId;
```

Migration phải xuất hiện trong lịch sử, `IsPublicColumnBytes` phải có giá trị và
`DocumentsTableId` phải là `NULL`. Script `verify-database.sql` vẫn kiểm tra bảng
`Documents` ở mốc schema cũ.

## 5. Sau khi có schema: tạo tài khoản quản trị đầu tiên

Đặt cấu hình quản trị ban đầu **trước khi** khởi động app. Không có mật khẩu mặc định
trong mã nguồn và trang đăng nhập không hiển thị tài khoản thử.

```powershell
# Development — user secrets (không commit)
dotnet user-secrets set "BootstrapAdmin:Email" "admin@ten-mien-cua-ban"
dotnet user-secrets set "BootstrapAdmin:Password" "<mật-khẩu-mạnh-của-bạn>"

# Production / máy chủ khác — biến môi trường
$env:BootstrapAdmin__Email = 'admin@ten-mien-cua-ban'
$env:BootstrapAdmin__Password = '<mật-khẩu-mạnh-của-bạn>'
```

- Email đã tồn tại: app chỉ **gán** vai trò Admin, không đổi mật khẩu.
- Email chưa tồn tại + có mật khẩu: app tạo tài khoản mới với vai trò Admin.
- Thiếu cấu hình: app **không tạo** tài khoản nào và ghi log cảnh báo (Development hướng
  dẫn `dotnet user-secrets`, Production hướng dẫn biến môi trường).
- Khóa cũ `DevelopmentAdmin:Email`/`DevelopmentAdmin:Password`/`DevelopmentAdmin:LoginPassword`
  vẫn được đọc để không phá cấu hình local cũ.
- Nếu email/mật khẩu quản trị từng nằm trong lịch sử git hoặc tài liệu cũ, hãy **đổi mật
  khẩu** tài khoản quản trị thật trên mọi môi trường đã dùng nó.

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
| `20261115155351_AddPrivateDocuments` | Tạo bảng `Documents` cho tài liệu riêng. |
| `20261115163138_RemoveDocumentsAndAddPublicSubjects` | Xóa bảng `Documents`, thêm `Subjects.IsPublic` và chuyển môn chưa có chủ sang công khai. |

Sinh lại script sau mỗi lần đổi schema:

```powershell
dotnet ef migrations script --idempotent --project TracNghiemWeb.csproj --output "$env:TEMP\Renshuu-update-current.sql"
```
