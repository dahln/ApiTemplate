[![Build ApiTemplate](https://github.com/dahln/ApiTemplate/actions/workflows/build.yml/badge.svg)](https://github.com/dahln/ApiTemplate/actions/workflows/build.yml)

## What & Why
This project is an example of one way to build a .NET Web application, using Blazor, Web API, and SQL. Demonstrating simple CRUD & Search operations, protected by Authentication/Authorization. This is an active application that I continue to update as .NET is updated and expanded. I created this template because the stock templates from Microsoft do not offer all the functionality I want in a template. This template is largely a collection of different tools and libraries that I use in my API's.

## Technologies
 - .NET 8 & C#
 - Web API
 - SQL (Sqlite)
 - Identity API
 - Identity API 2FA
 - GitHub Actions

## Getting Started
Getting started with this project is easy.
1. Recommendation: Use the Github 'Use this template' feature. Then clone the repo.
2. Using the powershell script 'RenameProject.ps1', rename the 'ApiTemplate' folders, files, and code references from 'ApiTemplate' to the 'NewProjectNameOfYourChoice'. From the root of the solution, run this command: 

   Some files and folders will not be updated if they are open. Close VS Code, Visual Studio, or other tools prior to running this script.
   ```
   .\RenameProject.ps1 -FolderPath . -OldName "ApiTemplate"  -NewName "NewProjectNameOfYourChoice"
   ```
   You can delete the script after you use it. Unless you want to rename your project again, there is no reason to keep it.

4. From the root of the solution, start the API project by running this command:
   ```
   dotnet watch --project NewProjectNameOfYourChoice.API
   ```
   The API project acts as the host for the API and the App.

5. You can use the API and the 'seed' method to create a large quantity of seed data to expirament with. See the .http file for examples of http calls or visit {yourAppUrl}/swagger to use the API swagger page.

## Working with .http Files
This project includes a `ApiTemplate.API.http` file with examples of API calls that you can use to test the endpoints. 

- **Visual Studio**: Has built-in support for .http files. You can run requests directly from the editor.
- **VS Code**: Requires an extension to work with .http files. We recommend installing the [REST Client extension](https://marketplace.visualstudio.com/items?itemName=humao.rest-client) by Huachao Mao.

The REST Client extension allows you to:
- Send HTTP requests directly from VS Code
- View responses in a formatted way
- Save and organize your API requests
- Use variables and environments for different configurations

After installing the extension, you can open the `ApiTemplate.API.http` file and click the "Send Request" link above each HTTP request to execute it.

## Misc Details    
1. This project has no required outside dependencies to get started. You simply need to clone the repo, then run the API project by calling 'dotnet watch' from the API project folder.
2. Authentication is handled by the Identity Framework, and user data is stored in your own DB.
3. On optional (but recommended) dependency is SendGrid. This template uses SendGrid to send emails. The template does not require SendGrid in order to work, however some features are not available until you add a SendGrid API key and system email address to the AppSettings.json. Features you cannot use without SendGrid include:
   - Account Activation and Email Confirmation
   - Account/Password recovery
   - Allowing a user to change their email/username.
4. This application demonstrates simple CRUD operations.
5. This template has some basic admin features. The first user to register will automatically be given the administrator role. Administrative abilities include: enable/disable registration, setting the SendGrid email  API values, and listing/deleting registered users.

## Project Architecture
This application has 4 projects in the solution. The API, Common, and Database. The API is all the server functionality; it contains all the database operations and server side logic. The API contains all thel logic and functionality. The Common project is data models that the API exposes, so that we don't expose database structure to the public. The database project contains all the Entities and DB Context. Depending on your needs, you could create a service layer to contain all the business logic and remove DB operations from the API project. Every project is different, this template is a starting point.

## API Versioning
There are tools to handle API versioning. Add which ever tools you prefer. This template handles API version manually by specifying v1 in the service URL.

## Why Identity for Authentication?
1. It enables quick setup and getting your project going easily. 
2. It is easy to customize and supported by Microsoft.
3. It allows for 100% control of your user data and authentication/authorization process. There are other Authentication options such as Azure B2C/Entra and Auth0.

## Administrator Role
This project includes an Administrator role that provides access to administrative functions and API endpoints. The administrator role is automatically created when the application starts up.

### Default Administrator Account
On startup, the application looks for a user with the email `administrator@ApiTemplate.site` and automatically assigns them the Administrator role if they exist. **Important:** You should change this email address to your own email in the `Program.cs` file before deploying to production.

To update the default administrator email:
1. Open `ApiTemplate.API/Program.cs`
2. Find the line: `var user = await userManager.FindByEmailAsync("administrator@ApiTemplate.site");`
3. Replace `"administrator@ApiTemplate.site"` with your desired email address
4. Register a user account with that email address

### Administrator API Endpoints
Administrators have access to the following API endpoints (all require the `Administrator` role):

**User Management:**
- `GET /api/v1/user/{userId}/role/administrator` - Toggle administrator role for a user
- `DELETE /api/v1/user/{userId}` - Delete a user by ID (cannot delete own account)
- `POST /api/v1/users` - Search for users by email (paginated results)

**System Settings:**
- `GET /api/v1/settings` - Get current system settings
- `PUT /api/v1/settings` - Update system settings (SendGrid configuration)

### Managing Administrator Roles via API
To grant or remove administrator privileges for users:

1. **Get a user's current roles:**
   ```
   GET /api/v1/account/roles
   ```

2. **Search for users (Administrator only):**
   ```
   POST /api/v1/users
   Content-Type: application/json
   
   {
     "filterText": "user@example.com",
     "pageNumber": 0,
     "pageSize": 15
   }
   ```

3. **Toggle administrator role for a user (Administrator only):**
   ```
   GET /api/v1/user/{userId}/role/administrator
   ```

See the `ApiTemplate.API.http` file for complete examples of these API calls.

## [SendGrid](https://sendgrid.com/en-us/pricing)
This project uses SendGrid to send emails. A SendGrid API key is required. You will need to specify your own SendGrid API key and system email address. Some features that require email are not available until you provide the necessary SendGrid values. It is a simple process to create your own SendGrid account and retreive your API key. You can add these configuration values to your API by using the '/api/v1/settings' endpoints.

## Why SQLite?
It runs on Windows and Linux. It is great for this template. Depending on your projects needs, it may work for production. If you need more than SQLite offers then I recommend switching to Azure SQL. If you switch to Azure SQL, besure to delete your SQLite DB migrations and create new a 'Initial Migration' for your new Azure SQL DB.

## DB Migrations
This project includes the necessary "Initial Creation" DB migration, used for the initial creation of a DB when the application connects to the DB for the first time. The Program.cs in the API project will automatically check for DB migrations which need to run, and run them automatically. You can run the DB migrations manually if desired. The commands below outline how to generate a DB migration and run a migration.

https://docs.microsoft.com/en-us/ef/core/cli/dotnet#common-options

## Using dotnet CLI
Run these commands from the root of the solution. Adjust these commands to match the name of your project (Replace 'ApiTemplate')
```
dotnet ef migrations add InitialCreate --project ApiTemplate.Database --startup-project ApiTemplate.API
```
```
dotnet ef database update --project ApiTemplate.Database --startup-project ApiTemplate.API
```


## Ignore Local Changes to AppSettings.json
Sensative configuration data, such as the DB connection strings, are kept in the appsettings.json files. Depending on your situation, you may NOT want to check in these values to the repo. Use the following commands to ignore changes to the appsettings.json files:
 ```
 git update-index --assume-unchanged .\ApiTemplate.API\appsettings.json
 ```
 To reverse the ignore, use these commands:
 ```
 git update-index --no-assume-unchanged .\ApiTemplate.API\appsettings.json
 ```


## Licensing
This project uses the 'Unlicense'.  It is a simple license - review it at your own leisure.

## Resources
- [Identity API with WebAPI](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity-api-authorization?view=aspnetcore-8.0)

## Misc & Recommended Tools
1. [Azure](https://portal.azure.com)
2. [Namecheap](https://namecheap.com)
2. [Namecheap Logo Maker](https://www.namecheap.com/logo-maker/)
3. [SSLS](https://www.ssls.com/)
4. [SVG Crop](https://svgcrop.com/)

## Questions & Contributions
Questions and contributions are welcome. Send them in by creating a GitHub issue.



