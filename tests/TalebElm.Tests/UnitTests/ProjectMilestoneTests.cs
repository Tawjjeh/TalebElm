using TalebElm.Domain.Entities;

namespace TalebElm.Tests.UnitTests;

public class ProjectMilestoneTests
{
    [Fact]
    public void Constructor_SetsDefaults()
    {
        var milestone = new ProjectMilestone();

        Assert.Equal(Guid.Empty, milestone.ProjectId);
        Assert.Null(milestone.ModuleId);
        Assert.Equal(string.Empty, milestone.Title);
        Assert.Equal(string.Empty, milestone.Instructions);
        Assert.Equal(0, milestone.Order);
        Assert.Equal(string.Empty, milestone.WorkflowJobName);
    }

    [Fact]
    public void Properties_CanBeAssigned()
    {
        var projectId = Guid.NewGuid();
        var moduleId = Guid.NewGuid();

        var milestone = new ProjectMilestone
        {
            ProjectId = projectId,
            ModuleId = moduleId,
            Title = "Milestone 1: Domain Model",
            Instructions = "Step-by-step guidance",
            Order = 1,
            WorkflowJobName = "test-milestone-1"
        };

        Assert.Equal(projectId, milestone.ProjectId);
        Assert.Equal(moduleId, milestone.ModuleId);
        Assert.Equal("Milestone 1: Domain Model", milestone.Title);
        Assert.Equal("Step-by-step guidance", milestone.Instructions);
        Assert.Equal(1, milestone.Order);
        Assert.Equal("test-milestone-1", milestone.WorkflowJobName);
    }
}
