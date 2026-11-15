using System.Security.Claims;

namespace TracNghiemWeb.Services.Security;

public static class AppRoles
{
    public const string User = "User";
    public const string Admin = "Admin";
}

public interface ICurrentUser
{
    string? Id { get; }
    bool IsAdmin { get; }
}

public sealed class CurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    public string? Id => accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
    public bool IsAdmin => accessor.HttpContext?.User.IsInRole(AppRoles.Admin) == true;
}
