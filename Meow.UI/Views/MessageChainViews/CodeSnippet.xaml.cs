using System.Windows;
using System.Windows.Controls;
using Lagrange.Core.Message;
using Lagrange.Core.Message.Entity;
using Meow.UI.ViewModels;
using Meow.UI.ViewModels.Models;
using Brushes = System.Windows.Media.Brushes;
using Button = System.Windows.Controls.Button;

namespace Meow.UI.Views.MessageChainViews;

public partial class CodeSnippet
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
            UIElement uiElement = messageEntity switch
            {
                TextEntity textEntity => new TextBlock {Text = textEntity.Text, Foreground = Brushes.White},
                ImageEntity imageEntity => new ImageEntityView(imageEntity),
                _ => new TextBlock{Text = $"[{messageEntity.GetType()}]"}
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
        if (DataContext is not SessionMsgRecord sessionMsgRecord)
        {
            return;
        }

        if (!SimulationViewModel.GetSessionById(sessionMsgRecord.SessionId, out var simulationSession))
        {
            return;
        }
        
        simulationSession.EditingMessageChain.Add(new ForwardEntity(sessionMsgRecord.RawMessage));
    }

    /// <summary>
    /// 删除一条记录
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Delete_OnClick(object sender, RoutedEventArgs e)
    {
        // 表面上是删除其实是隐藏
        // 等有必要改成真正地删除再改
        Visibility = Visibility.Collapsed;
    }
}