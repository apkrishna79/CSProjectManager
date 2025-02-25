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
