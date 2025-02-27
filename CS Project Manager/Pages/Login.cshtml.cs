/*
 * Prologue
Created By: Isabel Loney
Date Created: 2/25/25
Last Revised By: Isabel Loney
Date Revised: 2/27/25
Purpose: Handles user login functionality by validating email and password input, checking if the user exists in the database, 
and signing in the user upon successful authentication. Displays appropriate error messages for invalid login attempts.

Preconditions: StudentUserService instance must be properly initialized and injected, login form must provide non-null, non-empty values for Email and Password, User model must be correctly defined
Postconditions: user is authenticated and signed in if the username and password are valid, user is redirected to the Dashboard page upon successful login, rror messages are displayed for invalid login attempts
Error and exceptions: ArgumentNullException (thrown if the username or Password properties are null)
Side effects: N/A
Invariants: _studentUserserService field is always initialized with a valid instance
Other faults: N/A
*/

using CS_Project_Manager.Services;
using CS_Project_Manager.Utilities;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace CS_Project_Manager.Pages
{
    public class LoginModel(StudentUserService studentUserService) : PageModel
    {
        // Instance of UserService used for user-related database operations
        private readonly StudentUserService _studentUserService = studentUserService;

        // Bound properties to hold email and password input from the login form
        [BindProperty]
        public string Username { get; set; }    // User-provided email address for login
        [BindProperty]
        public string Password { get; set; } // User-provided password for login

        // Handles login submission; verifies user credentials and initiates authentication if valid
        public async Task<IActionResult> OnPostAsync()
        {
            // Check if Email or Password fields are empty and return error if so
            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
            {
                ModelState.AddModelError(string.Empty, "Email and password cannot be empty.");
                return Page(); // Return the same page with error messages
            }

            // Attempt to retrieve the user by email from the database
            var user = await _studentUserService.GetUserByUsernameAsync(Username);
            if (user == null) // If no user is found, display an error message
            {
                ModelState.AddModelError(string.Empty, "No account associated with this username.");
                return Page();
            }

            // Verify that the provided password matches the stored password hash
            if (!PasswordHelper.VerifyPassword(Password, user.PasswordHash))
            {
                ModelState.AddModelError(string.Empty, "Incorrect password."); // Error for incorrect password
                return Page();
            }

            // If credentials are valid, create a list of claims for the authenticated user
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, user.Username), // User's username
            };

            // Create a ClaimsIdentity using the claims and cookie authentication scheme
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            // Sign in the user with the created ClaimsPrincipal, which contains their identity information
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            // Redirect to the home page after successful login
            return RedirectToPage("/Dashboard");
        }
    }
}
