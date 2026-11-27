namespace TalebElm.Infrastructure.Services;

internal sealed record CodeforcesEnvelope<T>(
    string Status,
    string? Comment,
    T? Result);
