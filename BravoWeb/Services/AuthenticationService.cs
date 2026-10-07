using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BravoWeb.Data;
using BravoWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace BravoWeb.Services;

/// <summary>
/// Service for handling user authentication, password hashing, and user verification
/// </summary>
public class UserAuthenticationService
{
    private readonly AppDbContext _context;

    public UserAuthenticationService(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Hash a password using PBKDF2 algorithm
    /// </summary>
    public string HashPassword(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            const int iterations = 10000;
            const int saltSize = 16;
            const int hashSize = 32;

            // Generate random salt
            var salt = new byte[saltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // Hash password with PBKDF2
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256))
            {
                var hash = pbkdf2.GetBytes(hashSize);

                // Combine salt + hash and encode as base64
                var hashWithSalt = new byte[saltSize + hashSize];
                Array.Copy(salt, 0, hashWithSalt, 0, saltSize);
                Array.Copy(hash, 0, hashWithSalt, saltSize, hashSize);

                return Convert.ToBase64String(hashWithSalt);
            }
        }
    }

    /// <summary>
    /// Verify if a password matches the stored hash
    /// </summary>
    public bool VerifyPassword(string password, string hash)
    {
        try
        {
            // Decode the hash
            var hashBytes = Convert.FromBase64String(hash);
            const int saltSize = 16;
            const int iterations = 10000;

            // Extract salt
            var salt = new byte[saltSize];
            Array.Copy(hashBytes, 0, salt, 0, saltSize);

            // Hash the provided password with the extracted salt
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256))
            {
                var hash2 = pbkdf2.GetBytes(32);

                // Compare hashes
                for (int i = 0; i < 32; i++)
                {
                    if (hashBytes[i + saltSize] != hash2[i])
                        return false;
                }

                return true;
            }
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Authenticate user by username and password
    /// </summary>
    public async Task<User?> AuthenticateAsync(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            return null;

        // Find user by username
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());

        if (user == null || !user.IsActive)
            return null;

        // Verify password
        if (!VerifyPassword(password, user.PasswordHash))
            return null;

        // Update last login time
        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return user;
    }

    /// <summary>
    /// Create a new user account
    /// </summary>
    public async Task<(bool Success, string Message, User? User)> CreateUserAsync(
        string username,
        string email,
        string password,
        string role = "Editor")
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(username))
            return (false, "Username is required", null);

        if (string.IsNullOrWhiteSpace(email))
            return (false, "Email is required", null);

        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            return (false, "Password must be at least 8 characters", null);

        // Check if username already exists
        if (await _context.Users.AnyAsync(u => u.Username.ToLower() == username.ToLower()))
            return (false, "Username already exists", null);

        // Check if email already exists
        if (await _context.Users.AnyAsync(u => u.Email.ToLower() == email.ToLower()))
            return (false, "Email already exists", null);

        // Create new user
        var user = new User
        {
            Username = username,
            Email = email,
            PasswordHash = HashPassword(password),
            Role = role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);

        try
        {
            await _context.SaveChangesAsync();
            return (true, "User created successfully", user);
        }
        catch (Exception ex)
        {
            return (false, $"Error creating user: {ex.Message}", null);
        }
    }

    /// <summary>
    /// Update user password
    /// </summary>
    public async Task<(bool Success, string Message)> UpdatePasswordAsync(
        int userId,
        string currentPassword,
        string newPassword)
    {
        if (string.IsNullOrWhiteSpace(currentPassword) || string.IsNullOrWhiteSpace(newPassword))
            return (false, "Current and new passwords are required");

        if (newPassword.Length < 8)
            return (false, "New password must be at least 8 characters");

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return (false, "User not found");

        // Verify current password
        if (!VerifyPassword(currentPassword, user.PasswordHash))
            return (false, "Current password is incorrect");

        // Update password
        user.PasswordHash = HashPassword(newPassword);

        try
        {
            await _context.SaveChangesAsync();
            return (true, "Password updated successfully");
        }
        catch (Exception ex)
        {
            return (false, $"Error updating password: {ex.Message}");
        }
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    public async Task<User?> GetUserByIdAsync(int userId)
    {
        return await _context.Users.FindAsync(userId);
    }

    /// <summary>
    /// Check if user has a specific role
    /// </summary>
    public bool HasRole(User user, string role)
    {
        return user?.Role == role;
    }

    /// <summary>
    /// Check if user is admin
    /// </summary>
    public bool IsAdmin(User user)
    {
        return HasRole(user, "Admin");
    }
}
