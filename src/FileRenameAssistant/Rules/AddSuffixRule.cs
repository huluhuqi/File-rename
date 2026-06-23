namespace FileRenameAssistant.Rules;

public sealed class AddSuffixRule : IRenameRule
{
    private readonly string _suffix;

    public AddSuffixRule(string suffix)
    {
        _suffix = suffix;
    }

    public string Name => "添加后缀";

    public string Apply(string fileNameWithoutExtension, RenameContext context)
    {
        return fileNameWithoutExtension + _suffix;
    }
}
