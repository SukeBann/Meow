using System.Windows;
using System.Windows.Controls;
using Lagrange.Core.Message;
using Lagrange.Core.Message.Entity;
using Meow.UI.ViewModels.Models;
using Brushes = System.Windows.Media.Brushes;
using Button = System.Windows.Controls.Button;

namespace Meow.UI.Views.MessageChainViews;

public partial class CodeSnippet : UserControl
{
    public CodeSnippet()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is not SessionMsgRecord sessionMsgRecord)
        {
            return;
        }

        ParseMessageChain(sessionMsgRecord.PseudocodeSnippetInfo.RawMessage);
    }

    private void ParseMessageChain(MessageChain messageChain)
    {
        foreach (var messageEntity in messageChain)
        {
            UIElement? uiElement = messageEntity switch
            {
                TextEntity textEntity => new TextBlock() {Text = textEntity.Text, Foreground = Brushes.White},
                ImageEntity imageEntity => new ImageEntityView(imageEntity),
                _ => new TextBlock() {Text = $"[{messageEntity.GetType()}]"}
            };

            MessageContainer.Children.Add(uiElement);
        }
    }

    /// <summary>
    /// 详情
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ShowDetail_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button {DataContext: PseudocodeSnippetInfo pseudocode})
        {
            return;
        }

        pseudocode.OpenTrueMsgPop = true;
        if (!pseudocode.IsReadMsg)
        {
            pseudocode.IsReadMsg = true;
        }
    }

    /// <summary>
    /// 回复
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <exception cref="NotImplementedException"></exception>
    private void Reply_OnClick(object sender, RoutedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private void Delete_OnClick(object sender, RoutedEventArgs e)
    {
        throw new NotImplementedException();
    }
}