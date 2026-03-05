using System.Reactive.Subjects;
using Camille.Core.MiraiBase.Contract;
using Camille.Imp.MiraiBase.Message;
using Meow.Config;

namespace Meow.Core;

/// <summary>
/// 如你所见这是Meow的主要实现, 我期望把所有Api都在这里面暴露出来
/// 不同的部分会放在partial类里面实现, 文件命名是Meow_xxx这样的
/// </summary>
public partial class Meow : MeowBase
{
    #region Properties

    /// <summary>
    /// Bot QQ号
    /// </summary>
    public long BotQq { get; set; }

    public Subject<(Meow meow, MiraiMsgContainerBase msg, string command, string args)> OnCommandReceived { get; set; }

    #endregion

    public Meow(MeowConfig config, string workFolder) : base(config, config.BotName, workFolder)
    {
        CommandPrompt = config.CommandPrompt;
        CommandArgsSeparator = config.CommandArgsSeparator;
        BotQq = config.BotQq;

        LoadPluginPermissionFromDb();
        LoadUserInfoFromDb();
        OnCommandReceived = new Subject<(Meow meow, MiraiMsgContainerBase msg, string command, string args)>();
        OnMiraiMessageReceived.Subscribe(msg =>
        {
            var msgBase = (MiraiMsgContainerBase) msg;
            if (TryParseCommand(msg.MessageChain, out var commandTrigger, out var args))
            {
                OnCommandReceived.OnNext((this, msgBase, commandTrigger, args));
            }
        });
    }
}