using TalebElm.Domain.Exceptions;
using TalebElm.Domain.Interfaces;

namespace TalebElm.Application.Ordering;

/// <summary>
/// Shared sibling-<see cref="IOrderedSibling.Order"/> maintenance for Modules
/// (siblings share a Track) and Lessons (siblings share a Module), per
/// Decisions 10 and 12: <c>Order</c> values stay contiguous (<c>1, 2, 3, ...</c>)
/// and unique within the parent. Compaction is strictly within siblings; the
/// helpers never touch another parent's rows.
///
/// How the shifts stay safe against SQLite's eager unique-index check
/// (one <c>UPDATE</c> per row, checked per statement):
/// <list type="bullet">
/// <item>Insert and delete shifts are a single <c>SaveChangesAsync</c>: EF Core
/// orders the updates so no transient duplicate <c>Order</c> ever exists
/// (verified against real SQLite).</item>
/// <item>A move cannot be one save: the moving row and the intermediate range
/// form a dependency cycle (the mover's target is occupied until the range
/// shifts, and the range cannot shift until the mover vacates), and EF Core
/// refuses it with a circular-dependency error. <see cref="MoveAsync{T}"/>
/// therefore parks the mover above the current max (a free slot), shifts the
/// range, saves, then places the mover — two saves inside one
/// <see cref="IUnitOfWork.ExecuteInTransactionAsync"/> transaction, so the
/// intermediate parked state is never visible.</item>
/// </list>
/// Callers load the siblings first, tracked, via
/// <c>IModuleRepository.GetByTrackIdForUpdateAsync</c> /
/// <c>ILessonRepository.GetByModuleIdForUpdateAsync</c> (the read
/// <c>GetByTrackIdAsync</c> / <c>GetByModuleIdAsync</c> return detached copies,
/// whose mutations would never be saved), and pass them in; the helpers mutate
/// the tracked entities in place.
/// </summary>
public static class SiblingOrdering
{
    /// <summary>
    /// Makes room for a new sibling at <paramref name="order"/> (1-based):
    /// siblings with <c>Order &gt;= order</c> shift up by 1, highest first, so
    /// every target slot is free. The caller adds the new entity and calls
    /// <c>SaveChangesAsync</c> once. Appending at <c>count + 1</c> shifts
    /// nothing.
    /// </summary>
    public static void PrepareInsert<T>(IReadOnlyList<T> siblings, int order) where T : IOrderedSibling
    {
        if (order < 1 || order > siblings.Count + 1)
            throw new ValidationException($"Order must be between 1 and {siblings.Count + 1}.");

        foreach (var sibling in siblings.Where(s => s.Order >= order).OrderByDescending(s => s.Order))
            sibling.Order += 1;
    }

    /// <summary>
    /// Compacts the siblings after the row at <paramref name="order"/> was
    /// removed: siblings with <c>Order &gt; order</c> shift down by 1, lowest
    /// first, into the freed slot. <paramref name="siblings"/> are the
    /// remaining siblings (the deleted row already removed from the list, so
    /// no sibling may still hold <paramref name="order"/>); the caller
    /// removes the entity (after the #251 conflict guards) and calls
    /// <c>SaveChangesAsync</c> once.
    /// </summary>
    public static void PrepareDelete<T>(IReadOnlyList<T> siblings, int order) where T : IOrderedSibling
    {
        if (order < 1 || order > siblings.Count + 1)
            throw new ValidationException($"Order must be between 1 and {siblings.Count + 1}.");
        if (siblings.Any(s => s.Order == order))
            throw new ValidationException($"A sibling still exists at Order {order}; remove it before compacting.");

        foreach (var sibling in siblings.Where(s => s.Order > order).OrderBy(s => s.Order))
            sibling.Order -= 1;
    }

    /// <summary>
    /// Moves <paramref name="moving"/> from its current <c>Order</c> to
    /// <paramref name="newOrder"/> (1-based, within the sibling list),
    /// shifting the intermediate range to stay contiguous. Moving to the
    /// current order is a no-op. The move runs in two saves inside one
    /// transaction (see the class remarks for why one save cannot work).
    /// </summary>
    public static async Task MoveAsync<T>(
        IUnitOfWork unitOfWork,
        IReadOnlyList<T> siblings,
        T moving,
        int newOrder,
        CancellationToken ct = default) where T : IOrderedSibling
    {
        var oldOrder = moving.Order;

        if (!siblings.Any(s => ReferenceEquals(s, moving)))
            throw new ValidationException("The moving sibling must be part of the sibling list.");
        if (newOrder < 1 || newOrder > siblings.Count)
            throw new ValidationException($"Order must be between 1 and {siblings.Count}.");
        if (newOrder == oldOrder)
            return;

        await unitOfWork.ExecuteInTransactionAsync(async ct2 =>
        {
            // Phase 1: park the mover above the current max (a free slot),
            // then shift the intermediate range into the vacated slot.
            // Moving down (old < new): the range (old, new] shifts down by 1,
            // lowest first. Moving up (old > new): the range [new, old)
            // shifts up by 1, highest first.
            var maxOrder = siblings.Max(s => s.Order);
            moving.Order = maxOrder + 1;

            if (oldOrder < newOrder)
            {
                foreach (var sibling in siblings.Where(s => s.Order > oldOrder && s.Order <= newOrder).OrderBy(s => s.Order))
                    sibling.Order -= 1;
            }
            else
            {
                foreach (var sibling in siblings.Where(s => s.Order >= newOrder && s.Order < oldOrder).OrderByDescending(s => s.Order))
                    sibling.Order += 1;
            }

            await unitOfWork.SaveChangesAsync(ct2);

            // Phase 2: the target slot is free now; place the mover.
            moving.Order = newOrder;
            await unitOfWork.SaveChangesAsync(ct2);
        }, ct);
    }
}
