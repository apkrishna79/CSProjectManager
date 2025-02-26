/*
* Prologue
Created By: Isabel Loney
Date Created: 2/25/2025
Last Revised By: Isabel Loney
Date Revised: 2/26/2025
Purpose: Provides utility methods for hashing and verifying passwords using BCrypt.

Preconditions: password parameter must be a non-null, non-empty string; hash parameter (for VerifyPassword) must be a non-null, non-empty string and a valid BCrypt hash
Postconditions: HashPassword returns a hashed version of the input password, VerifyPassword returns true if the password matches the hash, false otherwise
Error and exceptions: ArgumentNullException (thrown if the password or hash parameter is null), BCrypt.Net.SaltParseException (thrown if the hash parameter is not a valid BCrypt hash)
Side effects: N/A
Invariants: HashPassword always returns a valid BCrypt hash, VerifyPassword always returns a boolean value
Other faults: N/A
*/

namespace CS_Project_Manager.Utilities
{
    public class PasswordHelper
    {
        // Hashes a plaintext password using BCrypt for secure storage
        public static string HashPassword(string password) =>
            BCrypt.Net.BCrypt.HashPassword(password);

        // Verifies a plaintext password against a hashed password to check for a match
        public static bool VerifyPassword(string password, string hash) =>
            BCrypt.Net.BCrypt.Verify(password, hash);
    }
}
