using Microsoft.EntityFrameworkCore;
using TalebElm.Domain.Entities;
using TalebElm.Tests.Infrastructure;

namespace TalebElm.Tests.Infrastructure;

/// <summary>
/// Behavioural checks for the capstone milestone constraints. The model metadata and the
/// migrated schema are asserted in <see cref="CapstoneConfigurationTests"/>; these tests prove
/// that the database actually rejects the rows those constraints describe.
/// </summary>
public class ProjectMilestoneConfigurationTests : SqliteTestBase
{
    private readonly CapstoneProject _project;

    public ProjectMilestoneConfigurationTests()
    {
        var track = new Track { Name = "Backend", Description = "Backend track" };
        _project = new CapstoneProject
        {
            Track = track,
            Title = "Capstone",
            Description = "One cumulative project across the track",
            Task = new GitHubTask { TemplateRepository = "tawjjeh/template" }
        };

        DbContext.Tracks.Add(track);
        DbContext.CapstoneProjects.Add(_project);
        DbContext.SaveChanges();
        ClearTracker();
    }

    private ProjectMilestone NewMilestone(int order, string workflowJobName) => new()
    {
        ProjectId = _project.Id,
        Title = $"Milestone {order}",
        Instructions = "Build it.",
        Order = order,
        WorkflowJobName = workflowJobName
    };

    [Fact]
    public async Task DuplicateWorkflowJobNameInTheSameProject_IsRejected()
    {
        DbContext.ProjectMilestones.Add(NewMilestone(1, "build"));
        await SaveChangesAsync();
        ClearTracker();

        DbContext.ProjectMilestones.Add(NewMilestone(2, "build"));

        await Assert.ThrowsAsync<DbUpdateException>(() => DbContext.SaveChangesAsync());
    }

    [Fact]
    public async Task DuplicateOrderInTheSameProject_IsRejected()
    {
        DbContext.ProjectMilestones.Add(NewMilestone(1, "build"));
        await SaveChangesAsync();
        ClearTracker();

        DbContext.ProjectMilestones.Add(NewMilestone(1, "test"));

        await Assert.ThrowsAsync<DbUpdateException>(() => DbContext.SaveChangesAsync());
    }

    [Fact]
    public async Task SameWorkflowJobNameInAnotherProject_IsAllowed()
    {
        DbContext.ProjectMilestones.Add(NewMilestone(1, "build"));
        await SaveChangesAsync();
        ClearTracker();

        var otherTrack = new Track { Name = "Frontend", Description = "Frontend track" };
        var otherProject = new CapstoneProject
        {
            Track = otherTrack,
            Title = "Frontend capstone",
            Description = "One cumulative project across the track",
            Task = new GitHubTask { TemplateRepository = "tawjjeh/frontend-template" }
        };

        DbContext.Tracks.Add(otherTrack);
        DbContext.CapstoneProjects.Add(otherProject);
        DbContext.ProjectMilestones.Add(new ProjectMilestone
        {
            ProjectId = otherProject.Id,
            Title = "Milestone 1",
            Instructions = "Build it.",
            Order = 1,
            WorkflowJobName = "build"
        });

        await DbContext.SaveChangesAsync();

        await using var check = CreateContext();
        Assert.Equal(2, await check.ProjectMilestones.CountAsync());
    }
}
