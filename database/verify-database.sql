/*
    verify-database.sql
    Kiểm tra nhanh một database SQL Server của TracNghiemWeb sau khi chạy
    database/update-database.sql (hoặc sau khi app Development tự migrate).

    Cách dùng (chạy trên đúng server/database của bạn, không cần sửa file):
        sqlcmd -S "<server>" -d "<database>" -E -b -i database\verify-database.sql
        hoặc mở trong SSMS/Azure Data Studio và Execute trên database đích.

    Script chỉ đọc (SELECT / INFORMATION_SCHEMA), không thay đổi dữ liệu.
    Các dòng bắt đầu bằng "CHECK" là kết quả cần đạt "OK".
*/

SET NOCOUNT ON;

PRINT '--- 1. Database hiện tại ---';
SELECT
    DB_NAME() AS DatabaseName,
    SUSER_SNAME() AS LoginName,
    (SELECT SERVERPROPERTY('ServerName')) AS ServerName;

PRINT '--- 2. Lịch sử migration (phải có đủ 5 bản, mới nhất AddIdentityAccountsAndOwnership) ---';
SELECT [MigrationId], [ProductVersion]
FROM [__EFMigrationsHistory]
ORDER BY [MigrationId];

IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory]
               WHERE [MigrationId] = N'20261114025237_AddIdentityAccountsAndOwnership')
    PRINT 'CHECK FAIL: thiếu migration 20261114025237_AddIdentityAccountsAndOwnership';
ELSE
    PRINT 'CHECK OK: đã có migration AddIdentityAccountsAndOwnership';

PRINT '--- 3. Bảng Identity bắt buộc ---';
DECLARE @required TABLE (TableName sysname);
INSERT INTO @required (TableName) VALUES
    (N'AspNetUsers'), (N'AspNetRoles'), (N'AspNetUserRoles'),
    (N'AspNetUserClaims'), (N'AspNetRoleClaims'),
    (N'AspNetUserLogins'), (N'AspNetUserTokens');

SELECT r.TableName,
       CASE WHEN t.object_id IS NULL THEN 'MISSING' ELSE 'OK' END AS [Status]
FROM @required r
LEFT JOIN sys.tables t ON t.name = r.TableName
ORDER BY r.TableName;

IF EXISTS (
    SELECT 1 FROM @required r
    LEFT JOIN sys.tables t ON t.name = r.TableName
    WHERE t.object_id IS NULL)
    PRINT 'CHECK FAIL: thiếu bảng Identity ở trên';
ELSE
    PRINT 'CHECK OK: đủ 7 bảng Identity';

PRINT '--- 4. Cột Ownership mới trên dữ liệu học tập ---';
SELECT c.TABLE_NAME, c.COLUMN_NAME, c.DATA_TYPE, c.IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS c
WHERE c.TABLE_NAME IN (N'Subjects', N'SubjectFolders', N'Attempts')
  AND c.COLUMN_NAME IN (N'OwnerId', N'CreatedAt')
ORDER BY c.TABLE_NAME, c.COLUMN_NAME;

IF (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
    WHERE COLUMN_NAME = N'OwnerId' AND TABLE_NAME IN (N'Subjects', N'SubjectFolders', N'Attempts')) <> 3
    PRINT 'CHECK FAIL: thiếu cột OwnerId (cần đủ 3 bảng)';
ELSE IF (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
         WHERE COLUMN_NAME = N'CreatedAt' AND TABLE_NAME IN (N'Subjects', N'SubjectFolders')) <> 2
    PRINT 'CHECK FAIL: thiếu cột CreatedAt (cần đủ 2 bảng)';
ELSE
    PRINT 'CHECK OK: đủ cột OwnerId/CreatedAt';

PRINT '--- 5. Khóa ngoại tới AspNetUsers (SetNull cho nội dung, Cascade cho bài làm) ---';
SELECT fk.name AS ForeignKeyName,
       OBJECT_NAME(fk.parent_object_id) AS TableName,
       fk.delete_referential_action_desc AS OnDelete
FROM sys.foreign_keys fk
WHERE fk.name IN (
    N'FK_Subjects_AspNetUsers_OwnerId',
    N'FK_SubjectFolders_AspNetUsers_OwnerId',
    N'FK_Attempts_AspNetUsers_OwnerId')
ORDER BY fk.name;

IF (SELECT COUNT(*) FROM sys.foreign_keys
    WHERE name IN (N'FK_Subjects_AspNetUsers_OwnerId',
                   N'FK_SubjectFolders_AspNetUsers_OwnerId',
                   N'FK_Attempts_AspNetUsers_OwnerId')) <> 3
    PRINT 'CHECK FAIL: thiếu khóa ngoại OwnerId';
ELSE
    PRINT 'CHECK OK: đủ 3 khóa ngoại OwnerId';

PRINT '--- 6. Index duy nhất theo chủ sở hữu ---';
SELECT i.name AS IndexName, OBJECT_NAME(i.object_id) AS TableName, i.is_unique, i.filter_definition
FROM sys.indexes i
WHERE i.name IN (N'IX_SubjectFolders_OwnerId_Name', N'IX_Subjects_OwnerId', N'IX_Attempts_OwnerId')
ORDER BY i.name;

PRINT '--- 7. Đếm dòng để so sánh trước/sau khi cập nhật ---';
SELECT N'Subjects' AS TableName, COUNT(*) AS [RowCount] FROM dbo.Subjects
UNION ALL SELECT N'SubjectFolders', COUNT(*) FROM dbo.SubjectFolders
UNION ALL SELECT N'Questions', COUNT(*) FROM dbo.Questions
UNION ALL SELECT N'ReadingPassages', COUNT(*) FROM dbo.ReadingPassages
UNION ALL SELECT N'Attempts', COUNT(*) FROM dbo.Attempts
UNION ALL SELECT N'AttemptAnswers', COUNT(*) FROM dbo.AttemptAnswers
UNION ALL SELECT N'AspNetUsers', COUNT(*) FROM dbo.AspNetUsers
UNION ALL SELECT N'AspNetRoles', COUNT(*) FROM dbo.AspNetRoles
UNION ALL SELECT N'AspNetUserRoles', COUNT(*) FROM dbo.AspNetUserRoles;

PRINT '--- 8. Vai trò và số quản trị viên (không in email/mật khẩu) ---';
SELECT r.Name AS RoleName, COUNT(ur.UserId) AS UserCount
FROM dbo.AspNetRoles r
LEFT JOIN dbo.AspNetUserRoles ur ON ur.RoleId = r.Id
GROUP BY r.Name;

IF NOT EXISTS (SELECT 1 FROM dbo.AspNetRoles WHERE NormalizedName = N'ADMIN')
BEGIN
    IF NOT EXISTS (SELECT 1 FROM dbo.AspNetUsers)
        PRINT 'CHECK INFO: database trống, vai trò Admin sẽ được tạo ở lần chạy app Development đầu tiên (hoặc chạy bootstrap bên dưới).';
    ELSE
        PRINT 'CHECK FAIL: đã có tài khoản nhưng chưa có vai trò Admin.';
END
ELSE IF (SELECT COUNT(*) FROM dbo.AspNetUserRoles ur
         JOIN dbo.AspNetRoles r ON r.Id = ur.RoleId
         WHERE r.NormalizedName = N'ADMIN') = 0
    PRINT 'CHECK FAIL: có vai trò Admin nhưng chưa gán cho tài khoản nào.';
ELSE
    PRINT 'CHECK OK: có vai trò Admin và ít nhất một quản trị viên';

PRINT '--- 9. Nội dung dùng chung vs. nội dung riêng ---';
SELECT
    (SELECT COUNT(*) FROM dbo.Subjects WHERE OwnerId IS NULL)      AS SharedSubjects,
    (SELECT COUNT(*) FROM dbo.Subjects WHERE OwnerId IS NOT NULL)  AS OwnedSubjects,
    (SELECT COUNT(*) FROM dbo.SubjectFolders WHERE OwnerId IS NULL)     AS SharedFolders,
    (SELECT COUNT(*) FROM dbo.SubjectFolders WHERE OwnerId IS NOT NULL) AS OwnedFolders;

PRINT '--- 10. Bài làm chưa gán chủ sở hữu (dữ liệu cũ trước khi có đăng nhập) ---';
SELECT COUNT(*) AS AttemptsWithoutOwner FROM dbo.Attempts WHERE OwnerId IS NULL;
PRINT 'CHECK INFO: bài làm cũ chưa có OwnerId vẫn giữ nguyên nhưng không hiển thị cho tài khoản nào.';
PRINT 'CHECK INFO: nếu database mới tinh, số dòng trên phải bằng 0.';

PRINT '--- Hoàn tất kiểm tra ---';
