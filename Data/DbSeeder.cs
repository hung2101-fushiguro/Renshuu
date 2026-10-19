using Microsoft.EntityFrameworkCore;
using TracNghiemWeb.Models;

namespace TracNghiemWeb.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.Subjects.AnyAsync())
        {
            return;
        }

        var csharp = new Subject { Name = "Lập trình C# cơ bản" };
        var aspnet = new Subject { Name = "ASP.NET Core" };

        csharp.Questions = new List<Question>
        {
            New("Kiểu dữ liệu nào dùng để lưu số nguyên 64-bit trong C#?", "int", "long", "float", "decimal", "B",
                "long là kiểu số nguyên 64-bit; int chỉ 32-bit."),
            New("Từ khóa nào dùng để khai báo một hằng số trong C#?", "const", "static", "readonly", "final", "A",
                "const khai báo hằng số biên dịch, giá trị không đổi sau khi khai báo."),
            New("Phương thức nào là điểm bắt đầu của một chương trình C#?", "start()", "Main()", "Run()", "Program()", "B",
                "CLR tìm phương thức Main để bắt đầu thực thi chương trình."),
            New("Câu lệnh nào thoát khỏi vòng lặp ngay lập tức?", "continue", "break", "return", "goto", "B",
                "break kết thúc vòng lặp hiện tại; continue chỉ bỏ qua lượt lặp hiện tại."),
            New("Kết quả của biểu thức 7 / 2 khi cả hai toán hạng là int?", "3.5", "3", "4", "Lỗi biên dịch", "B",
                "Phép chia hai số int là chia nguyên nên kết quả là 3."),
            New("Ký hiệu nào dùng để kế thừa lớp trong C#?", "extends", "inherits", "Dấu hai chấm (:)", "implements", "C",
                "C# dùng dấu hai chấm, ví dụ: class Dog : Animal."),
            New("Chỉ số đầu tiên của mảng trong C# là bao nhiêu?", "0", "1", "-1", "Tùy cấu hình", "A",
                "Mảng trong C# đánh chỉ số từ 0."),
            New("Cặp toán tử nào giúp xử lý null an toàn?", "?.", "??", "!", "Cả ?. và ??", "D",
                "?. tránh NullReferenceException, ?? cung cấp giá trị mặc định khi null."),
            New("List<T> nằm trong namespace nào?", "System.Collections.Generic", "System.Linq", "System.Text", "System.IO", "A",
                "List<T> là collection generic trong System.Collections.Generic."),
            New("Từ khóa nào dùng để bắt ngoại lệ trong C#?", "catch", "rescue", "except", "handle", "A",
                "Cấu trúc try/catch dùng để bắt và xử lý ngoại lệ."),
        };

        aspnet.Questions = new List<Question>
        {
            New("Middleware trong ASP.NET Core thường được cấu hình ở đâu?", "Program.cs", "appsettings.json", "web.config", "Startup.cs bắt buộc", "A",
                "Từ .NET 6, pipeline middleware được cấu hình trong Program.cs."),
            New("Razor Pages sử dụng file có phần mở rộng nào?", ".razor", ".cshtml", ".aspx", ".html", "B",
                "Razor Pages dùng file .cshtml kèm file .cshtml.cs (PageModel)."),
            New("Lifetime nào tạo một instance mới cho mỗi HTTP request?", "AddSingleton", "AddTransient mỗi request", "AddScoped", "AddStatic", "C",
                "AddScoped tạo một instance cho mỗi request; AddSingleton dùng chung toàn ứng dụng."),
            New("Tệp nào thường chứa chuỗi kết nối trong ứng dụng ASP.NET Core?", "appsettings.json", "Program.cs", "launchSettings.json", "global.json", "A",
                "Chuỗi kết nối nên đặt trong ConnectionStrings của appsettings.json hoặc user secrets."),
            New("Handler mặc định xử lý yêu cầu POST trong PageModel tên là gì?", "OnGet", "OnPost", "OnPostAsync", "Post", "C",
                "Razor Pages tự gọi OnPostAsync cho yêu cầu POST khi form được submit."),
            New("Lệnh nào tạo một migration mới với EF Core?", "dotnet ef migrations add <Tên>", "dotnet ef database create", "dotnet build --migrate", "ef add migration", "A",
                "dotnet ef migrations add tạo migration mới từ thay đổi model."),
            New("Kestrel trong ASP.NET Core là gì?", "Web server đa nền tảng tích hợp sẵn", "Cơ sở dữ liệu", "Template Razor", "Công cụ migration", "A",
                "Kestrel là web server mặc định, đa nền tảng của ASP.NET Core."),
            New("Thuộc tính nào gắn route tùy chỉnh cho một Razor Page?", "@page \"{id:int}\"", "[HttpGet]", "@route", "@model", "A",
                "Chỉ thị @page có thể nhận mẫu route tùy chỉnh."),
            New("Cách đúng để đăng ký DbContext với Dependency Injection?", "builder.Services.AddDbContext<AppDbContext>(...)", "new AppDbContext() trong mọi PageModel", "builder.AddDatabase(...)", "services.DbContext(...)", "A",
                "AddDbContext đăng ký DbContext với DI và cấu hình provider."),
            New("Thuộc tính [BindProperty] trong PageModel có tác dụng gì?", "Tự động bind dữ liệu form vào thuộc tính", "Đổi tên cột trong database", "Chặn truy cập trang", "Tạo route mới", "A",
                "[BindProperty] giúp model binding dữ liệu từ request vào thuộc tính của PageModel."),
        };

        db.Subjects.AddRange(csharp, aspnet);
        await db.SaveChangesAsync();
    }

    private static Question New(
        string content,
        string a, string b, string c, string d,
        string correct, string explanation) => new()
        {
            Content = content,
            OptionA = a,
            OptionB = b,
            OptionC = c,
            OptionD = d,
            CorrectOption = correct,
            Explanation = explanation,
        };
}
