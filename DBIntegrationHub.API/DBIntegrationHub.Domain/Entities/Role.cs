using DBIntegrationHub.Domain.Shared;

namespace DBIntegrationHub.Domain.Entities;

public class Role : Entity
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;

    // Navigation Property
    public ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();

    private Role() { } // EF Core için

    private Role(string name, string description)
    {
        Name = name;
        Description = description;
    }

    public static Role Create(string name, string description = "")
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Rol adı boş olamaz", nameof(name));

        return new Role(name, description);
    }

    public void Update(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Rol adı boş olamaz", nameof(name));

        Name = name;
        Description = description;
    }

    // Sabit roller
    public static class RoleNames
    {
        public const string Admin = "Admin";
        public const string User = "User";
        public const string Viewer = "Viewer";
    }
}

