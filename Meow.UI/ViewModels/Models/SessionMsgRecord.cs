using Lagrange.Core.Message;
using PropertyChanged;

namespace Meow.UI.ViewModels.Models;

/// <summary>
/// 会话中的消息信息
/// </summary>
[AddINotifyPropertyChangedInterface]
public class SessionMsgRecord
{
    public SessionMsgRecord(MessageChain rawMessage)
    {
        RawMessage = rawMessage;
        IsRead = false;
        PseudocodeSnippetInfo = new PseudocodeSnippetInfo(rawMessage);
    }

    /// <summary>
    /// 原始消息
    /// </summary>
    public MessageChain RawMessage { get; set; }
    
    /// <summary>
    /// 消息是否被读取
    /// </summary>
    public bool IsRead { get; set; }
    
    /// <summary>
    /// 伪代码段
    /// </summary>
    public PseudocodeSnippetInfo PseudocodeSnippetInfo { get; set; }
}