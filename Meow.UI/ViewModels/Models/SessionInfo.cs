using System.Net.Mime;
using System.Reactive.Linq;
using System.Windows;
using Lagrange.Core.Event.EventArg;
using Lagrange.Core.Message;
using Meow.Core.Model.Base;

namespace Meow.UI.ViewModels.Models;

/// <summary>
/// 会话相关信息
/// </summary>
public class SessionInfo
{
    public SessionInfo(Meow.Core.Meow host, MessageChain.MessageType messageType, uint sessionUin, string sessionName, string? sessionRename)
    {
        MessageType = messageType;
        SessionUin = sessionUin;
        SessionName = sessionName;
        SessionRename = sessionRename;
        Host = host;

        SubscribeChatMsg();

        EditingMessageChain = messageType switch
        {
            MessageChain.MessageType.Group => MessageBuilder.Group(sessionUin).Build(),
            MessageChain.MessageType.Temp or MessageChain.MessageType.Friend =>
                MessageBuilder.Friend(sessionUin).Build(),
            _ => throw new ArgumentOutOfRangeException(nameof(messageType), messageType, null)
        };
    }

    /// <summary>
    /// 订阅消息
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    private void SubscribeChatMsg()
    {
        SessionDisposable = MessageType switch
        {
            MessageChain.MessageType.Group => Host.OnGroupMessageReceived
                .Where(x => x.groupMessageEvent.Chain.GroupUin == SessionUin)
                .Subscribe(x =>
                ProcessMsgChain(x.groupMessageEvent.Chain)),
            
            MessageChain.MessageType.Temp => Host.OnTempMessageReceived
                .Where(x => x.tempMessageEvent.Chain.FriendUin == SessionUin)
                .Subscribe(x =>
                ProcessMsgChain(x.tempMessageEvent.Chain)),
            
            MessageChain.MessageType.Friend => Host.OnFriendMessageReceived
                .Where(x => x.friendMessageEvent.Chain.FriendUin == SessionUin)
                .Subscribe(x =>
                ProcessMsgChain(x.friendMessageEvent.Chain)),
            _ => throw new ArgumentOutOfRangeException(nameof(MessageType), "错误的消息类型, 无法订阅消息")
        };
    }

    /// <summary>
    /// 处理消息链
    /// </summary>
    /// <param name="messageChain"></param>
    private void ProcessMsgChain(MessageChain messageChain)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            MessageRecord.Add(new SessionMsgRecord(messageChain, SessionUUID));
        });
    }

    /// <summary>
    /// 释放消息订阅
    /// </summary>
    private IDisposable? SessionDisposable { get; set; }
    
    private Meow.Core.Meow Host { get; set; }
    
    /// <summary>
    /// 会话唯一ID
    /// </summary>
    public string SessionUUID { get; } = Guid.NewGuid().ToString();

    /// <summary>
    /// 最大消息容量
    /// </summary>
    private const int _msgRecordCapacity = 1000;
    
    /// <summary>
    /// 会话中的消息类型
    /// </summary>
    public MessageChain.MessageType MessageType { get; set; }
    
    /// <summary>
    /// 会话Uin 对应MessageType
    /// </summary>
    public uint SessionUin { get; set; }
    
    /// <summary>
    /// 会话名称
    /// </summary>
    public string SessionName { get; set; }
    
    /// <summary>
    /// 会话重命名
    /// </summary>
    public string? SessionRename { get; set; }

    /// <summary>
    /// 如果有重命名就显示重命名
    /// </summary>
    public string DisplayName => SessionRename ?? SessionName;

    /// <summary>
    /// 会话中的聊天记录
    /// </summary>
    public FixedCapacityObservableCollection<SessionMsgRecord> MessageRecord { get; } = new(_msgRecordCapacity);
    
    /// <summary>
    /// 编辑中消息
    /// </summary>
    public MessageChain EditingMessageChain { get; init; }

    ~SessionInfo()
    {
        SessionDisposable?.Dispose();
    }
}