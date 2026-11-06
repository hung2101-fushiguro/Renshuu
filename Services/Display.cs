using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Html;

namespace TracNghiemWeb.Services;

public static class Display
{
    private static readonly Regex UnderlineMarkup = new(@"&lt;(/?)[uU]&gt;", RegexOptions.Compiled);
    private static readonly Regex StarMarkup = new(
        @"&lt;star\s*/?&gt;\s*(?:&lt;/star\s*&gt;)?",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex BlankPairMarkup = new(
        @"&lt;blank\s*/?&gt;(.*?)&lt;/blank\s*&gt;",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
    private static readonly Regex BlankOpenMarkup = new(
        @"&lt;blank\s*/?&gt;",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex BlankCloseMarkup = new(
        @"&lt;/blank\s*&gt;",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex UnderscoreMarkup = new(@"[_＿]{3,}", RegexOptions.Compiled);

    public static IHtmlContent RichText(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return HtmlString.Empty;
        }

        var encoded = HtmlEncoder.Default.Encode(text);
        encoded = UnderlineMarkup.Replace(encoded, "<$1u>");
        encoded = StarMarkup.Replace(encoded, "<span class=\"star\">★</span>");
        encoded = BlankPairMarkup.Replace(encoded, "<span class=\"blank\">$1</span>");
        encoded = BlankOpenMarkup.Replace(encoded, "<span class=\"blank\"></span>");
        encoded = BlankCloseMarkup.Replace(encoded, string.Empty);
        encoded = UnderscoreMarkup.Replace(encoded, "<span class=\"blank\"></span>");
        return new HtmlString(encoded);
    }

    public static string OptionLabel(string? optionStyle, string optionKey)
    {
        if (optionStyle == "1234" && optionKey.Length == 1 && optionKey[0] is >= 'A' and <= 'D')
        {
            return (optionKey[0] - 'A' + 1).ToString();
        }

        return optionKey;
    }

    public static string Duration(TimeSpan time)
    {
        if (time < TimeSpan.Zero)
        {
            time = TimeSpan.Zero;
        }

        if (time.TotalHours >= 1)
        {
            return $"{(int)time.TotalHours} giờ {time.Minutes} phút";
        }

        if (time.TotalMinutes >= 1)
        {
            return $"{(int)time.TotalMinutes} phút {time.Seconds} giây";
        }

        return $"{time.Seconds} giây";
    }

    public static string Clock(TimeSpan time)
    {
        if (time < TimeSpan.Zero)
        {
            time = TimeSpan.Zero;
        }

        return time.TotalHours >= 1
            ? time.ToString(@"h\:mm\:ss")
            : time.ToString(@"m\:ss");
    }

    public static string Percentage(double value) => $"{Math.Round(value, 1):0.#}%";
}
