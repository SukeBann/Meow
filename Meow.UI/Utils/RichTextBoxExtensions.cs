using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using AngleSharp.Html.Dom;
using Meow.Utils;
using Microsoft.VisualBasic;
using Serilog;

namespace Meow.UI.Utils;

public static class RichTextBoxExtensions
{
    public static readonly DependencyProperty AutoScrollToEndProperty = DependencyProperty.RegisterAttached(
        "AutoScrollToEnd", typeof(bool), typeof(RichTextBoxExtensions),
        new PropertyMetadata(default(bool), OnAutoScrollToEndChanged));

    private static void OnAutoScrollToEndChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not RichTextBox richTextBox)
        {
            return;
        }

        if (e.NewValue is not bool enabled)
        {
            return;
        }

        if (enabled)
        {
            richTextBox.TextChanged += OnTextChanged;
        }
        else
        {
            richTextBox.TextChanged -= OnTextChanged;
        }
    }

    /// <summary>
    /// 当前字符数量
    /// </summary>
    private static int CurrentCharCount { get; set; }

    /// <summary>
    /// 最大字符数量
    /// </summary>
    private const int MaxCharCount = 40000;

    public static void OnTextChangedAndClear(object sender, TextChangedEventArgs e)
    {
        if (sender is RichTextBox richTextBox)
        {
            // As there is only added text, the Change List will contain only one item
            var textChange = e.Changes.First();
            var changeStart = richTextBox.Document.ContentStart.GetPositionAtOffset(textChange.Offset);
            var changeEnd =
                richTextBox.Document.ContentStart.GetPositionAtOffset(textChange.Offset + textChange.AddedLength);
            var addedText = new TextRange(changeStart, changeEnd).Text;

            // Now process the addedText as required for your application
            // For example to find and replace image tags with real images
            // ReplaceImageTagsWithImages(addedText, richTextBox);
            CheckTextLengthAndClean(e, richTextBox);
        }
    }

    private static void CheckTextLengthAndClean(TextChangedEventArgs e, RichTextBox richTextBox)
    {
        foreach (var change in e.Changes)
        {
            CurrentCharCount += change.AddedLength - change.RemovedLength;
        }

        if (CurrentCharCount >= MaxCharCount)
        {
            richTextBox.Document.Blocks.Clear();
            CurrentCharCount = 0;
            Ioc.GetService<ILogger>()?.Information("自动清理日志, 字符长度超过：{MaxCharCount}", MaxCharCount);
        }
    }

    private static void OnTextChanged(object sender, TextChangedEventArgs textChangedEventArgs)
    {
        if (sender is RichTextBox richTextBox)
        {
            richTextBox.ScrollToEnd();
        }
    }

    public static void SetAutoScrollToEnd(RichTextBox element, bool value)
    {
        element.SetValue(AutoScrollToEndProperty, value);
    }

    public static bool GetAutoScrollToEnd(RichTextBox element)
    {
        return (bool) element.GetValue(AutoScrollToEndProperty);
    }
}