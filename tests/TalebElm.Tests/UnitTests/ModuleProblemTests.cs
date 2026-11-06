using TalebElm.Domain.Entities;

namespace TalebElm.Tests.UnitTests;
public class ModuleProblemTests
{
    [Fact]
    public void ModuleProblem_HasExpectedDefaultValues()
    {
        var moduleProblem = new ModuleProblem();
        Assert.True(moduleProblem.IsRequired);
        Assert.Equal(string.Empty, moduleProblem.Title);
        Assert.Equal(string.Empty, moduleProblem.ExternalIdentifier);
        Assert.Null(moduleProblem.Rating);
        Assert.Null(moduleProblem.GitHubTaskId);
    }
}
