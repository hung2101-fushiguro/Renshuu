using System.IO.Compression;
using System.Text;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging.Abstractions;
using TracNghiemWeb.Data;
using TracNghiemWeb.Models;
using TracNghiemWeb.Services;
using TracNghiemWeb.Services.Documents;
using TracNghiemWeb.Services.Security;

static class AccessAndDocumentChecks
{
    static void Check(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
    static FormFile File(string name, string content) => new(new MemoryStream(Encoding.UTF8.GetBytes(content)), 0, Encoding.UTF8.GetByteCount(content), "Upload", name);
    static FormFile Office(string name, string xml)
    {
        var stream = new MemoryStream();
        using (var zip = new ZipArchive(stream, ZipArchiveMode.Create, true))
        {
            using (var writer = new StreamWriter(zip.CreateEntry("[Content_Types].xml").Open())) writer.Write("<Types/>");
            using (var writer = new StreamWriter(zip.CreateEntry("word/document.xml").Open())) writer.Write(xml);
        }
        stream.Position = 0;
        return new(stream, 0, stream.Length, "Upload", name);
    }

    public static async Task RunAsync(DbContextOptions<AppDbContext> options, TimeProvider time, ICurrentUser owner)
    {
        AppDbContext Db() => new(options);
        var other = new TestUser { Id = "regression-other" };
        var admin = new TestUser { Id = "regression-admin", IsAdmin = true };
        await using (var db = Db())
        {
            db.Users.AddRange(new ApplicationUser { Id = other.Id!, UserName = "other@example.test", Email = "other@example.test", DisplayName = "Other" },
                new ApplicationUser { Id = admin.Id!, UserName = "admin@example.test", Email = "admin@example.test", DisplayName = "Admin" });
            await db.SaveChangesAsync();
        }
        async Task<T> Quiz<T>(ICurrentUser actor, Func<QuizService, Task<T>> operation)
        { await using var db = Db(); return await operation(new QuizService(db, time, actor)); }
        int subject, question;
        await using (var db = Db()) { subject = await db.Subjects.OrderBy(s => s.Id).Select(s => s.Id).FirstAsync(); question = await db.Questions.Where(q => q.SubjectId == subject).Select(q => q.Id).FirstAsync(); }
        var attempt = (await Quiz(owner, q => q.StartAsync(subject, 1, null))).AttemptId!.Value;
        var answer = new Dictionary<int, string?> { [question] = "B" };
        Check((await Quiz(owner, q => q.SaveDraftAsync(attempt, answer))).Success, "Owner draft failed");
        foreach (var actor in new ICurrentUser[] { other, admin, new TestUser { Id = null } })
        {
            Check(await Quiz(actor, q => q.GetAttemptForResultAsync(attempt)) is null, "Foreign attempt read");
            Check(!(await Quiz(actor, q => q.SaveDraftAsync(attempt, answer))).Success, "Foreign draft write");
            Check(!(await Quiz(actor, q => q.SubmitAsync(attempt, answer))).Success, "Foreign submission");
            Check(!await Quiz(actor, q => q.DeleteAttemptAsync(attempt)), "Foreign delete");
        }
        await Quiz(owner, q => q.SubmitAsync(attempt, answer));
        Check((await Quiz(other, q => q.GetWrongQuestionsAsync(subject))).Count == 0, "Wrong review crossed users");
        Check((await Quiz(owner, q => q.GetAnalyticsAsync())).TotalAttempts > 0, "Owner analytics missing");
        foreach (var actor in new ICurrentUser[] { other, admin, new TestUser { Id = null } })
        {
            Check((await Quiz(actor, q => q.GetAnalyticsAsync())).TotalAttempts == 0, "Analytics crossed users");
            Check((await Quiz(actor, q => q.GetAnalyticsAsync(subject))).TotalEverWrong == 0, "Subject analytics crossed users");
            Check((await Quiz(actor, q => q.GetPracticeSummaryAsync())).AttemptCount == 0, "Dashboard summary crossed users");
        }
        await Quiz(other, q => q.ClearHistoryAsync());
        Check(await Quiz(owner, q => q.GetAttemptForResultAsync(attempt)) is not null, "Clear history crossed users");
        Check(!(await Quiz(new TestUser { Id = null }, q => q.StartAsync(subject, 1, null))).Success, "Anonymous attempt creation");
        Console.WriteLine("PASS Attempts, drafts, grades, history and wrong review are isolated, including from admin");

        var root = Path.Combine(Path.GetTempPath(), "TracNghiemDocumentsTest_" + Guid.NewGuid().ToString("N"));
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["Documents:StoragePath"] = root }).Build();
        var storage = new DocumentStorage(config, new TestEnvironment(), NullLogger<DocumentStorage>.Instance);
        async Task<T> Documents<T>(ICurrentUser actor, Func<DocumentService, Task<T>> operation)
        { await using var db = Db(); return await operation(new DocumentService(db, actor, storage, time)); }
        try
        {
            var uploaded = await Documents(owner, d => d.UploadAsync(new("Tài liệu riêng", "Ghi chú", subject), File("../../lesson.txt", "Bài học tiếng Việt\n<script>alert(1)</script>")));
            Check(uploaded.Success, uploaded.Error ?? "Upload failed");
            var document = (await Documents(owner, d => d.FindAsync(uploaded.Id!.Value)))!;
            Check(document.OriginalFileName == "lesson.txt", "Original filename not normalized");
            Check((await storage.ReadPreviewAsync(document.StorageKey)).Text.Contains("tiếng Việt"), "Text preview lost Unicode");
            Check(await Documents(other, d => d.FindAsync(document.Id)) is null, "Foreign document read");
            Check(!(await Documents(other, d => d.UpdateAsync(document.Id, new("Stolen", null, null, Convert.ToBase64String(document.RowVersion)), null))).Success, "Foreign document edit");
            Check(!(await Documents(other, d => d.DeleteAsync(document.Id, Convert.ToBase64String(document.RowVersion)))).Success, "Foreign document delete");
            Check((await Documents(other, d => d.List(true).CountAsync())) == 0, "All scope granted to non-admin");
            Check(await Documents(admin, d => d.FindAsync(document.Id)) is not null, "Admin cannot inspect upload");
            var firstVersion = Convert.ToBase64String(document.RowVersion);
            Check((await Documents(admin, d => d.UpdateAsync(document.Id, new("Admin sửa", "Nội dung mới", subject, firstVersion), File("replacement.txt", "Nội dung thay thế")))).Success, "Admin replacement failed");
            var updated = (await Documents(owner, d => d.FindAsync(document.Id)))!;
            Check(updated.OwnerId == owner.Id && updated.Title == "Admin sửa", "Admin edit changed owner");
            Check(!System.IO.File.Exists(Path.Combine(root, document.StorageKey)), "Old blob was not removed after replacement");
            Check((await storage.ReadPreviewAsync(updated.StorageKey)).Text == "Nội dung thay thế", "Replacement content mismatch");
            Check((await Documents(admin, d => d.UpdateAsync(document.Id, new("Admin sửa TXT", null, subject, Convert.ToBase64String(updated.RowVersion), "Sửa trực tiếp tiếng Việt"), null))).Success, "Inline TXT edit failed");
            updated = (await Documents(owner, d => d.FindAsync(document.Id)))!;
            Check((await storage.ReadPreviewAsync(updated.StorageKey)).Text == "Sửa trực tiếp tiếng Việt", "Inline TXT content mismatch");
            Check(!(await Documents(owner, d => d.UpdateAsync(document.Id, new("Stale", null, subject, firstVersion), null))).Success, "Stale edit overwrote newer document");
            Check(!(await Documents(owner, d => d.DeleteAsync(document.Id, firstVersion))).Success, "Stale delete removed newer document");
            Check(!(await Documents(owner, d => d.UploadAsync(new("Invalid", null, null), File("bad.pdf", "not PDF")))).Success, "False PDF accepted");
            Check(!(await Documents(owner, d => d.UploadAsync(new("Invalid", null, null), File("page.html", "<html>")))).Success, "HTML accepted");
            var large = new FormFile(new MemoryStream([1]), 0, DocumentStorage.MaxFileBytes + 1, "Upload", "large.txt");
            Check(!(await Documents(owner, d => d.UploadAsync(new("Large", null, null), large))).Success, "Oversized upload accepted");
            var xml = "<w:document xmlns:w='http://schemas.openxmlformats.org/wordprocessingml/2006/main'><w:body><w:p><w:r><w:t>Văn bản DOCX</w:t></w:r></w:p></w:body></w:document>";
            var office = await Documents(owner, d => d.UploadAsync(new("DOCX", null, subject), Office("lesson.docx", xml)));
            Check(office.Success, "DOCX rejected");
            var officeDocument = (await Documents(owner, d => d.FindAsync(office.Id!.Value)))!;
            Check((await storage.ReadPreviewAsync(officeDocument.StorageKey)).Text.Contains("Văn bản DOCX"), "DOCX preview missing");
            Check(!(await Documents(owner, d => d.UploadAsync(new("DTD", null, null), Office("evil.docx", "<!DOCTYPE x [<!ENTITY x SYSTEM 'file:///C:/Windows/win.ini'>]><x>&x;</x>")))).Success, "DTD accepted");
            var rejected = false;
            try { using var ignored = storage.Open("../outside.txt"); } catch (InvalidOperationException) { rejected = true; }
            Check(rejected, "Storage path traversal accepted");
            Check((await Documents(admin, d => d.DeleteAsync(updated.Id, Convert.ToBase64String(updated.RowVersion)))).Success, "Admin delete failed");
            Check(!System.IO.File.Exists(Path.Combine(root, updated.StorageKey)), "Deleted blob retained");
            Check(await Documents(owner, d => d.FindAsync(updated.Id)) is null, "Deleted document remains accessible");
            Console.WriteLine("PASS Private files, owner/admin CRUD, replacement, Unicode previews, rowversion and upload validation");
        }
        finally
        {
            var resolved = Path.GetFullPath(root);
            if (Path.GetDirectoryName(resolved) == Path.GetFullPath(Path.GetTempPath()).TrimEnd(Path.DirectorySeparatorChar) && Path.GetFileName(resolved).StartsWith("TracNghiemDocumentsTest_")) Directory.Delete(resolved, true);
        }

        var adminActor = new TestUser { Id = admin.Id, IsAdmin = true };
        string connectionString;
        await using (var db = Db()) connectionString = db.Database.GetConnectionString()!;
        ServiceProvider Provider(ICurrentUser actor)
        {
            var services = new ServiceCollection();
            services.AddLogging(); services.AddDbContext<AppDbContext>(o => o.UseSqlServer(connectionString));
            services.AddDataProtection().UseEphemeralDataProtectionProvider();
            services.AddIdentityCore<ApplicationUser>(o => { o.User.RequireUniqueEmail = true; o.Password.RequiredLength = 10; o.Password.RequireNonAlphanumeric = false; })
                .AddRoles<IdentityRole>().AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();
            services.AddScoped<ICurrentUser>(_ => actor); services.AddScoped<UserAdministrationService>();
            return services.BuildServiceProvider();
        }
        using var provider = Provider(adminActor);
        using (var scope = provider.CreateScope())
        {
            await AccountBootstrap.EnsureRolesAsync(scope.ServiceProvider);
            var manager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var firstAdmin = await manager.FindByIdAsync(admin.Id!);
            Check((await manager.AddToRoleAsync(firstAdmin!, AppRoles.Admin)).Succeeded, "Initial admin role failed");
        }
        async Task<T> Administration<T>(Func<UserAdministrationService, Task<T>> operation)
        { using var scope = provider.CreateScope(); return await operation(scope.ServiceProvider.GetRequiredService<UserAdministrationService>()); }
        const string password = "Integration9Password!";
        Check((await Administration(s => s.CreateAsync(new("Managed", "managed@example.test", AppRoles.User, true, password, null)))).Success, "Admin create failed");
        ApplicationUser managed;
        await using (var db = Db()) { managed = await db.Users.SingleAsync(u => u.Email == "managed@example.test"); }
        var stamp = managed.SecurityStamp;
        Check((await Administration(s => s.UpdateAsync(managed.Id, new("Changed", managed.Email!, AppRoles.User, false, null, managed.ConcurrencyStamp)))).Success, "Admin edit failed");
        await using (var db = Db())
        {
            managed = await db.Users.AsNoTracking().SingleAsync(u => u.Id == managed.Id);
            Check(!managed.IsActive && managed.SecurityStamp != stamp, "Disabled account did not invalidate cookie stamp");
        }
        Check(!(await Administration(s => s.UpdateAsync(managed.Id, new("Stale", managed.Email!, AppRoles.User, true, null, "invalid")))).Success, "Stale user update succeeded");
        Check((await Administration(s => s.DeleteAsync(managed.Id, managed.ConcurrencyStamp))).Success, "Admin delete failed");
        await using (var db = Db()) Check((await db.Users.SingleAsync(u => u.Id == managed.Id)).IsDeleted, "Account ownership record removed");
        ApplicationUser originalAdmin;
        await using (var db = Db()) originalAdmin = await db.Users.AsNoTracking().SingleAsync(u => u.Id == admin.Id);
        Check(!(await Administration(s => s.UpdateAsync(originalAdmin.Id, new(originalAdmin.DisplayName, originalAdmin.Email!, AppRoles.User, true, null, originalAdmin.ConcurrencyStamp)))).Success, "Self demotion accepted");
        // Simulate an authorized request that began before a different admin was deactivated.
        adminActor.Id = "in-flight-admin-request";
        Check(!(await Administration(s => s.DeleteAsync(originalAdmin.Id, originalAdmin.ConcurrencyStamp))).Success, "Last active admin deleted");
        adminActor.IsAdmin = false;
        Check(!(await Administration(s => s.CreateAsync(new("Unauthorized", "bad@example.test", AppRoles.Admin, true, password, null)))).Success, "User created admin through service");
        Console.WriteLine("PASS User administration enforces role, stale edits, cookie invalidation, soft deletion and last-admin rules");
    }

    sealed class TestEnvironment : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "TracNghiemWeb.Tests";
        public string EnvironmentName { get; set; } = "Development";
        public string ContentRootPath { get; set; } = Directory.GetCurrentDirectory();
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
        public string WebRootPath { get; set; } = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
    }
}
