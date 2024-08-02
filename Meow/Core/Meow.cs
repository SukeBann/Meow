using System.Reactive.Subjects;
using Lagrange.Core.Common.Entity;
using Lagrange.Core.Common.Interface.Api;
using Lagrange.Core.Event;
using Lagrange.Core.Event.EventArg;
using Lagrange.Core.Message;

namespace Meow.Core;

/// <summary>
/// 如你所见这是Meow的主要实现, 我期望把所有Api都在这里面暴露出来
/// 不同的部分会放在partial类里面实现, 文件命名是Meow_xxx这样的
/// </summary>
public partial class Meow : MeowBase, IMeowEvent
{
    public Meow(string meowName,
        string workFolder,
        char commandPrompt,
        char commandArgsSeparator) : base(meowName,
        workFolder)
    {
        CommandPrompt = commandPrompt;
        CommandArgsSeparator = commandArgsSeparator;

        // 事件
        OnBotOnlineEvent = new Subject<(Meow, BotOnlineEvent)>();
        OnBotOfflineEvent = new Subject<(Meow, BotOfflineEvent)>();
        OnBotLogEvent = new Subject<(Meow, BotLogEvent)>();
        OnBotCaptchaEvent = new Subject<(Meow, BotCaptchaEvent)>();
        OnGroupInvitationReceived = new Subject<(Meow, GroupInvitationEvent)>();
        OnFriendMessageReceived = new Subject<(Meow, FriendMessageEvent)>();
        OnGroupMessageReceived = new Subject<(Meow, GroupMessageEvent)>();
        OnTempMessageReceived = new Subject<(Meow, TempMessageEvent)>();
        OnMessageReceived = new Subject<(Meow, MessageChain, MessageChain.MessageType)>();
        OnCommandReceived = new Subject<(Meow meow, MessageChain messageChain, EventBase @eventBase, string command, string? args)>();
        OnGroupAdminChangedEvent = new Subject<(Meow, GroupAdminChangedEvent)>();
        OnGroupMemberIncreaseEvent = new Subject<(Meow, GroupMemberIncreaseEvent)>();
        OnGroupMemberDecreaseEvent = new Subject<(Meow, GroupMemberDecreaseEvent)>();
        OnFriendRequestEvent = new Subject<(Meow, FriendRequestEvent)>();
        
        OnBotOnlineEvent.Subscribe(OnMeowOnline);
        
        LoadPluginPermissionFromDb();
        LoadUserInfoFromDb();
    }

    #region Properties

    public List<BotGroup> BotGroups { get; private set; } = [];

    #endregion

    private async void OnMeowOnline((Meow meow, BotOnlineEvent botOnlineEvent) obj)
    {
        BotGroups = await MeowBot.FetchGroups();
    }
}