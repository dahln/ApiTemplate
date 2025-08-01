using System;

namespace ApiTemplate.Common;

public class AccountEmail
{
    public string Email { get; set; } = string.Empty;
}

public class IdentityManageUserResponse
{
    public string Email { get; set; } = string.Empty;
    public bool IsEmailConfirmed { get; set; }
}

public class IdentityManage2faResponse
{
    public string SharedKey { get; set; } = string.Empty;
    public int RecoveryCodesLeft { get; set; }
    public string[] RecoveryCodes { get; set; } = Array.Empty<string>();
    public bool IsTwoFactorEnabled { get; set; }
    public bool IsMachineRemembered { get; set; }
}

public class LoginResponse
{
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
    public int Status { get; set; }
}

public class User
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsAdministrator { get; set; }
    public bool IsSelf { get; set; }
}




