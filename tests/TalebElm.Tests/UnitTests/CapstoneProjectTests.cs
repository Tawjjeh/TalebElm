using TalebElm.Domain.Entities;

namespace TalebElm.Tests.UnitTests;

public class CapstoneProjectTests
{
    [Fact]
    public void CapstoneProject_HasExpectedDefaultValues()
    {
        var project = new CapstoneProject();

        Assert.Equal(Guid.Empty, project.TrackId);
        Assert.Equal(Guid.Empty, project.TaskId);
        Assert.Equal(string.Empty, project.Title);
        Assert.Equal(string.Empty, project.Description);
        Assert.NotNull(project.Milestones);
        Assert.Empty(project.Milestones);
    }

    [Fact]
    public void CapstoneProject_WhenPropertiesAreAssignedCorrectly_ReturnTrue()
    {
        var trackId = Guid.NewGuid();
        var taskId = Guid.NewGuid();

        var project = new CapstoneProject
        {
            TrackId = trackId,
            TaskId = taskId,
            Title = "Testing Title",
            Description = "Testing Description"
        };

        Assert.Equal(trackId, project.TrackId);
        Assert.Equal(taskId, project.TaskId);
        Assert.Equal("Testing Title", project.Title);
        Assert.Equal("Testing Description", project.Description);
    }

    [Fact]
    public void CapstoneProject_WhenProjectMilestonesAreAddedCorrectly_ReturnTrue()
    {
        var project = new CapstoneProject();
        var milestone = new ProjectMilestone { Order = 1, Title = "Testing Title" };

        project.Milestones.Add(milestone);

        Assert.Single(project.Milestones);
        Assert.Contains(milestone, project.Milestones);
    }
}
