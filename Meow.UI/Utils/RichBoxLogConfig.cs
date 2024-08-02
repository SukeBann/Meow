using System.Windows.Controls;
using Serilog;
using Serilog.Sinks.RichTextBox.Themes;

namespace Meow.UI.Utils;

public class RichBoxLogConfig
{
    /// <summary>
    /// 为日志配置添加向wpf富文本框的输出
    /// </summary>
    /// <param name="loggerConfiguration">logger配置</param>
    /// <param name="richTextBox">目标富文本框</param>
    /// <returns></returns>
    public static LoggerConfiguration GetLogRichBoxPanel(LoggerConfiguration loggerConfiguration,
        RichTextBox richTextBox)
    {
        return loggerConfiguration.WriteTo.RichTextBox(richTextBox, theme: new RichTextBoxConsoleTheme(
            new Dictionary<RichTextBoxThemeStyle, RichTextBoxConsoleThemeStyle>()
            {
                [RichTextBoxThemeStyle.Text] = new() {Foreground = ConsoleHtmlColor.Gray},
                [RichTextBoxThemeStyle.SecondaryText] = new() {Foreground = ConsoleHtmlColor.DarkGray},
                [RichTextBoxThemeStyle.TertiaryText] = new() {Foreground = ConsoleHtmlColor.DarkGray},
                [RichTextBoxThemeStyle.Invalid] = new() {Foreground = ConsoleHtmlColor.Yellow},
                [RichTextBoxThemeStyle.Null] = new() {Foreground = ConsoleHtmlColor.White},
                [RichTextBoxThemeStyle.Name] = new() {Foreground = ConsoleHtmlColor.White},
                [RichTextBoxThemeStyle.String] = new() {Foreground = ConsoleHtmlColor.White},
                [RichTextBoxThemeStyle.Number] = new() {Foreground = ConsoleHtmlColor.White},
                [RichTextBoxThemeStyle.Boolean] =
                    new() {Foreground = ConsoleHtmlColor.White},
                [RichTextBoxThemeStyle.Scalar] = new() {Foreground = ConsoleHtmlColor.White},
                [RichTextBoxThemeStyle.LevelVerbose] = new()
                    {Foreground = ConsoleHtmlColor.Gray, Background = ConsoleHtmlColor.DarkGray},
                [RichTextBoxThemeStyle.LevelDebug] = new()
                    {Foreground = ConsoleHtmlColor.White, Background = ConsoleHtmlColor.DarkGray},
                [RichTextBoxThemeStyle.LevelInformation] = new()
                    {Foreground = ConsoleHtmlColor.White, Background = ConsoleHtmlColor.Blue},
                [RichTextBoxThemeStyle.LevelWarning] = new()
                    {Foreground = ConsoleHtmlColor.DarkGray, Background = ConsoleHtmlColor.Yellow},
                [RichTextBoxThemeStyle.LevelError] = new()
                    {Foreground = ConsoleHtmlColor.White, Background = ConsoleHtmlColor.Red},
                [RichTextBoxThemeStyle.LevelFatal] = new()
                    {Foreground = ConsoleHtmlColor.White, Background = ConsoleHtmlColor.Red},
            }));
    }
}