namespace FileRenameAssistant.Rules;

public sealed class NumberingRule : IRenameRule
{
    private readonly int _start;
    private readonly int _step;
    private readonly int _digits;

    public NumberingRule(int start, int step, int digits)
    {
        _start = start;
        _step = Math.Max(1, step);
        _digits = Math.Clamp(digits, 1, 8);
    }

    public string Name => "自动编号";

    public string Apply(string fileNameWithoutExtension, RenameContext context)
    {
        var value = _start + context.Index * _step;
        var number = value.ToString().PadLeft(_digits, '0');
        return $"{number}_{fileNameWithoutExtension}";
    }
}
