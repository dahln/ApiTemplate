using ApiTemplate.API.Utility;
using ApiTemplate.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiTemplate.API.Controllers;

[Tags("Identity API - Additional/Supplemental Endpoints")]
public class AccountController : Controller
{
    private DBContext _db;
    private UserManager<IdentityUser> _userManager;
    private SignInManager<IdentityUser> _signInManager;
    public AccountController(DBContext dbContext, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
    {
        _db = dbContext;
        _userManager = userManager;
        _signInManager = signInManager;
    }


    [Authorize]
    [HttpGet]
    [Route("api/v1/account/logout")]
    [EndpointDescription("Logout the current user.")]
    [ProducesResponseType(typeof(void), 200)]
    async public Task<IActionResult> AccountLogout()
    {
        await _signInManager.SignOutAsync();
        return Ok();
    }

    /// <summary>
    /// Call this BEFORE allowing the change of email.
    /// WHY? Because the identity API doesn't check if an email is unique when updating user email with POST:manage/info.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [Authorize]
    [HttpPost]
    [Route("api/v1/account/exists")]
    [EndpointDescription("Check if an account exists by email.")]
    [ProducesResponseType(typeof(bool), 200)]
    async public Task<IActionResult> AccountExistsByEmail([FromBody] ApiTemplate.Common.AccountEmail model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        return Ok(user != null);
    }

    [Authorize]
    [HttpDelete]
    [Route("api/v1/account")]
    [EndpointDescription("Delete the current user account.")]
    [ProducesResponseType(typeof(void), 200)]
    [ProducesResponseType(typeof(string), 400)]
    async public Task<IActionResult> DeleteAccount()
    {
        string userId = User.GetUserId();
        //DELETE: Do necesary account cleanup - what do we archive and what do we delete?


        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return BadRequest("User not found.");

        await _userManager.DeleteAsync(user);
        return Ok();
    }

    /// <summary>
    /// Return True or False to let the App know if the API/Server will allow all operations. 
    /// For example, if the Server does not have a SendGrid API key then Password Recovery and Changing Email is 
    /// not allowed because the recovery and confirmation emails will never be sent.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("api/v1/account/operations/recovery")]
    [EndpointDescription("Check if the account recovery operations are allowed. If the SendGrid API is integrated, then account recovery emails can be sent to the user.")]
    [ProducesResponseType(typeof(bool), 200)]
    [ProducesResponseType(typeof(string), 400)]
    public async Task<IActionResult> AccountAllowAllOperations()
    {
        var settings = await _db.SystemSettings.FirstOrDefaultAsync();
        if (settings == null)
            return BadRequest("Settings Missing");

        var sendGridApi = settings.SendGridKey;
        var sendGridSystemEmailAddress = settings.SendGridSystemEmailAddress;

        var allowAllOperations = false;

        if (!string.IsNullOrEmpty(sendGridApi) && !string.IsNullOrEmpty(sendGridSystemEmailAddress))
            allowAllOperations = true;

        return Ok(allowAllOperations);
    }


    [Authorize]
    [HttpGet]
    [Route("api/v1/account/roles")]
    [EndpointDescription("Get the current user's roles.")]
    [ProducesResponseType(typeof(List<string>), 200)]
    [ProducesResponseType(typeof(string), 400)]
    async public Task<IActionResult> GeCurrentUserRoles()
    {
        string userId = User.GetUserId();
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return BadRequest("User not found.");

        var userRoles = await _userManager.GetRolesAsync(user);
        return Ok(userRoles);
    }


    [Authorize]
    [HttpGet]
    [Route("api/v1/account/2fa")]
    [EndpointDescription("Check if 2FA is enabled for the current account.")]
    [ProducesResponseType(typeof(bool), 200)]
    [ProducesResponseType(typeof(string), 400)]
    public async Task<IActionResult> AccountTwoFactorEnabled()
    {
        string userId = User.GetUserId();
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return BadRequest("User not found.");

        var isTwoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
        return Ok(isTwoFactorEnabled);
    }

    [Authorize(Roles = "Administrator")]
    [HttpGet]
    [Route("api/v1/user/{userId}/role/administrator")]
    [EndpointDescription("Administrator: Provide a userId, and toggle the 'Administrator' role for a user-by-id.")]
    [ProducesResponseType(typeof(void), 200)]
    [ProducesResponseType(typeof(string), 400)]
    async public Task<IActionResult> ToggleUserAdministratorRole(string userId)
    {
        string currentUserId = User.GetUserId();

        if (currentUserId == userId)
            return BadRequest("You cannot toggle your own administrative role");

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound("User not found.");

        var IsAdministrator = await _userManager.IsInRoleAsync(user, "Administrator");
        if (IsAdministrator)
            await _userManager.RemoveFromRoleAsync(user, "Administrator");
        else
            await _userManager.AddToRoleAsync(user, "Administrator");

        return Ok();
    }

    [Authorize(Roles = "Administrator")]
    [HttpDelete]
    [Route("api/v1/user/{userId}")]
    [EndpointDescription("Administrator: Delete a user by userId. Administrator CANNOT use this call to delete their own account.")]
    [ProducesResponseType(typeof(void), 200)]
    [ProducesResponseType(typeof(string), 400)]
    async public Task<IActionResult> DeleteUser(string userId)
    {
        string currentUserId = User.GetUserId();
        if (currentUserId == userId)
            return BadRequest("Cannot delete this account.");

        var user = await _userManager.FindByIdAsync(userId);
        if (user != null)
            await _userManager.DeleteAsync(user);

        return Ok();
    }

    [Authorize(Roles = "Administrator")]
    [HttpPost]
    [Route("api/v1/users")]
    [EndpointDescription("Administrator: Search for users by email. Returns a paginated list of users.")]
    [ProducesResponseType(typeof(ApiTemplate.Common.SearchResponse<ApiTemplate.Common.User>), 200)]
    async public Task<IActionResult> UserSearch([FromBody] ApiTemplate.Common.Search model)
    {
        string userId = User.GetUserId();

        var query = _db.Users.AsQueryable();

        if (!string.IsNullOrEmpty(model.FilterText))
        {
            query = query.Where(i => i.Email != null && i.Email.ToLower().ToLower().Contains(model.FilterText.ToLower()));
        }

        if (model.SortBy == nameof(ApiTemplate.Common.User.Email))
        {
            query = model.SortDirection == ApiTemplate.Common.SortDirection.Ascending
                        ? query.OrderBy(c => c.Email)
                        : query.OrderByDescending(c => c.Email);
        }
        else
        {
            query = model.SortDirection == ApiTemplate.Common.SortDirection.Ascending
                        ? query.OrderBy(c => c.Email)
                        : query.OrderByDescending(c => c.Email);
        }

        ApiTemplate.Common.SearchResponse<ApiTemplate.Common.User> response = new ApiTemplate.Common.SearchResponse<ApiTemplate.Common.User>()
        {
            PageNumber = model.PageNumber,
            PageSize = model.PageSize
        };
        response.TotalResults = await query.CountAsync();


        var dataResponse = await query.Skip((model.PageNumber ?? 0) * (model.PageSize ?? 15))
                                      .Take(model.PageSize ?? 15)
                                      .ToListAsync();

        response.Results = dataResponse.Select(c => new ApiTemplate.Common.User()
        {
            Id = c.Id,
            Email = c.Email ?? string.Empty,
            IsAdministrator = false //Populate this in the next step.
        }).ToList();

        foreach (var user in response.Results)
        {
            var tempUser = dataResponse.FirstOrDefault(x => x.Id == user.Id);
            if (tempUser == null)
                continue;

            user.IsAdministrator = await _userManager.IsInRoleAsync(tempUser, "Administrator");
            user.IsSelf = user.Id == userId;
        }

        return Ok(response);
    }

    [Authorize(Roles = "Administrator")]
    [HttpPut]
    [Route("api/v1/settings")]
    [EndpointDescription("Administrator: Update the system settings.")]
    [ProducesResponseType(typeof(void), 200)]
    [ProducesResponseType(typeof(string), 400)]
    async public Task<IActionResult> UpdateSystemSettings([FromBody] ApiTemplate.Common.SystemSettings model)
    {
        //Settings exist, created on startup. We simply need to update them.

        var settings = await _db.SystemSettings.FirstOrDefaultAsync();
        if (settings == null)
            return BadRequest("Settings Missing");

        if (model.SendGridKey != null)
            settings.SendGridKey = (model.SendGridKey.Trim() == "--- NOT DISPLAYED FOR SECURITY ---" || model.SendGridKey.Trim() == string.Empty) ? settings.SendGridKey : model.SendGridKey;
        else
            settings.SendGridKey = null;

        settings.SendGridSystemEmailAddress = model.SendGridSystemEmailAddress;

        await _db.SaveChangesAsync();

        return Ok();
    }

    [Authorize(Roles = "Administrator")]
    [HttpGet]
    [Route("api/v1/settings")]
    [EndpointDescription("Administrator: Get the system settings.")]
    [ProducesResponseType(typeof(ApiTemplate.Common.SystemSettings), 200)]
    [ProducesResponseType(typeof(string), 400)]
    async public Task<IActionResult> GetSystemSettings()
    {
        var settings = await _db.SystemSettings.FirstOrDefaultAsync();
        if (settings == null)
            return BadRequest("Settings Missing");

        var response = new ApiTemplate.Common.SystemSettings()
        {
            SendGridKey = "--- NOT DISPLAYED FOR SECURITY ---",
            SendGridSystemEmailAddress = settings.SendGridSystemEmailAddress != null ? settings.SendGridSystemEmailAddress : string.Empty,
        };

        return Ok(response);
    }

}



