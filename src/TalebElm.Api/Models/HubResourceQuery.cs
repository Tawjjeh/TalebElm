using Microsoft.AspNetCore.Mvc;
using TalebElm.Application.DTOs;

namespace TalebElm.Api.Models;

public sealed record HubResourceQuery
{
    [FromQuery(Name = "search")]
    public string? Search { get; init; }

    public string? Language { get; init; }
    public int? Level { get; init; }
    public bool? IsPaid { get; init; }
    public int? Kind { get; init; }

    public int Page { get; init; } = 1;
    public int Size { get; init; } = 20;

    public ResourceFilterParameters ToFilters() => new(
        Search, Language, Level, IsPaid, Kind, Page, Size);
}
