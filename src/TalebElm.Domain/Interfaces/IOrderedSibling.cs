namespace TalebElm.Domain.Interfaces;

/// <summary>
/// A content entity positioned among siblings by a 1-based <see cref="Order"/>.
/// Implemented by <see cref="Entities.Module"/> (siblings share a Track) and
/// <see cref="Entities.Lesson"/> (siblings share a Module).
/// </summary>
public interface IOrderedSibling
{
    int Order { get; set; }
}
