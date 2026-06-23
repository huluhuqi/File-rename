using System.Collections.Generic;
using System.Text;

namespace FileRenameAssistant.Services;

/// <summary>
/// 简繁字符表，仅覆盖最常用的字。
/// 完整词库可在后续版本中通过 OpenCC 数据替换。
/// </summary>
public static class ChineseConverter
{
    private static readonly Dictionary<char, char> SToT = new()
    {
        ['实'] = '實', ['验'] = '驗', ['图'] = '圖', ['片'] = '片', ['数'] = '數', ['据'] = '據',
        ['报'] = '報', ['告'] = '告', ['项'] = '項', ['目'] = '目', ['会'] = '會', ['议'] = '議',
        ['记'] = '記', ['录'] = '錄', ['资'] = '資', ['料'] = '料', ['学'] = '學', ['术'] = '術',
        ['论'] = '論', ['文'] = '文', ['总'] = '總', ['结'] = '結', ['备'] = '備', ['份'] = '份',
        ['压'] = '壓', ['缩'] = '縮', ['视'] = '視', ['频'] = '頻', ['音'] = '音', ['乐'] = '樂',
        ['课'] = '課', ['程'] = '程', ['书'] = '書', ['籍'] = '籍', ['办'] = '辦', ['公'] = '公',
        ['国'] = '國', ['际'] = '際', ['标'] = '標', ['准'] = '準', ['号'] = '號', ['码'] = '碼',
        ['软'] = '軟', ['件'] = '件', ['硬'] = '硬', ['驱'] = '驅', ['动'] = '動', ['网'] = '網',
        ['络'] = '絡', ['页'] = '頁', ['面'] = '面', ['长'] = '長', ['宽'] = '寬', ['编'] = '編',
        ['辑'] = '輯', ['图'] = '圖', ['像'] = '像', ['处'] = '處', ['理'] = '理', ['计'] = '計'
    };

    private static readonly Dictionary<char, char> TToS = BuildReverse();

    private static Dictionary<char, char> BuildReverse()
    {
        var map = new Dictionary<char, char>();
        foreach (var kv in SToT)
        {
            map[kv.Value] = kv.Key;
        }
        return map;
    }

    public static string S2T(string text) => Convert(text, SToT);

    public static string T2S(string text) => Convert(text, TToS);

    private static string Convert(string text, IReadOnlyDictionary<char, char> table)
    {
        if (string.IsNullOrEmpty(text)) return text;
        var sb = new StringBuilder(text.Length);
        foreach (var ch in text)
        {
            sb.Append(table.TryGetValue(ch, out var mapped) ? mapped : ch);
        }
        return sb.ToString();
    }
}
