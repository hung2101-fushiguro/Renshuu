# Chạy chức năng tài khoản

Chạy bằng PowerShell tại thư mục dự án:

```powershell
$env:ConnectionStrings__DefaultConnection='Server=localhost;Database=TracNghiemWeb;Trusted_Connection=True;TrustServerCertificate=True'
dotnet run --project TracNghiemWeb.csproj
```

Development startup áp dụng các migration còn thiếu. AddIdentityAccounts thêm bảng Identity và UserId nullable vào Attempts. Bài cũ được giữ nguyên nhưng không hiển thị trong tài khoản mới. AddPracticeMode thêm IsPractice, mặc định false cho các bài cũ.

Truy cập http://localhost:5080/Account/Login và đăng ký tài khoản. Mật khẩu cần ít nhất 8 ký tự, gồm chữ hoa, chữ thường, số và ký tự đặc biệt.

Để cấp quyền quản trị cho tài khoản đã đăng ký trên máy phát triển:

```powershell
dotnet user-secrets set "DevelopmentAdmin:Email" "email-ban-da-dang-ky"
```

Khởi động lại ứng dụng, đăng xuất rồi đăng nhập lại. Sau khi cấp quyền, có thể xóa cấu hình bằng `dotnet user-secrets remove "DevelopmentAdmin:Email"`; quyền đã cấp vẫn tồn tại. Thiết lập này chỉ hoạt động trong Development. Không có mật khẩu mặc định.

## Ôn tập và làm bài

Chọn chế độ ở trang bắt đầu. Ôn tập cho phép nhấn S hoặc bấm nút để hiện/ẩn đáp án dưới từng câu. Làm bài chỉ hiển thị đáp án sau khi nộp. Luồng làm lại câu sai luôn dùng chế độ Ôn tập. Chế độ được lưu theo từng lượt làm và hiển thị trong lịch sử/kết quả.

## Hiển thị tài khoản thử trên máy phát triển

Để hiện hai dòng Email/Mật khẩu dưới nút đăng nhập, đặt thêm user secrets `DevelopmentAdmin:ShowLoginCredentials` thành `true` và `DevelopmentAdmin:LoginPassword` thành mật khẩu của tài khoản đã đăng ký. Email lấy từ `DevelopmentAdmin:Email`. Cấu hình này chỉ đổi nội dung hiển thị, không đặt lại mật khẩu tài khoản. Chỉ dùng tài khoản dành riêng cho nhóm thử nghiệm.

Hai dòng này chỉ được render trong Development. Tắt bằng `dotnet user-secrets set "DevelopmentAdmin:ShowLoginCredentials" "false"`. User secrets và mật khẩu local không đi cùng nhánh Git; mỗi máy cần cấu hình riêng.

## Kiểm thử

Các lần kiểm thử local dùng database riêng: TracNghiemWeb_LoginCheck (HTTP) và TracNghiemWeb_Ui_20261113 (Chrome/Playwright). Báo cáo và ảnh chụp nằm trong output/auth-ui trên máy đã chạy kiểm thử; output được bỏ qua bởi Git. Repository chưa có test project .NET; GitHub Actions hiện chạy restore và build.

Chưa cấu hình email khôi phục mật khẩu hoặc đăng nhập Google.
