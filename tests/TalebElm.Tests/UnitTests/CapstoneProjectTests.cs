using TalebElm.Domain.Entities;

namespace TalebElm.Tests.UnitTests;

public class CapstoneProjectTests
{
    [Fact]
    public void Constructor_SetsDefaults()
    {
        var project = new CapstoneProject();

        Assert.Equal(Guid.Empty, project.TrackId);
        Assert.Equal(string.Empty, project.Title);
        Assert.Equal(string.Empty, project.Description);
        Assert.Equal(Guid.Empty, project.TaskId);
        Assert.NotNull(project.Milestones);
        Assert.Empty(project.Milestones);
    }

    [Fact]
    public void Properties_CanBeAssigned()
    {
        var trackId = Guid.NewGuid();
        var taskId = Guid.NewGuid();

        var project = new CapstoneProject
        {
            TrackId = trackId,
            Title = "Build an Enterprise E-Commerce Engine",
            Description = "Overview",
            TaskId = taskId
        };

        Assert.Equal(trackId, project.TrackId);
        Assert.Equal("Build an Enterprise E-Commerce Engine", project.Title);
        Assert.Equal("Overview", project.Description);
        Assert.Equal(taskId, project.TaskId);
    }

    [Fact]
    public void Milestones_CanAddMilestone()
    {
        var project = new CapstoneProject();

        project.Milestones.Add(new ProjectMilestone { Order = 1 });

        Assert.Single(project.Milestones);
    }
}
