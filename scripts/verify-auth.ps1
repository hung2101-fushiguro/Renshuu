param(
    [string]$BaseUrl = 'http://localhost:5191',
    [string]$Email = 'admin@renshuu.local',
    [string]$Password = 'Admin#2026pass'
)

$ErrorActionPreference = 'Stop'
$work = Join-Path $env:TEMP ("renshuu-auth-" + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $work | Out-Null
$cookies = Join-Path $work 'cookies.txt'
$bodyFile = Join-Path $work 'body.html'
$failures = New-Object System.Collections.Generic.List[string]

function Invoke-Curl {
    param([string[]]$Arguments)
    $output = & curl.exe -s -S @Arguments
    if ($LASTEXITCODE -ne 0) { throw "curl thất bại: $($Arguments -join ' ')" }
    return ($output -join "`n")
}

function New-Result {
    param([string]$Meta)
    $parts = $Meta.Trim() -split ' '
    $status = [int]$parts[0]
    $location = if ($parts.Count -gt 1) { $parts[1] } else { '' }
    if ($location) { $location = $location.Replace($BaseUrl, '') }
    $text = ''
    if (Test-Path $bodyFile) { $text = [System.IO.File]::ReadAllText($bodyFile, [System.Text.Encoding]::UTF8) }
    return @{ Status = $status; Location = $location; Body = $text }
}

function Get-Page {
    param([string]$Path)
    $meta = Invoke-Curl @('-o', $bodyFile, '-b', $cookies, '-c', $cookies,
        '-w', '%{http_code} %{redirect_url}', "$BaseUrl$Path")
    return (New-Result $meta)
}

function Get-Token {
    param([string]$Html)
    $match = [regex]::Match($Html, 'name="__RequestVerificationToken"[^>]*value="([^"]+)"')
    if (-not $match.Success) { throw 'Không tìm thấy antiforgery token trong form.' }
    return $match.Groups[1].Value
}

function Post-Form {
    param([string]$Path, [hashtable]$Fields)
    $arguments = @('-o', $bodyFile, '-b', $cookies, '-c', $cookies,
        '-w', '%{http_code} %{redirect_url}', '-X', 'POST', "$BaseUrl$Path")
    foreach ($key in $Fields.Keys) {
        $arguments += @('--data-urlencode', "$key=$($Fields[$key])")
    }
    $meta = Invoke-Curl $arguments
    return (New-Result $meta)
}

function Assert-True {
    param([bool]$Condition, [string]$Label)
    if ($Condition) { Write-Host "PASS  $Label" } else { Write-Host "FAIL  $Label"; $script:failures.Add($Label) }
}

try {
    # 1. Anonymous requests are redirected to the sign-in page.
    $anonymous = Get-Page '/'
    Assert-True ($anonymous.Status -eq 302 -and $anonymous.Location -like '/Account/Login*') 'Ẩn danh vào trang chủ bị chuyển tới đăng nhập'

    $anonymousAdmin = Get-Page '/Admin/Accounts'
    Assert-True ($anonymousAdmin.Status -eq 302 -and $anonymousAdmin.Location -like '/Account/Login*') 'Ẩn danh vào trang quản lý tài khoản bị chặn'

    # 2. Wrong password shows a generic error and stays on the form.
    $loginPage = Get-Page '/Account/Login'
    Assert-True ($loginPage.Status -eq 200) 'Trang đăng nhập tải được'
    $badLogin = Post-Form '/Account/Login' @{
        Email = $Email
        Password = 'SaiMatKhau#1'
        __RequestVerificationToken = (Get-Token $loginPage.Body)
    }
    Assert-True ($badLogin.Status -eq 200 -and $badLogin.Body -match 'validation-summary-errors') 'Sai mật khẩu bị từ chối với thông báo chung'

    # 3. A valid sign-in honours a local return URL.
    $loginPage = Get-Page '/Account/Login'
    $goodLogin = Post-Form '/Account/Login' @{
        Email = $Email
        Password = $Password
        ReturnUrl = '/Admin/Accounts'
        __RequestVerificationToken = (Get-Token $loginPage.Body)
    }
    Assert-True ($goodLogin.Status -eq 302 -and $goodLogin.Location -eq '/Admin/Accounts') 'Đăng nhập đúng chuyển tới ReturnUrl nội bộ'

    # 4. Admin pages load for the administrator.
    $accounts = Get-Page '/Admin/Accounts'
    Assert-True ($accounts.Status -eq 200 -and $accounts.Body -match 'Quản lý tài khoản') 'Quản trị viên xem được danh sách tài khoản'
    Assert-True ($accounts.Body -match 'admin@renshuu.local') 'Danh sách hiển thị tài khoản quản trị'

    $dashboard = Get-Page '/'
    Assert-True ($dashboard.Status -eq 200) 'Quản trị viên xem được không gian luyện tập'

    # 5. Logout is POST-only.
    $logoutGet = Get-Page '/Account/Logout'
    Assert-True ($logoutGet.Status -eq 302 -and $logoutGet.Location -eq '/') 'GET logout không đăng xuất'
    $stillIn = Get-Page '/Admin/Accounts'
    Assert-True ($stillIn.Status -eq 200) 'Sau GET logout phiên đăng nhập vẫn còn'

    $logout = Post-Form '/Account/Logout' @{
        __RequestVerificationToken = (Get-Token $stillIn.Body)
    }
    Assert-True ($logout.Status -eq 302 -and $logout.Location -eq '/Account/Login') 'Đăng xuất bằng POST kết thúc phiên'

    $afterLogout = Get-Page '/Admin/Accounts'
    Assert-True ($afterLogout.Status -eq 302 -and $afterLogout.Location -like '/Account/Login*') 'Sau đăng xuất, trang quản trị bị chặn lại'

    # 6. A created non-admin account can practise but not administer accounts.
    $createLogin = Post-Form '/Account/Login' @{
        Email = $Email
        Password = $Password
        ReturnUrl = '/Admin/Accounts'
        __RequestVerificationToken = (Get-Token (Get-Page '/Account/Login').Body)
    }
    Assert-True ($createLogin.Status -eq 302) 'Đăng nhập lại để tạo tài khoản thử'

    $adminPage = Get-Page '/Admin/Accounts'
    $learnerEmail = "learner-$([guid]::NewGuid().ToString('N').Substring(0,8))@renshuu.local"
    $createForm = Post-Form '/Admin/Accounts?handler=Create' @{
        NewEmail = $learnerEmail
        NewDisplayName = 'Người học thử'
        NewPassword = 'Learner#2026pass'
        __RequestVerificationToken = (Get-Token $adminPage.Body)
    }
    Assert-True ($createForm.Status -eq 302) 'Tạo được tài khoản người học'

    $logoutAdmin = Post-Form '/Account/Logout' @{
        __RequestVerificationToken = (Get-Token (Get-Page '/Admin/Accounts').Body)
    }
    Assert-True ($logoutAdmin.Status -eq 302) 'Đăng xuất quản trị viên'

    $learnerLogin = Post-Form '/Account/Login' @{
        Email = $learnerEmail
        Password = 'Learner#2026pass'
        __RequestVerificationToken = (Get-Token (Get-Page '/Account/Login').Body)
    }
    Assert-True ($learnerLogin.Status -eq 302 -and $learnerLogin.Location -eq '/') 'Người học đăng nhập được'

    $learnerHome = Get-Page '/'
    Assert-True ($learnerHome.Status -eq 200) 'Người học xem được không gian luyện tập'

    $learnerAdmin = Get-Page '/Admin/Accounts'
    Assert-True ($learnerAdmin.Status -eq 302 -and $learnerAdmin.Location -like '/Account/Denied*') 'Người học bị chặn khỏi trang quản lý tài khoản'

    $learnerContent = Get-Page '/Admin/Index'
    Assert-True ($learnerContent.Status -eq 200) 'Người học vẫn dùng được trang quản lý nội dung của mình'
}
finally {
    Remove-Item -Recurse -Force $work -ErrorAction SilentlyContinue
}

Write-Host ''
if ($failures.Count -eq 0) {
    Write-Host 'Tất cả kiểm tra đăng nhập/phân quyền đều đạt.'
    exit 0
}

Write-Host ("Có {0} kiểm tra không đạt:" -f $failures.Count)
$failures | ForEach-Object { Write-Host " - $_" }
exit 1
