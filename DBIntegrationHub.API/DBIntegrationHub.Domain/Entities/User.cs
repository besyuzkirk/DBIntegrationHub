using DBIntegrationHub.Domain.Shared;

namespace DBIntegrationHub.Domain.Entities;

public class User : Entity
{
    public string Username { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    
    // Navigation Property
    public ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();

    private User() { } // EF Core için

    private User(string username, string email, string passwordHash)
    {
        Username = username;
        Email = email;
        PasswordHash = passwordHash;
        IsActive = true;
    }

    public static User Create(string username, string email, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Kullanıcı adı boş olamaz", nameof(username));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email boş olamaz", nameof(email));

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Şifre hash'i boş olamaz", nameof(passwordHash));

        return new User(username, email, passwordHash);
    }

    public void UpdatePassword(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Şifre hash'i boş olamaz", nameof(passwordHash));

        PasswordHash = passwordHash;
    }

    public void UpdateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email boş olamaz", nameof(email));

        Email = email;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;

    public void UpdateLastLogin()
    {
        LastLoginAt = DateTime.UtcNow;
    }
}

