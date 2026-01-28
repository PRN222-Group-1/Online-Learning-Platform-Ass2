namespace OnlineLearningPlatformAss2.Service.Utils;

public class PasswordUtils
{
    public static string HashPassword(string password)
    {
        if (password is null)
            throw new ArgumentNullException(nameof(password), "Password cannot be null");

        return string.IsNullOrWhiteSpace(password)
            ? throw new ArgumentException("Password cannot be empty or whitespace", nameof(password))
            : BCrypt.Net.BCrypt.HashPassword(password, 12);
    }

    public static bool VerifyPassword(string password, string hash)
    {
        if (password is null)
            throw new ArgumentNullException(nameof(password), "Password cannot be null");

        return hash is null
            ? throw new ArgumentNullException(nameof(hash), "Hash cannot be null")
            : BCrypt.Net.BCrypt.Verify(password, hash);
    }
}
