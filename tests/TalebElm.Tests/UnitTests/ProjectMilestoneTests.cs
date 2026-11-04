using TalebElm.Domain.Entities;

namespace TalebElm.Tests.UnitTests;

public class ProjectMilestoneTests
{
    [Fact]
    public void ProjectMilestone_HasExpectedDefaultValues()
    {
        var milestone = new ProjectMilestone();

        Assert.Equal(Guid.Empty, milestone.CapstoneProjectId);
        Assert.Equal(0, milestone.Order);
        Assert.Equal(string.Empty, milestone.Title);
        Assert.Equal(string.Empty, milestone.Instructions);
        Assert.Equal(string.Empty, milestone.WorkflowJobName);
    }

    [Fact]
    public void ProjectMilestone_WhenPropertiesAreAssignedCorrectly_ReturnTrue()
    {
        var projectId = Guid.NewGuid();

        var milestone = new ProjectMilestone
        {
            CapstoneProjectId = projectId,
            Order = 1,
            Title = "Milestone 1: Parse Request Line",
            Instructions = "Step-by-step guidance",
            WorkflowJobName = "test-milestone-1"
        };

        Assert.Equal(projectId, milestone.CapstoneProjectId);
        Assert.Equal(1, milestone.Order);
        Assert.Equal("Milestone 1: Parse Request Line", milestone.Title);
        Assert.Equal("Step-by-step guidance", milestone.Instructions);
        Assert.Equal("test-milestone-1", milestone.WorkflowJobName);
    }

    [Fact]
    public void CapstoneProject_WhenNavigationAssignedCorrectly_ReturnTrue()
    {
        var project = new CapstoneProject();

        var milestone = new ProjectMilestone { CapstoneProject = project };

        Assert.Same(project, milestone.CapstoneProject);
    }
}
