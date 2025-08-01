using ApiTemplate.Database;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Threading.Tasks;

namespace ApiTemplate.API.Utility;

//The details in this github issue where helpful.
//https://github.com/dotnet/aspnetcore/issues/50298

internal sealed class EmailSender : IEmailSender<IdentityUser>
{
    private readonly ILogger _logger;
    private IHttpContextAccessor _httpContextAccessor;
    private readonly IServiceScopeFactory _scopeFactory;

    public EmailSender(ILogger<EmailSender> logger, IHttpContextAccessor httpContextAccessor, IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
        _scopeFactory = serviceScopeFactory;
    }

    private async Task SendEmailAsync(string toEmail, string subject, string message)
    {
        using (var scope = _scopeFactory.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<DBContext>();
            var settings = await dbContext.SystemSettings.FirstOrDefaultAsync();
            if (settings == null || string.IsNullOrEmpty(settings.SendGridKey) || string.IsNullOrEmpty(settings.SendGridSystemEmailAddress))
            {
                return;
            }

            await Execute(settings.SendGridKey, settings.SendGridSystemEmailAddress, subject, message, toEmail);
        }
    }

    private async Task Execute(string apiKey, string sendGridFromEmail, string subject, string message, string toEmail)
    {
        if (string.IsNullOrEmpty(apiKey))
        {
            return;
        }

        if (string.IsNullOrEmpty(sendGridFromEmail))
        {
            return;
        }

        var client = new SendGridClient(apiKey);
        var msg = new SendGridMessage()
        {
            From = new EmailAddress(sendGridFromEmail),
            Subject = subject,
            PlainTextContent = message,
            HtmlContent = message
        };
        msg.AddTo(new EmailAddress(toEmail));

        // Disable click tracking.
        msg.SetClickTracking(false, false);
        var response = await client.SendEmailAsync(msg);
        _logger.LogInformation(response.IsSuccessStatusCode
                               ? $"Email to {toEmail} queued successfully!"
                               : $"Failure Email to {toEmail}");
    }

    public Task SendConfirmationLinkAsync(IdentityUser user, string email, string confirmationLink)
    {
        Uri confirmationLinkUri = new Uri(confirmationLink);
        var adjustedUrlForApp = confirmationLinkUri.PathAndQuery.Replace("confirmEmail", "confirmingEmail");
        if (_httpContextAccessor.HttpContext == null || _httpContextAccessor.HttpContext.Request == null)
        {
            throw new InvalidOperationException("HttpContext or HttpContext.Request is null.");
        }
        var hostUrl = _httpContextAccessor.HttpContext.Request.Host.Value;

        Uri adjustedConfirmationLink = new Uri($"https://{hostUrl}{adjustedUrlForApp}");

        if (adjustedConfirmationLink.Query.Contains("changedEmail="))
        {
            return SendEmailAsync(email, "Confirm your changed email", $"You have changed your email. Please confirm your new email by <a href='{adjustedConfirmationLink}'>clicking here</a>. If you did not request an email change, disregard this email. Thank you.");
        }
        else
        {
            return SendEmailAsync(email, "Confirm your email", $"Please confirm your account by <a href='{adjustedConfirmationLink}'>clicking here</a>. Thank you.");
        }
    }

    public Task SendPasswordResetLinkAsync(IdentityUser user, string email, string resetLink) =>
        SendEmailAsync(email, "Reset your password", $"Please reset your password by <a href='{resetLink}'>clicking here</a>.");

    public Task SendPasswordResetCodeAsync(IdentityUser user, string email, string resetCode)
    {
        if (_httpContextAccessor.HttpContext == null || _httpContextAccessor.HttpContext.Request == null)
        {
            throw new InvalidOperationException("HttpContext or HttpContext.Request is null.");
        }

        var resetLink = $"https://{_httpContextAccessor.HttpContext.Request.Host.Value}/password/reset/{resetCode}";
        return SendEmailAsync(email, "Reset your password", $"Please reset your password using the following link <a href='{resetLink}'>Reset Password.</a>");
    }
}

