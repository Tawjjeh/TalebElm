using System.Text.RegularExpressions;

namespace TalebElm.Infrastructure.Services;

internal static partial class CodeforcesRequestValidator
{
    public const int MaxHandleLength = 24;

    [GeneratedRegex("^[A-Za-z0-9_.-]+$")]
    private static partial Regex HandlePattern();

    public static void ValidateHandle(string handle)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(handle);

        if (handle.Length > MaxHandleLength || !HandlePattern().IsMatch(handle))
            throw new ArgumentException("The handle is not a valid Codeforces handle.", nameof(handle));
    }

    public static void ValidateRange(int from, int count)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(from, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(count, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(count, 10000);
    }
}
