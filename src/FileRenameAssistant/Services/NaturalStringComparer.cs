using System.Text.RegularExpressions;

namespace FileRenameAssistant.Services;

public sealed class NaturalStringComparer : IComparer<string>
{
    private static readonly Regex TokenRegex = new(@"(\d+)|(\D+)", RegexOptions.Compiled);

    public int Compare(string? x, string? y)
    {
        if (ReferenceEquals(x, y)) return 0;
        if (x is null) return -1;
        if (y is null) return 1;

        var xt = TokenRegex.Matches(x);
        var yt = TokenRegex.Matches(y);
        var count = Math.Min(xt.Count, yt.Count);

        for (var i = 0; i < count; i++)
        {
            var xs = xt[i].Value;
            var ys = yt[i].Value;

            var xn = long.TryParse(xs, out var xv);
            var yn = long.TryParse(ys, out var yv);

            var result = xn && yn
                ? xv.CompareTo(yv)
                : string.Compare(xs, ys, StringComparison.CurrentCultureIgnoreCase);

            if (result != 0) return result;
        }

        return xt.Count.CompareTo(yt.Count);
    }
}
