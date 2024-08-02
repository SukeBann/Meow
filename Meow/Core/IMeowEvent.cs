using System.Reactive.Subjects;
using Lagrange.Core.Event;
using Lagrange.Core.Event.EventArg;
using Lagrange.Core.Message;

namespace Meow.Core;

/// <summary>
/// 定义了Meow的Rx事件
/// </summary>
public interface IMeowEvent
{
    /// <summary>
    /// 根据bot事件生成对应的Subject
    /// </summary>
    public void ImplementEventFromBotContext();

    /// <summary>
    /// Bot在线时触发
    /// </summary>
    public Subject<(Meow meow, BotOnlineEvent botOnlineEvent)> OnBotOnlineEvent { get; set; }

    /// <summary>
    /// Bot 下线时触发,可用于监控 Bot 是否掉线
    /// </summary>
    public Subject<(Meow meow, BotOfflineEvent botOfflineEvent)> OnBotOfflineEvent { get; set; }

    /// <summary>
    /// 日志产生时触发
    /// </summary>
    public Subject<(Meow meow, BotLogEvent botLogEvent)> OnBotLogEvent { get; set; }

    /// <summary>
    /// Bot 需要验证码时触发
    /// </summary>
    public Subject<(Meow meow, BotCaptchaEvent botCaptchaEvent)> OnBotCaptchaEvent { get; set; }

    /// <summary>
    /// Bot 被邀请入群时触发
    /// </summary>
    public Subject<(Meow meow, GroupInvitationEvent groupInvitationEvent)> OnGroupInvitationReceived { get; set; }

    /// <summary>
    /// 收到私聊消息时触发
    /// </summary>
    public Subject<(Meow meow, FriendMessageEvent friendMessageEvent)> OnFriendMessageReceived { get; set; }

    /// <summary>
    /// 收到群聊消息时触发
    /// </summary>
    public Subject<(Meow meow, GroupMessageEvent groupMessageEvent)> OnGroupMessageReceived { get; set; }

    /// <summary>
    /// 收到群临时消息时触发
    /// </summary>
    public Subject<(Meow meow, TempMessageEvent tempMessageEvent)> OnTempMessageReceived { get; set; }

    /// <summary>
    /// 收到消息时触发, 群聊私聊临时消息都会被聚合到这里
    /// </summary>
    public Subject<(Meow meow, MessageChain messageChain, MessageChain.MessageType messageType)> OnMessageReceived
    {
        get;
        set;
    }

    /// <summary>
    /// 收到触发了命令格式的消息时触发
    /// <param name="@event">消息事件类型</param>
    /// </summary>
    public Subject<(Meow meow, MessageChain messageChain, EventBase @event, string command, string? args)> OnCommandReceived
    {
        get;
        set;
    }

    /// <summary>
    /// 群管变更时触发
    /// </summary>
    public Subject<(Meow meow, GroupAdminChangedEvent groupAdminChangedEvent)> OnGroupAdminChangedEvent { get; set; }

    /// <summary>
    /// 有人入群时触发
    /// </summary>
    public Subject<(Meow meow, GroupMemberIncreaseEvent groupMemberIncreaseEvent)> OnGroupMemberIncreaseEvent
    {
        get;
        set;
    }

    /// <summary>
    /// 有人退群时触发
    /// </summary>
    public Subject<(Meow meow, GroupMemberDecreaseEvent groupMemberDecreaseEvent)> OnGroupMemberDecreaseEvent
    {
        get;
        set;
    }

    /// <summary>
    /// 有好友申请时触发
    /// </summary>
    public Subject<(Meow meow, FriendRequestEvent friendRequestEvent)> OnFriendRequestEvent { get; set; }
}