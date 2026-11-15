using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TracNghiemWeb.Data;
using TracNghiemWeb.Services.Security;

namespace TracNghiemWeb.Pages.Administration.Users;

public class IndexModel(AppDbContext db, ICurrentUser current) : PageModel
{
    [BindProperty(SupportsGet = true)] public string? Q { get; set; }
    [BindProperty(SupportsGet = true)] public string? Role { get; set; }
    [BindProperty(SupportsGet = true)] public string? State { get; set; }
    [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;
    public int Total { get; private set; }
    public int PageCount => Math.Max(1, (Total + 19) / 20);
    public string? CurrentId => current.Id;
    public List<UserRow> Items { get; private set; } = new();
    public async Task OnGetAsync()
    {
        Q = Q?.Trim(); if (Q?.Length > 200) Q = Q[..200];
        var query = db.Users.AsNoTracking().Where(u => !u.IsDeleted);
        if (!string.IsNullOrEmpty(Q)) query = query.Where(u => u.DisplayName.Contains(Q) || u.Email!.Contains(Q));
        if (State == "active") query = query.Where(u => u.IsActive);
        else if (State == "inactive") query = query.Where(u => !u.IsActive);
        if (Role is AppRoles.Admin or AppRoles.User)
            query = query.Where(u => db.UserRoles.Any(m => m.UserId == u.Id && db.Roles.Any(r => r.Id == m.RoleId && r.Name == Role)));
        Total = await query.CountAsync();
        PageNumber = Math.Clamp(PageNumber, 1, PageCount);
        Items = await query.OrderByDescending(u => u.CreatedAtUtc).ThenBy(u => u.Id).Skip((PageNumber - 1) * 20).Take(20)
            .Select(u => new UserRow(u.Id, u.DisplayName, u.Email ?? "", u.IsActive, u.CreatedAtUtc,
                db.UserRoles.Where(m => m.UserId == u.Id).Join(db.Roles, m => m.RoleId, r => r.Id, (m, r) => r.Name).OrderBy(r => r).FirstOrDefault() ?? AppRoles.User,
                db.Documents.Count(d => d.OwnerId == u.Id), u.LockoutEnd)).ToListAsync();
    }
}
public record UserRow(string Id, string Name, string Email, bool Active, DateTime CreatedUtc, string Role, int Documents, DateTimeOffset? LockoutEnd);
