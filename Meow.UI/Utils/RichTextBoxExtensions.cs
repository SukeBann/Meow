using System.Windows;
using System.Windows.Controls;

namespace Meow.UI.Utils;

public static class RichTextBoxExtensions
{
    public static readonly DependencyProperty AutoScrollToEndProperty = DependencyProperty.RegisterAttached(
        "AutoScrollToEnd", typeof(bool), typeof(RichTextBoxExtensions), 
        new PropertyMetadata(default(bool), OnAutoScrollToEndChanged));

    private static void OnAutoScrollToEndChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is RichTextBox richTextBox && e.NewValue is bool enabled)
        {
            if (enabled)
            {
                richTextBox.TextChanged += OnTextChanged;
            }
            else
            {
                richTextBox.TextChanged -= OnTextChanged;
            }
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
        return (bool)element.GetValue(AutoScrollToEndProperty);
    }
}