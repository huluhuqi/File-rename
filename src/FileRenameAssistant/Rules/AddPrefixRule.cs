namespace FileRenameAssistant.Rules;

public sealed class AddPrefixRule : IRenameRule
{
    private readonly string _prefix;

    public AddPrefixRule(string prefix)
    {
        _prefix = prefix;
    }

    public string Name => "添加前缀";

    public string Apply(string fileNameWithoutExtension, RenameContext context)
    {
        return _prefix + fileNameWithoutExtension;
    }
}
