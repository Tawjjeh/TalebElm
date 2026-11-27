using Moq;
using TalebElm.Application.Ordering;
using TalebElm.Domain.Entities;
using TalebElm.Domain.Exceptions;
using TalebElm.Domain.Interfaces;

namespace TalebElm.Tests.UnitTests;

public class SiblingOrderingTests
{
    private static List<Module> Modules(params int[] orders) =>
        orders.Select(o => new Module { Title = $"M{o}", Order = o }).ToList();

    private static List<Lesson> Lessons(params int[] orders) =>
        orders.Select(o => new Lesson { Title = $"L{o}", Order = o }).ToList();

    private static Mock<IUnitOfWork> TransactionalUnitOfWork()
    {
        var uow = new Mock<IUnitOfWork>();
        uow.Setup(u => u.ExecuteInTransactionAsync(
                It.IsAny<Func<CancellationToken, Task>>(),
                It.IsAny<CancellationToken>()))
            .Returns<Func<CancellationToken, Task>, CancellationToken>(
                (action, ct) => action(ct));
        uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);
        return uow;
    }

    [Fact]
    public void PrepareInsert_MiddleOrder_ShiftsSiblingsUp()
    {
        var modules = Modules(1, 2, 3);

        SiblingOrdering.PrepareInsert(modules, 2);

        Assert.Equal([1, 3, 4], modules.Select(m => m.Order).OrderBy(o => o));
    }

    [Fact]
    public void PrepareInsert_FirstOrder_ShiftsAllUp()
    {
        var modules = Modules(1, 2, 3);

        SiblingOrdering.PrepareInsert(modules, 1);

        Assert.Equal([2, 3, 4], modules.Select(m => m.Order).OrderBy(o => o));
    }

    [Fact]
    public void PrepareInsert_AppendOrder_ShiftsNothing()
    {
        var modules = Modules(1, 2, 3);

        SiblingOrdering.PrepareInsert(modules, 4);

        Assert.Equal([1, 2, 3], modules.Select(m => m.Order).OrderBy(o => o));
    }

    [Fact]
    public void PrepareInsert_Lessons_ShiftsTheSameWay()
    {
        var lessons = Lessons(1, 2);

        SiblingOrdering.PrepareInsert(lessons, 1);

        Assert.Equal([2, 3], lessons.Select(l => l.Order).OrderBy(o => o));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    public void PrepareInsert_OutOfRangeOrder_ThrowsValidation(int order)
    {
        var modules = Modules(1, 2, 3);

        Assert.Throws<ValidationException>(() => SiblingOrdering.PrepareInsert(modules, order));
    }

    [Fact]
    public void PrepareDelete_MiddleOrder_ShiftsSiblingsDown()
    {
        var modules = Modules(1, 2, 3, 4);
        modules.Remove(modules.Single(m => m.Order == 2));

        SiblingOrdering.PrepareDelete(modules, 2);

        Assert.Equal([1, 2, 3], modules.Select(m => m.Order).OrderBy(o => o));
    }

    [Fact]
    public void PrepareDelete_LastOrder_ShiftsNothing()
    {
        var modules = Modules(1, 2, 3);
        modules.Remove(modules.Single(m => m.Order == 3));

        SiblingOrdering.PrepareDelete(modules, 3);

        Assert.Equal([1, 2], modules.Select(m => m.Order).OrderBy(o => o));
    }

    [Fact]
    public void PrepareDelete_Lessons_ShiftsTheSameWay()
    {
        var lessons = Lessons(1, 2, 3);
        lessons.Remove(lessons.Single(l => l.Order == 1));

        SiblingOrdering.PrepareDelete(lessons, 1);

        Assert.Equal([1, 2], lessons.Select(l => l.Order).OrderBy(o => o));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    public void PrepareDelete_OutOfRangeOrder_ThrowsValidation(int order)
    {
        var modules = Modules(1, 2, 3);

        Assert.Throws<ValidationException>(() => SiblingOrdering.PrepareDelete(modules, order));
    }

    [Fact]
    public void PrepareDelete_SiblingStillAtOrder_ThrowsValidation()
    {
        var modules = Modules(1, 2, 3);

        Assert.Throws<ValidationException>(() => SiblingOrdering.PrepareDelete(modules, 2));
    }

    [Fact]
    public async Task MoveAsync_MoveDown_ShiftsIntermediateRangeAndPlacesMover()
    {
        var uow = TransactionalUnitOfWork();
        var modules = Modules(1, 2, 3, 4, 5);
        var moving = modules[1]; // Order 2 -> 5

        await SiblingOrdering.MoveAsync(uow.Object, modules, moving, 5);

        Assert.Equal([1, 2, 3, 4, 5], modules.Select(m => m.Order).OrderBy(o => o));
        Assert.Equal(5, moving.Order);
        Assert.Equal("M3", modules.Single(m => m.Order == 2).Title);
        Assert.Equal("M5", modules.Single(m => m.Order == 4).Title);
        uow.Verify(u => u.ExecuteInTransactionAsync(It.IsAny<Func<CancellationToken, Task>>(), It.IsAny<CancellationToken>()), Times.Once);
        uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task MoveAsync_MoveUp_ShiftsIntermediateRangeAndPlacesMover()
    {
        var uow = TransactionalUnitOfWork();
        var modules = Modules(1, 2, 3, 4, 5);
        var moving = modules[4]; // Order 5 -> 2

        await SiblingOrdering.MoveAsync(uow.Object, modules, moving, 2);

        Assert.Equal([1, 2, 3, 4, 5], modules.Select(m => m.Order).OrderBy(o => o));
        Assert.Equal(2, moving.Order);
        Assert.Equal("M2", modules.Single(m => m.Order == 3).Title);
        Assert.Equal("M4", modules.Single(m => m.Order == 5).Title);
        uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task MoveAsync_SameOrder_IsNoOpWithoutSaving()
    {
        var uow = TransactionalUnitOfWork();
        var modules = Modules(1, 2, 3);
        var moving = modules[1];

        await SiblingOrdering.MoveAsync(uow.Object, modules, moving, 2);

        Assert.Equal([1, 2, 3], modules.Select(m => m.Order).OrderBy(o => o));
        uow.Verify(u => u.ExecuteInTransactionAsync(It.IsAny<Func<CancellationToken, Task>>(), It.IsAny<CancellationToken>()), Times.Never);
        uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(4)]
    public async Task MoveAsync_OutOfRangeOrder_ThrowsValidationWithoutSaving(int newOrder)
    {
        var uow = TransactionalUnitOfWork();
        var modules = Modules(1, 2, 3);

        await Assert.ThrowsAsync<ValidationException>(() =>
            SiblingOrdering.MoveAsync(uow.Object, modules, modules[0], newOrder));

        uow.Verify(u => u.ExecuteInTransactionAsync(It.IsAny<Func<CancellationToken, Task>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task MoveAsync_MoverNotInSiblingList_ThrowsValidation()
    {
        var uow = TransactionalUnitOfWork();
        var modules = Modules(1, 2, 3);
        var outsider = new Module { Title = "X", Order = 1 };

        await Assert.ThrowsAsync<ValidationException>(() =>
            SiblingOrdering.MoveAsync(uow.Object, modules, outsider, 2));
    }

    [Fact]
    public async Task MoveAsync_Lessons_MovesTheSameWay()
    {
        var uow = TransactionalUnitOfWork();
        var lessons = Lessons(1, 2, 3);
        var moving = lessons[0]; // Order 1 -> 3

        await SiblingOrdering.MoveAsync(uow.Object, lessons, moving, 3);

        Assert.Equal([1, 2, 3], lessons.Select(l => l.Order).OrderBy(o => o));
        Assert.Equal(3, moving.Order);
        Assert.Equal("L2", lessons.Single(l => l.Order == 1).Title);
    }
}
