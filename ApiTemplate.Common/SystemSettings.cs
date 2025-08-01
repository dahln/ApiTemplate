

namespace ApiTemplate.Common;

public class SystemSettings
{
    public string SendGridKey { get; set; } = string.Empty;
    public string SendGridSystemEmailAddress { get; set; } = string.Empty;
    public bool RegistrationEnabled { get; set; }
    public string EmailDomainRestriction { get; set; } = string.Empty;
}


