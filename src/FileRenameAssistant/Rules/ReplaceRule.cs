namespace FileRenameAssistant.Rules;

public sealed class ReplaceRule : IRenameRule
{
    private readonly string _findText;
    private readonly string _replaceText;

    public ReplaceRule(string findText, string replaceText)
    {
        _findText = findText;
        _replaceText = replaceText;
    }

    public string Name => "查找替换";

    public string Apply(string fileNameWithoutExtension, RenameContext context)
    {
        if (string.IsNullOrEmpty(_findText))
        {
            return fileNameWithoutExtension;
        }

        return fileNameWithoutExtension.Replace(_findText, _replaceText);
    }
}
