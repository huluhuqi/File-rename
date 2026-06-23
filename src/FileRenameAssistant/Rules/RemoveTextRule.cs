namespace FileRenameAssistant.Rules;

public sealed class RemoveTextRule : IRenameRule
{
    private readonly string _text;

    public RemoveTextRule(string text)
    {
        _text = text;
    }

    public string Name => "删除指定文字";

    public string Apply(string fileNameWithoutExtension, RenameContext context)
    {
        if (string.IsNullOrEmpty(_text))
        {
            return fileNameWithoutExtension;
        }

        return fileNameWithoutExtension.Replace(_text, "");
    }
}
