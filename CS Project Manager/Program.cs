/*
 * Prologue: Program.cs
 * Programmers: Anakha Krishna
 * Date Created: 2/13/25
 * Date Revised: 2/16/25
 * Purpose: Configures and starts the web application using ASP.NET Core with Razor Pages.
 *
 * Preconditions:
 * - .NET SDK and required dependencies installed
 * - Application run within valid ASP.NET Core environment
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

var builder = WebApplication.CreateBuilder(args); // Create web application builder instance to initialize the application

builder.Services.AddRazorPages(); // Register Razor Pages services in dependency injection container

// Add MVC services with Razor Pages options and configure routing conventions
builder.Services.AddMvc().AddRazorPagesOptions(options => 
{
    options.Conventions.AddPageRoute("/Landing", ""); // Set "/Landing" as the default route
});

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
