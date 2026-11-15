# Bàn giao — Đăng nhập, phân quyền và quản lý tài khoản

Tài liệu này dành cho agent/đồng đội tiếp nhận nhánh `feature/login-authorization`.
Đọc hết phần **Trạng thái** và **Việc còn lại** trước khi sửa code.

- Nhánh: `feature/login-authorization` (đã push lên `origin`)
- Commit mới nhất: `8b35d5e` — `feat(auth): thêm đăng nhập, phân quyền và quản lý tài khoản`
- Base: `eb779ea` (nhánh `flash-card`, gộp từ `main`)
- Build: web + test project đều `0 warning, 0 error`
- Working tree sạch, `output/` đã nằm trong `.gitignore`

> **Cập nhật sau tích hợp 15/11/2026 (nhánh `integration/auth-documents`):**
> tài liệu này ghi lại bản bàn giao gốc của `feature/login-authorization`. Khi gộp vào
> workspace hiện tại, các điểm sau đã thay đổi:
> - Admin **chỉ quản lý tài khoản**; không xem thư mục, môn học hay tài liệu riêng của
>   người khác. Trang chi tiết tài khoản chỉ hiện số liệu tổng hợp.
> - Mọi nội dung do tài khoản tạo — kể cả tài khoản Admin — là **riêng tư**. Chỉ dữ liệu
>   có `OwnerId = NULL` mới là nội dung dùng chung.
> - Flashcard được **giữ lại** trong bản tích hợp (nhánh gốc đã xóa).
> - Thêm tài liệu riêng (`Documents`) với migration `20261115155351_AddPrivateDocuments`.
> - Bộ kiểm thử hiện có **11 nhóm** integration và **19 kiểm tra** HTTP auth.
> - Trang đăng nhập **không còn** hiển thị tài khoản thử/mật khẩu ở bất kỳ môi trường nào;
>   đã có trang **đăng ký công khai** cho người học (`/Account/Register`).
> - Xóa tài khoản: tài liệu riêng và tệp vật lý bị xóa; thư mục/môn học chuyển về
>   `OwnerId = NULL`.

---

## 1. Yêu cầu nghiệp vụ đã chốt

1. Đăng nhập bằng ASP.NET Core Identity, phân quyền **Admin** và **Người học**.
2. Admin xem được **danh sách người dùng** kèm **thư mục và môn học từng người đã tạo**.
3. Admin quản lý tài khoản: tạo, đặt lại mật khẩu, khóa/mở khóa, cấp/thu hồi quyền Admin, xóa.
4. **Bỏ toàn bộ chức năng Flashcard** khỏi UI (không chỉ ẩn menu).

---

## 2. Kiến trúc đã triển khai

### Xác thực
| Thành phần | Vị trí |
| --- | --- |
| User model | `Models/ApplicationUser.cs` (`IdentityUser` + `DisplayName`, `CreatedAt`, `NameOrEmail`) |
| DbContext | `Data/AppDbContext.cs` kế thừa `IdentityDbContext<ApplicationUser>` |
| Cấu hình Identity + cookie + conventions | `Program.cs` |
| Claims | `ApplicationUserClaimsFactory` (thêm claim `"DisplayName"` để layout không cần query DB) |
| Bootstrap admin | `AdminBootstrap.EnsureAdminAsync` — chạy ở mọi môi trường nhưng chỉ đọc `BootstrapAdmin:Email`/`Password` (Development: user secrets, Production: biến môi trường); thiếu cấu hình thì chỉ log cảnh báo |

Conventions trong `Program.cs`:

```csharp
options.Conventions.AuthorizeFolder("/");
options.Conventions.AllowAnonymousToFolder("/Account");
options.Conventions.AllowAnonymousToPage("/Error");
options.Conventions.AuthorizePage("/Admin/Accounts", Security.AdminPolicy);
```

> Lưu ý: chỉ trang `/Admin/Accounts` gắn policy Admin. Các trang `/Admin/*` khác (quản lý nội dung) **mở cho mọi tài khoản đã đăng nhập**, nhưng mỗi người chỉ thấy và sửa nội dung do mình tạo. Đây là chủ ý, không phải thiếu sót — nếu muốn chặn hẳn người học khỏi `/Admin/*` thì phải đổi yêu cầu nghiệp vụ.

### Danh tính & chủ sở hữu
| Thành phần | Vị trí |
| --- | --- |
| Danh tính người dùng hiện tại | `Services/CurrentUser.cs` — **luôn suy ra từ `HttpContext.User`, không bao giờ từ form** |
| Quy tắc hiển thị nội dung | `Services/ContentVisibility.cs` (`VisibleSubjects`, `VisibleFolders`) |
| Quản trị tài khoản | `Services/AccountAdminService.cs` |
| Lọc theo người dùng | `Services/QuizService.cs`, `Services/ImportService.cs`, các `Pages/Admin/*` |

Quy tắc hiển thị nội dung (quan trọng, đừng phá):

- `OwnerId == null` → **nội dung dùng chung** (dữ liệu seed, nội dung cũ) → mọi tài khoản luyện được.
- Mọi nội dung do tài khoản tạo (kể cả Admin) → chỉ chủ sở hữu thấy; Admin không có quyền xem thay.
- Bài làm, lịch sử, câu sai, thống kê, tài liệu → **luôn** lọc `OwnerId == _currentUser.UserId`.

### Trang đã thêm
- `Pages/Account/Login.cshtml(.cs)` — đăng nhập, chống open redirect, thông báo chung khi sai. Không hiển thị tài khoản hay mật khẩu thử ở bất kỳ môi trường nào.
- `Pages/Account/Register.cshtml(.cs)` — đăng ký công khai cho người học: tên hiển thị, email, mật khẩu + nhập lại. Tài khoản tạo ra luôn là người học (không có trường vai trò nào được bind), email được trim + lowercase và kiểm tra trùng với thông báo trung tính; dùng chung `UserManager`/password policy với luồng đặt lại mật khẩu. Thành công → chuyển về `/Account/Login` kèm thông báo.
- `Pages/Account/Logout.cshtml(.cs)` — `OnGet` chuyển về `/Index` (không đăng xuất); `OnPost` mới đăng xuất.
- `Pages/Account/Denied.cshtml(.cs)` — trang từ chối truy cập.
- `Pages/Admin/Accounts.cshtml(.cs)` — danh sách + tìm kiếm + tạo tài khoản.
- `Pages/Admin/AccountDetail.cshtml(.cs)` — chi tiết, đổi vai trò, khóa, reset mật khẩu, xóa; chỉ hiện số liệu tổng hợp, không hiện nội dung riêng.
- `Pages/Shared/_AuthLayout.cshtml`, `wwwroot/css/auth.css`, `wwwroot/js/auth.js`.

### Bảo vệ nghiệp vụ trong `AccountAdminService`
- Không tự khóa / tự xóa tài khoản đang đăng nhập.
- Không thu hồi quyền / khóa / xóa **quản trị viên cuối cùng**.
- Xóa tài khoản: chuyển `SubjectFolders.OwnerId` và `Subjects.OwnerId` về `NULL` (giữ lại nội dung làm dùng chung) **trước khi** xóa user; bài làm và tài liệu riêng bị `CASCADE`, tệp tài liệu được xóa khỏi ổ đĩa.

---

## 3. Database

Migration: `20261114025237_AddIdentityAccountsAndOwnership` và
`20261115155351_AddPrivateDocuments`.

- Tạo 7 bảng `AspNet*`.
- `Subjects`, `SubjectFolders`: `OwnerId` (FK → `AspNetUsers`, `SET NULL`) + `CreatedAt`.
- `Attempts`: `OwnerId` (FK → `AspNetUsers`, `CASCADE`).
- `Documents`: `OwnerId` (FK → `AspNetUsers`, `CASCADE`), `SubjectId`
  (FK → `Subjects`, `SET NULL`), `RowVersion` cho cập nhật đồng thời.
- Bỏ unique index cũ `IX_SubjectFolders_Name`; thay bằng unique có lọc `IX_SubjectFolders_OwnerId_Name` trên `(OwnerId, Name)`.

Tệp bàn giao trong `database/`:

| Tệp | Mục đích |
| --- | --- |
| `README.md` | Quy trình bàn giao: xác định server/database, backup, áp dụng, verify, tạo admin đầu tiên |
| `TASK-LOG.md` | Nhật ký thay đổi schema, ảnh hưởng dữ liệu cũ, việc cần làm khi merge |
| `update-database.sql` | Script idempotent do `dotnet ef migrations script --idempotent` sinh (đủ 5 migration) |
| `verify-database.sql` | Script chỉ đọc: kiểm tra migration, bảng, cột, FK, index, đếm dòng |

Sau mỗi lần đổi schema phải sinh lại:

```powershell
dotnet ef migrations script --idempotent --project TracNghiemWeb.csproj --output database/update-database.sql
```

---

## 4. Cách chạy & kiểm thử

### Chạy local
```powershell
.\scripts\start-local.ps1            # mặc định localhost / RenshuuUiDev, cổng 5186
```

`Properties/launchSettings.json` dùng cổng **5080**. Dev tự chạy migration và seed khi database mới.

### Tạo quản trị viên đầu tiên (từ cấu hình)
```powershell
# Development: user secrets
dotnet user-secrets set "BootstrapAdmin:Email" "admin@ten-mien-cua-ban"
dotnet user-secrets set "BootstrapAdmin:Password" "<mật-khẩu-mạnh-của-bạn>"

# Production/máy chủ khác: biến môi trường (không ghi vào file cấu hình)
$env:BootstrapAdmin__Email = 'admin@ten-mien-cua-ban'
$env:BootstrapAdmin__Password = '<mật-khẩu-mạnh-của-bạn>'
```

Hành vi: email đã tồn tại → **chỉ gán** vai trò Admin; email chưa tồn tại + có mật khẩu → tạo mới. Thiếu cấu hình → **không tạo** tài khoản nào, chỉ ghi log cảnh báo (Development hướng dẫn `dotnet user-secrets`, Production hướng dẫn biến môi trường). Khoá cũ `DevelopmentAdmin:Email`/`DevelopmentAdmin:Password`/`DevelopmentAdmin:LoginPassword` vẫn được đọc để không phá cấu hình local cũ; `DevelopmentAdmin:ShowLoginCredentials` đã bị bỏ. Không có mật khẩu mặc định trong code.

### Kiểm thử
```powershell
# 1) Integration SQL — cần SQL Server, tự tạo/xóa database tạm RenshuuRegression_*
dotnet run --project tests/Renshuu.IntegrationTests/Renshuu.IntegrationTests.csproj -- "localhost"
# Kỳ vọng: 11 dòng PASS + "All 11 SQL integration groups passed."

# 2) HTTP auth — app phải đang chạy; thông tin quản trị lấy từ biến môi trường
$env:RENSHUU_ADMIN_EMAIL = 'admin@...'
$env:RENSHUU_ADMIN_PASSWORD = '<mật-khẩu-quản-trị>'
.\scripts\verify-auth.ps1 -BaseUrl 'http://localhost:5080'
# Kỳ vọng: 19 PASS, script exit 0
```

Kết quả lần chạy cuối: **11/11** nhóm integration, **19/19** kiểm tra HTTP, kiểm thử
đăng ký công khai qua HTTP đều đạt.

---

## 5. Trạng thái xác minh

| Hạng mục | Trạng thái | Ghi chú |
| --- | --- | --- |
| Build web + test | PASS | 0 warning, 0 error |
| Migration trên DB trống | PASS | chạy trên `RenshuuVerify_Check`, sau đó đã xóa DB |
| `verify-database.sql` | PASS | tất cả `CHECK OK`; `CHECK INFO` về vai trò Admin khi DB trống là bình thường |
| Integration SQL 11 nhóm | PASS | migration, thứ tự nộp, nháp, deadline + grace, concurrent, giới hạn 200 câu, import, analytics, quyền riêng tư nội dung, đăng ký công khai |
| Auth HTTP 19 kiểm tra | PASS | ẩn danh, sai mật khẩu, ReturnUrl, admin, logout POST-only, learner bị chặn |
| Tài liệu riêng qua HTTP | PASS (thủ công) | tạo user → tải/xem/sửa/xóa TXT; admin nhận 404 với tài liệu và thư mục của user |
| Admin bị chặn nội dung người dùng | PASS | trang chi tiết chỉ hiện số liệu; không có danh sách thư mục/môn học/tài liệu riêng |
| Đăng ký công khai | PASS | tạo tài khoản người học, email trùng bị từ chối, mật khẩu yếu bị từ chối, gửi kèm `IsAdmin/Role` bị bỏ qua, tài khoản mới đăng nhập được và bị chặn khỏi `/Admin` |
| **Chưa xác minh** | — | đổi mật khẩu tự phục vụ, quên mật khẩu, responsive mobile của trang auth/tài liệu, seed dữ liệu trên DB thật của team |

---

## 6. Việc còn lại / rủi ro đã biết

1. **Chưa có đổi mật khẩu tự phục vụ.** Admin reset mật khẩu hộ; người dùng không tự đổi được. Nếu cần, thêm trang `Pages/Account/ChangePassword` và bật `AddDefaultTokenProviders` (đã có sẵn).
2. **Chưa có quên mật khẩu / email.** Chưa cấu hình SMTP; `SignIn.RequireConfirmedAccount = false`.
3. **Đăng ký công khai đã có** (`/Account/Register`), tài khoản mới luôn là người học. Chống spam hiện dùng rate limiting theo IP (20 lần/5 phút cho POST `/Account/Login` và `/Account/Register`); nếu cần chặt hơn, cân nhắc CAPTCHA hoặc xác nhận email.
4. **`/Admin/*` (nội dung) mở cho mọi tài khoản đã đăng nhập**, chỉ giới hạn theo `OwnerId`. Cân nhắc lại nếu muốn chỉ Admin được vào toàn bộ khu vực quản trị.
5. **Bài làm cũ (`OwnerId = NULL`) không hiển thị cho ai.** Nếu cần giữ, gán thủ công — câu SQL mẫu nằm trong `database/TASK-LOG.md`.
6. **Chưa kiểm thử mobile/responsive** cho `Login`/`Denied`/`Accounts`. Có sẵn `scripts/verify-experience.cjs` và `verify-study-flow.cjs` (Playwright) để mở rộng.
7. **Chưa có test tự động cho `AccountAdminService`** (chặn admin cuối cùng, tự khóa, chuyển `OwnerId` khi xóa). Đây là phần nên bổ sung đầu tiên.
8. **`TracNghiemWeb.slnx`** có thể cần thêm project test nếu muốn `dotnet test` trên solution.

---

## 7. Gợi ý thứ tự việc tiếp theo

1. Viết integration test cho `AccountAdminService` (dùng SQL Server như bộ test hiện có).
2. Thêm trang đổi mật khẩu tự phục vụ cho người dùng.
3. Kiểm tra responsive trang auth trên 375px, 768px, 1024px.
4. Chạy `database/verify-database.sql` trên database thật của team và ghi lại kết quả vào `database/TASK-LOG.md`.
5. Nếu cần, mở PR từ `feature/login-authorization` vào `main` và nhờ review diff.

---

## 8. Lệnh nhanh

```powershell
git switch feature/login-authorization
git pull origin feature/login-authorization
dotnet build --nologo
dotnet run --project TracNghiemWeb.csproj
```

Đọc thêm: [database/README.md](../database/README.md), [database/TASK-LOG.md](../database/TASK-LOG.md), [README.md](../README.md).
