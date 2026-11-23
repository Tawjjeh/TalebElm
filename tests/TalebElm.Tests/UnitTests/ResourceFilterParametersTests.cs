using TalebElm.Application.DTOs;

namespace TalebElm.Tests.UnitTests;

public class ResourceFilterParametersTests
{
    [Fact]
    public void Constructor_UsesDefaultPagingAndNullFilters()
    {
        var parameters = new ResourceFilterParameters();

        Assert.Null(parameters.SearchQuery);
        Assert.Null(parameters.Language);
        Assert.Null(parameters.Level);
        Assert.Null(parameters.IsPaid);
        Assert.Null(parameters.Kind);
        Assert.Equal(1, parameters.PageNumber);
        Assert.Equal(20, parameters.PageSize);
    }

    [Fact]
    public void Constructor_PreservesCustomFiltersAndPaging()
    {
        var parameters = new ResourceFilterParameters(
            SearchQuery: "calculus",
            Language: "en",
            Level: 3,
            IsPaid: false,
            Kind: 2,
            PageNumber: 4,
            PageSize: 50);

        Assert.Equal("calculus", parameters.SearchQuery);
        Assert.Equal("en", parameters.Language);
        Assert.Equal(3, parameters.Level);
        Assert.False(parameters.IsPaid);
        Assert.Equal(2, parameters.Kind);
        Assert.Equal(4, parameters.PageNumber);
        Assert.Equal(50, parameters.PageSize);
    }

    [Fact]
    public void RecordEquality_ConsidersAllFilterAndPagingValues()
    {
        var first = new ResourceFilterParameters("algebra", "en", 2, true, 1, 2, 25);
        var equal = new ResourceFilterParameters("algebra", "en", 2, true, 1, 2, 25);
        var different = first with { PageSize = 30 };

        Assert.Equal(first, equal);
        Assert.Equal(first.GetHashCode(), equal.GetHashCode());
        Assert.NotEqual(first, different);
    }

    [Fact]
    public void WithExpression_CopiesFilterStateAndAllowsOverrides()
    {
        var original = new ResourceFilterParameters("physics", "ar", 4, false, 3, 5, 40);

        var copy = original with { PageNumber = 6 };

        Assert.NotSame(original, copy);
        Assert.Equal("physics", copy.SearchQuery);
        Assert.Equal("ar", copy.Language);
        Assert.Equal(4, copy.Level);
        Assert.False(copy.IsPaid);
        Assert.Equal(3, copy.Kind);
        Assert.Equal(6, copy.PageNumber);
        Assert.Equal(40, copy.PageSize);

        Assert.Equal(5, original.PageNumber);
    }
}
