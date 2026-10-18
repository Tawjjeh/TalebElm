using TalebElm.Domain.Enums;

namespace TalebElm.Domain.Entities;

public class User : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Student;
public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
public ICollection<TrackEnrollment> TrackEnrollments { get; set; } = new List<TrackEnrollment>();
}