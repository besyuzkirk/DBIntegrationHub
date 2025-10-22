namespace DBIntegrationHub.Domain.Entities;

/// <summary>
/// User-Role arasındaki many-to-many ilişki için junction table
/// </summary>
public class UserRole
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;

    public DateTime AssignedAt { get; set; }

    private UserRole() { } // EF Core için

    private UserRole(Guid userId, Guid roleId)
    {
        UserId = userId;
        RoleId = roleId;
        AssignedAt = DateTime.UtcNow;
    }

    public static UserRole Create(Guid userId, Guid roleId)
    {
        return new UserRole(userId, roleId);
    }
}

