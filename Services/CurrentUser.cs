using System.Security.Claims;

namespace TracNghiemWeb.Services;

/// <summary>
/// Identity of the signed-in account for the current request.
/// Ownership is always derived from the authenticated principal, never from form data.
/// </summary>
public class CurrentUser
{
    public const string AdminRole = "Admin";

    private readonly IHttpContextAccessor _accessor;

    public CurrentUser(IHttpContextAccessor accessor) => _accessor = accessor;

    private ClaimsPrincipal? Principal => _accessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true;

    public string? UserId => IsAuthenticated ? Principal!.FindFirstValue(ClaimTypes.NameIdentifier) : null;

    public string? UserName => IsAuthenticated ? Principal!.Identity?.Name : null;

    public bool IsAdmin => Principal?.IsInRole(AdminRole) == true;

    public string DisplayName =>
        Principal?.FindFirstValue("DisplayName") is { Length: > 0 } name ? name : UserName ?? "Người dùng";
}
