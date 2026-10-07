using System;
using System.ComponentModel.DataAnnotations;

namespace BravoWeb.Models;

/// <summary>
/// Represents an admin user for the CMS
/// </summary>
public class User
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Username { get; set; } = null!;

    [Required]
    public string PasswordHash { get; set; } = null!;

    [Required, EmailAddress, StringLength(255)]
    public string Email { get; set; } = null!;

    /// <summary>
    /// User role: "Admin" for full access, "Editor" for content editing
    /// </summary>
    [Required, StringLength(50)]
    public string Role { get; set; } = "Editor";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastLoginAt { get; set; }

    public bool IsActive { get; set; } = true;
}
