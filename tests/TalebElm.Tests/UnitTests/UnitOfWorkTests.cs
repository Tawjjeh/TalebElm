using TalebElm.Domain.Entities;
using TalebElm.Domain.Enums;
using TalebElm.Domain.Interfaces;
using TalebElm.Infrastructure.Repositories;
using TalebElm.Tests.Infrastructure;

namespace TalebElm.Tests.UnitTests;

public class UnitOfWorkTests : SqliteTestBase
{
    [Fact]
    public async Task SaveChangesAsync_PersistsTrackAndModule_InOneCommit()
    {
        var track = new Track
        {
            Id = Guid.NewGuid(),
            Name = "Backend with .NET",
            Description = "Backend development track using .NET",
            Status = TrackStatus.Published
        };

        var module = new Module
        {
            Id = Guid.NewGuid(),
            Title = "Introduction to C#",
            Summary = "Basic syntax and core concepts",
            Order = 1,
            TrackId = track.Id
        };

        var unitOfWork = new UnitOfWork(DbContext);

        await unitOfWork.Tracks.AddAsync(track);
        await unitOfWork.Modules.AddAsync(module);

        await using var secondContext = CreateContext();
        Assert.Null(await secondContext.Tracks.FindAsync(track.Id));
        Assert.Null(await secondContext.Modules.FindAsync(module.Id));

        await unitOfWork.SaveChangesAsync();

        Assert.NotNull(await secondContext.Tracks.FindAsync(track.Id));
        Assert.NotNull(await secondContext.Modules.FindAsync(module.Id));
    }

    [Fact]
    public void Repositories_AreExposedThroughTheInterface()
    {
        IUnitOfWork unitOfWork = new UnitOfWork(DbContext);

        Assert.NotNull(unitOfWork.Users);
        Assert.NotNull(unitOfWork.Tracks);
        Assert.NotNull(unitOfWork.Modules);
        Assert.NotNull(unitOfWork.Lessons);
        Assert.NotNull(unitOfWork.Exams);
        Assert.NotNull(unitOfWork.UserProgresses);
        Assert.NotNull(unitOfWork.TrackEnrollments);
    }
}