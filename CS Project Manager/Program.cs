/*
 * Prologue: Program.cs
 * Programmers: Anakha Krishna, Ginny Ke, Jackson Wunderlich
 * Date Created: 2/13/25
 * Last revised by: Ginny Ke - added support for sprint board
 * Date Revised: 4/13/25 
 * Purpose: Configures and starts the web application using ASP.NET Core with Razor Pages.
 *
 * Preconditions:
 * - .NET SDK and required dependencies installed
 * - Application run within valid ASP.NET Core environment
 * - MongoDB configuration and database connection
 * - Whitelist IP address in MongoDB configuration
 *
 * Postconditions:
 * - Web application initialized and running
 * - Razor Pages configured and accessible
 *
 * Errors and exceptions:
 * - If environment not in development mode, exceptions are handled by redirecting to "/Error"
 * - If missing dependencies, application may fail to start
 *
 * Side effects:
 * - Application starts listening for incoming HTTP requests
 * - Static files are served
 * - Authorization middleware is enabled
 *
 * Invariants:
 * - Application always maps Razor Pages
 * - Static files are always served unless explicitly removed
 * - App follows the configured routing conventions
 *
 * Other faults: N/A
 */


using CS_Project_Manager.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using MongoDB.Bson;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args); // Create web application builder instance to initialize the application

// Add services to the container
// Configures the application to use cookie-based authentication with specified login and logout paths
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(options =>
        {
            options.LoginPath = "/Login";   // Redirects to /Login if the user is not authenticated
            options.LogoutPath = "/Logout"; // Redirects to /Logout after the user logs out
        });

builder.Services.AddRazorPages(); // Register Razor Pages services in dependency injection container

// Add MVC services with Razor Pages options and configure routing conventions
builder.Services.AddMvc().AddRazorPagesOptions(options =>
{
    options.Conventions.AddPageRoute("/Landing", ""); // Set "/Landing" as the default route
});

// MongoDB connection string for connecting to the database
const string connectionUri = "mongodb+srv://ginnyk10:Gk7856212727%24@csproman.v6xg6.mongodb.net/CSProMan?retryWrites=true&w=majority";

// Registers MongoDBService as a singleton, providing database connectivity throughout the application
builder.Services.AddSingleton(new MongoDBService(connectionUri));
builder.Services.AddSingleton<IMongoClient>(sp => new MongoClient(connectionUri));

// Registering required services for dependency injection
builder.Services.AddScoped<StudentUserService>();
builder.Services.AddScoped<ClassService>();
builder.Services.AddScoped<TeamService>();
builder.Services.AddScoped<ProjectService>();
builder.Services.AddScoped<RequirementService>();
builder.Services.AddScoped<TodoService>();
builder.Services.AddScoped<BrainstormService>();
builder.Services.AddScoped<CalendarService>();
builder.Services.AddScoped<DiscussionBoardService>();
builder.Services.AddScoped<DiscussionPostService>();
builder.Services.AddScoped<UserAvailabilityService>();
builder.Services.AddScoped<MeetingMinutesService>();
builder.Services.AddScoped<SprintBoardService>(); // support for sprint retrospective board

var app = builder.Build(); // Build app from configured builder

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment()) // If app not running in development mode:
{
    app.UseExceptionHandler("/Error"); // Use centralized error handler -> redirects to "/Error" when an exception occurs
}

app.UseStaticFiles(); // Enable serving static files (CSS, JS, etc.)

app.UseRouting(); // Enable request routing -> allows app to match incoming requests to endpoints

app.UseAuthorization(); // Enable authorization middleware (policies not yet defined as of 2/16)

app.MapRazorPages(); // Map Razor Pages to handle requests to corresponding Razor Page files

app.Run(); // Start app and begin listening for incoming HTTP requests
