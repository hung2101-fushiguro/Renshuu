# TASK-LOG — thay đổi database

Ghi theo thứ tự mới nhất ở trên. Mỗi task ghi rõ: có đổi schema hay không, migration
nào, đã sinh/chạy SQL nào, database nào đã thực sự được cập nhật.

---

## 2026-11-14 — Đăng nhập, phân quyền và quản lý tài khoản

**Có thay đổi schema.** Migration: `20261114025237_AddIdentityAccountsAndOwnership`.

### Thay đổi

- Thêm toàn bộ bảng ASP.NET Core Identity: `AspNetUsers`, `AspNetRoles`,
  `AspNetUserRoles`, `AspNetUserClaims`, `AspNetRoleClaims`, `AspNetUserLogins`,
  `AspNetUserTokens`.
- `AspNetUsers` có thêm `DisplayName` (nvarchar(120), null) và `CreatedAt` (datetime2).
- `Subjects`: thêm `OwnerId` (nvarchar(450), null) + FK `AspNetUsers` (`SET NULL`),
  `CreatedAt` (datetime2, default `GETDATE()`), index `IX_Subjects_OwnerId`.
- `SubjectFolders`: thêm `OwnerId` (null) + FK (`SET NULL`), `CreatedAt`,
  index duy nhất có lọc `IX_SubjectFolders_OwnerId_Name` trên `(OwnerId, Name)`.
  Index duy nhất cũ `IX_SubjectFolders_Name` bị bỏ để hai tài khoản có thể trùng tên thư mục.
- `Attempts`: thêm `OwnerId` (null) + FK `AspNetUsers` (`CASCADE`), index `IX_Attempts_OwnerId`.

### Ảnh hưởng dữ liệu

- Dữ liệu cũ được giữ nguyên. `OwnerId` của bản ghi cũ là `NULL` → được coi là
  **nội dung dùng chung** (thư viện môn học/thư mục hiển thị cho mọi tài khoản để luyện).
- Bài làm cũ có `OwnerId = NULL` không hiển thị trong lịch sử của bất kỳ tài khoản nào
  vì lịch sử lọc theo chủ sở hữu. Nếu cần giữ lịch sử cũ, phải gán `OwnerId` thủ công
  cho một tài khoản cụ thể — xem mục "Gán lại bài làm cũ" bên dưới.
- Xóa tài khoản: thư mục/môn học của tài khoản được chuyển thành nội dung dùng chung
  (`OwnerId = NULL`) trước khi xóa user; bài làm bị xóa theo `CASCADE`.

### Đã sinh gì

- `dotnet ef migrations script --idempotent --project TracNghiemWeb.csproj --output database/update-database.sql`
  → đã sinh lại `update-database.sql` (có đủ 5 migration, idempotent).
- `database/verify-database.sql` viết mới (chỉ đọc).
- `database/README.md` viết mới (hướng dẫn bàn giao).

### Đã chạy trên database nào

- **Đã chạy** `dotnet ef database update` trên database tạm `RenshuuVerify_Check`
  (SQL Server `localhost`) để kiểm tra: 5/5 migration áp dụng, `verify-database.sql`
  trả `CHECK OK` cho schema.
- **Đã chạy** bộ kiểm thử tích hợp SQL (`tests/Renshuu.IntegrationTests`) trên các
  database tạm `RenshuuRegression_*`; tất cả 7 nhóm kiểm tra đạt, database tạm đã xóa.
- **Đã chạy** `scripts/verify-auth.ps1` trên `RenshuuVerify_Check` (cổng 5080):
  20/20 kiểm tra đăng nhập/phân quyền đạt.
- Database `TracNghiemWeb` trong `appsettings.json` (máy dev) **chưa** được cập nhật
  trong task này — chạy `database\update-database.sql` theo `README.md` khi cần.

### Gán lại bài làm cũ (tùy chọn, chỉ khi muốn giữ lịch sử)

```sql
-- Chạy sau khi đã có tài khoản đích; thay email cho đúng
DECLARE @owner nvarchar(450) = (SELECT Id FROM AspNetUsers WHERE Email = N'email@cua-ban');
UPDATE Attempts SET OwnerId = @owner WHERE OwnerId IS NULL;
```

### Việc cần làm khi merge

1. Pull code, xác định server/database hiệu lực (README mục 1).
2. Sao lưu database (README mục 2).
3. Chạy `database\update-database.sql` hoặc `dotnet ef database update`.
4. Chạy `database\verify-database.sql`, đối chiếu `CHECK OK` và số dòng.
5. Cấu hình `DevelopmentAdmin:Email` / `DevelopmentAdmin:Password` để có quản trị viên đầu tiên.

---

## Trước đó

Không có thư mục `database/` trong lịch sử Git của nhánh này; các migration trước
(`InitialCreate`, `AddReadingPassages`, `AddSubjectFolders`, `SaveAttemptDrafts`) được
team áp dụng qua `dotnet ef database update` khi chạy app Development.
