using System.Diagnostics.CodeAnalysis;
using System.Reactive.Subjects;
using Lagrange.Core.Common.Interface.Api;
using Lagrange.Core.Event;
using Lagrange.Core.Event.EventArg;
using Lagrange.Core.Message;
using Meow.Bootstrapper;

namespace Meow.Core;

// 实现<see cref="IMeowEvent"/>的partial类
public partial class Meow
{
    [SuppressMessage("ReSharper", "HeapView.ObjectAllocation.Evident")]
    public void ImplementEventFromBotContext()
    {
        MeowBot.Invoker.OnBotOnlineEvent += (_, @event) =>
        {
            Info("Bot Online");
            OnBotOnlineEvent.OnNext((this, @event)); 
            BotInfoManager.SaveKeystore(WorkFolder, MeowBot.UpdateKeystore());
        };

        MeowBot.Invoker.OnBotOfflineEvent += (_, @event) => { OnBotOfflineEvent.OnNext((this, @event)); };

        MeowBot.Invoker.OnBotLogEvent += (_, @event) => { OnBotLogEvent.OnNext((this, @event)); };

        MeowBot.Invoker.OnBotCaptchaEvent += (_, @event) => { OnBotCaptchaEvent.OnNext((this, @event)); };

        MeowBot.Invoker.OnGroupInvitationReceived += (_, @event) =>
        {
            OnGroupInvitationReceived.OnNext((this, @event));
        };

        MeowBot.Invoker.OnFriendMessageReceived += (_, @event) =>
        {
            if (TryParseCommand(@event.Chain, out var commandTrigger, out var args))
            {
                OnCommandReceived.OnNext((this, @event.Chain, @event, commandTrigger, args));
                return;
            }

            OnFriendMessageReceived.OnNext((this, @event));
            OnMessageReceived.OnNext((this, @event.Chain, MessageChain.MessageType.Friend));
        };
        MeowBot.Invoker.OnGroupMessageReceived += (_, @event) =>
        {
            if (TryParseCommand(@event.Chain, out var commandTrigger, out var args))
            {
                OnCommandReceived.OnNext((this, @event.Chain, @event, commandTrigger, args));
                return;
            }

            OnGroupMessageReceived.OnNext((this, @event));
            OnMessageReceived.OnNext((this, @event.Chain, MessageChain.MessageType.Group));
        };

        MeowBot.Invoker.OnTempMessageReceived += (_, @event) =>
        {
            if (TryParseCommand(@event.Chain, out var commandTrigger, out var args))
            {
                OnCommandReceived.OnNext((this, @event.Chain, @event, commandTrigger, args));
                return;
            }

            OnTempMessageReceived.OnNext((this, @event));
            OnMessageReceived.OnNext((this, @event.Chain, MessageChain.MessageType.Temp));
        };

        MeowBot.Invoker.OnGroupAdminChangedEvent += (_, @event) => { OnGroupAdminChangedEvent.OnNext((this, @event)); };
        MeowBot.Invoker.OnGroupMemberIncreaseEvent += (_, @event) =>
        {
            OnGroupMemberIncreaseEvent.OnNext((this, @event));
        };

        MeowBot.Invoker.OnGroupMemberDecreaseEvent += (_, @event) =>
        {
            OnGroupMemberDecreaseEvent.OnNext((this, @event));
        };

        MeowBot.Invoker.OnFriendRequestEvent += (_, @event) => { OnFriendRequestEvent.OnNext((this, @event)); };
        Info("完成Bot事件到Rx的转发");
    }

    /// <inheritdoc />
    public Subject<(Meow meow, BotOnlineEvent botOnlineEvent)> OnBotOnlineEvent { get; set; }

    /// <inheritdoc />
    public Subject<(Meow meow, BotOfflineEvent botOfflineEvent)> OnBotOfflineEvent { get; set; }

    /// <inheritdoc />
    public Subject<(Meow meow, BotLogEvent botLogEvent)> OnBotLogEvent { get; set; }

    /// <inheritdoc />
    public Subject<(Meow meow, BotCaptchaEvent botCaptchaEvent)> OnBotCaptchaEvent { get; set; }

    /// <inheritdoc />
    public Subject<(Meow meow, GroupInvitationEvent groupInvitationEvent)> OnGroupInvitationReceived { get; set; }

    /// <inheritdoc />
    public Subject<(Meow meow, FriendMessageEvent friendMessageEvent)> OnFriendMessageReceived { get; set; }

    /// <inheritdoc />
    public Subject<(Meow meow, GroupMessageEvent groupMessageEvent)> OnGroupMessageReceived { get; set; }

    /// <inheritdoc />
    public Subject<(Meow meow, TempMessageEvent tempMessageEvent)> OnTempMessageReceived { get; set; }

    /// <inheritdoc />
    public Subject<(Meow meow, MessageChain messageChain, MessageChain.MessageType messageType)> OnMessageReceived
    {
        get;
        set;
    }

    /// <inheritdoc />
    public Subject<(Meow meow, MessageChain messageChain, EventBase @event, string command, string? args)>
        OnCommandReceived { get; set; }

    /// <inheritdoc />
    public Subject<(Meow meow, GroupAdminChangedEvent groupAdminChangedEvent)> OnGroupAdminChangedEvent { get; set; }

    /// <inheritdoc />
    public Subject<(Meow meow, GroupMemberIncreaseEvent groupMemberIncreaseEvent)> OnGroupMemberIncreaseEvent
    {
        get;
        set;
    }

    /// <inheritdoc />
    public Subject<(Meow meow, GroupMemberDecreaseEvent groupMemberDecreaseEvent)> OnGroupMemberDecreaseEvent
    {
        get;
        set;
    }

    /// <inheritdoc />
    public Subject<(Meow meow, FriendRequestEvent friendRequestEvent)> OnFriendRequestEvent { get; set; }
}