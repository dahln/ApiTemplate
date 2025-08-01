using ApiTemplate.API.Utility;
using ApiTemplate.Database;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);

//Add this so we can access HTTPContext data in other services.
builder.Services.AddHttpContextAccessor();


//Add the Database context.
builder.Services.AddDbContext<DBContext>(
   options => options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));


//Authorization is handles by Identity
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdministratorRole", policy => policy.RequireRole("Administrator"));
});


//Identiy API needs to be accessible
builder.Services.AddIdentityApiEndpoints<IdentityUser>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<DBContext>();

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => {
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ApiTemplate API", Version = "v1" });
    c.ResolveConflictingActions((apiDescriptions) => apiDescriptions.First());
});

//Identity options. Uncomment this to required email confirmation before allowing sign in.
//Other options are available.
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Lockout.AllowedForNewUsers = true;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    //options.SignIn.RequireConfirmedEmail = true;
});

//Adding necessary services.
builder.Services.AddTransient<UserManager<IdentityUser>>();
builder.Services.AddTransient<RoleManager<IdentityRole>>();
builder.Services.AddTransient<SignInManager<IdentityUser>>();
builder.Services.AddTransient<IEmailSender<IdentityUser>, EmailSender>();
builder.Services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();


var app = builder.Build();

//Check for and automatically apply pending migrations
//You can run DB migrations manually, or allow the system to run DB migrations on startup.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    //Ensure the database is created and migrations are applied
    var db = services.GetRequiredService<DBContext>();
    if (db != null)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        
        //If there are pending migrations, apply them.
        //This is useful for development and testing, but in production you may want to run migrations manually or through a CI/CD pipeline - adjust it for your needs.
        //This will ensure that the database is up to date with the latest migrations.
        if (db != null)
        {
            var migrations = db.Database.GetPendingMigrations();
            if (migrations.Any())
                await db.Database.MigrateAsync();
        }

        // Ensure the "Administrator" role exists
        var roleAdministratorExists = await roleManager.RoleExistsAsync("Administrator");
        if (!roleAdministratorExists)
        {
            await roleManager.CreateAsync(new IdentityRole("Administrator"));
        }

        //Look for the user 'administrator@ApiTemplate.site'. If it exists and is not an administrator, make it an administrator.
        //Note to Team: we can add additional users if necessary here. I added myself as an initial administrator. There is nothing special about my account and we can remove it in the future, as long as there are other administrators.
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
        var user = await userManager.FindByEmailAsync("administrator@ApiTemplate.site");
        if (user != null)
        {
            //If the user is not an administrator, make them an administrator.
            if (!await userManager.IsInRoleAsync(user, "Administrator"))
            {
                await userManager.AddToRoleAsync(user, "Administrator");
            }
        }

        //Ensure DB is not null. Seems redundant because of earlier null checks but the compiler was complaining about the possibility of db being null.
        if (db != null)
        {
            // Ensure the "SystemSetting" entry exists - it will be empty, but it will exist. This will simplify reading system settings.
            var systemSettings = db.SystemSettings.Any();
            if (systemSettings == false)
            {
                var newSystemSettings = new SystemSetting();
                db.SystemSettings.Add(newSystemSettings);
                db.SaveChanges();
            }
        }
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//Host the Darknote.App files in the same 'Server' process as the API.
app.UseRouting();

//Authorization was added, now the app needs to use it.
app.UseAuthorization();

app.MapControllers();


//Expose identity API endpoints. Identity API doesn't include a logout method. One was created in the account controller, along with other account related endpoints.
app.MapIdentityApi<IdentityUser>()
    .WithTags("Identity API - Included by Framework");
//app.MapPost("/register", () => "Deprecated. Use /api/v1/account/register."); //This will disable the built in 'Identity /register' method.

app.Run();


