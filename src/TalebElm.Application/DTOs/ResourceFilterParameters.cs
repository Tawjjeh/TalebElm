namespace TalebElm.Application.DTOs;

public record ResourceFilterParameters(
    string? SearchQuery,
    string? Language,
    int? Level,
    bool? IsPaid,
    int? Kind,
    int PageNumber = 1,
    int PageSize = 20);
