using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Camille.Imp.Extension;
using Camille.Imp.MiraiBase.Message;
using Camille.Imp.MiraiBase.Message.MessageContainer;

namespace Meow.Core.Model.Base;

public abstract class PluginBase : IMeowPlugin
{
    /// <inheritdoc />
    public virtual string PluginName { get; }

    /// <inheritdoc />
    public virtual bool IsNeedAdmin { get; }

    /// <summary>
    /// 插件宿主
    /// </summary>
    protected Meow? Bot { get; set; }

    /// <summary>
    /// 命令处理订阅
    /// </summary>
    protected IDisposable CommandParserDisposable { get; set; }

    /// <inheritdoc />
    public virtual string PluginDescription { get; }

    /// <inheritdoc />
    public virtual string PluginUid { get; }

    /// <inheritdoc />
    public virtual List<IMeowCommand> Commands { get; }

    /// <inheritdoc />
    public virtual bool HaveAnyCommands => Commands?.Count > 0;

    /// <inheritdoc />
    public virtual void InjectPlugin(Meow host)
    {
        Bot = host;
        SubscribeCommand();
    }

    /// <summary>
    /// 订阅命令
    /// </summary>
    protected virtual void SubscribeCommand()
    {
        if (Bot is null)
        {
            return;
        }

        // 没有命令就不订阅
        if (!HaveAnyCommands)
        {
            return;
        }

        CommandParserDisposable = Bot.OnCommandReceived.ObserveOn(ThreadPoolScheduler.Instance)
            .Subscribe(x => CommandParser(x.meow, x.msg, x.command, x.args));
    }

    /// <summary>
    /// 命令解析器
    /// </summary>
    protected virtual async void CommandParser(Meow meow, MiraiMsgContainerBase container, string command, string? args)
    {
        try
        {
            var targetCommand = Commands.FirstOrDefault(x => x.CommandTrigger == command);
            if (targetCommand is null)
            {
                return;
            }

            long senderId = 0;
            if (container is GroupMiraiMsgContainer groupMiraiMsg)
            {
                senderId = groupMiraiMsg?.Sender?.Id ?? 0;
            }
            else
            {
                senderId = container.Sender?.Id ?? 0;
            }
            
            if (senderId == 0)
            {
                Bot?.Error("Sender ID cannot be 0");
                return;
            }

            if (!meow.GetUserPermission(senderId, targetCommand))
            {
                return;
            }

            var commandResult = await targetCommand.RunCommand(meow, container, args).ConfigureAwait(false);
            if (commandResult.needSendMessage)
            {
                container.MessageChain = commandResult.messageChain;
                await container.SendToAsync(meow).ConfigureAwait(false);
            }
        }
        catch (Exception e)
        {
            meow.Error("Error On Command Parser", e);
        }
    }

    /// <inheritdoc />
    public virtual void Remove()
    {
        Bot = null;
    }

    ~PluginBase()
    {
        CommandParserDisposable?.Dispose();
    }
}