using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Lagrange.Core.Event;
using Lagrange.Core.Message;

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
    protected Meow? Host { get; set; }

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
        Host = host;
        SubscribeCommand();
    }

    /// <summary>
    /// 订阅命令
    /// </summary>
    protected virtual void SubscribeCommand()
    {
        if (Host is null)
        {
            return;
        }

        CommandParserDisposable = Host.OnCommandReceived.ObserveOn(ThreadPoolScheduler.Instance)
            .Subscribe(CommandParser);
    }

    /// <summary>
    /// 命令解析器
    /// </summary>
    /// <param name="commandArgs"></param>
    protected virtual async void CommandParser(
        (Meow meow, MessageChain messageChain, EventBase @event, string command, string? args) commandArgs)
    {
        var (meow, messageChain, _, command, args) = commandArgs;
        var targetCommand = Commands.FirstOrDefault(x => x.CommandTrigger == command);
        if (targetCommand is null || !targetCommand.CommandTrigger.Equals(targetCommand!.CommandTrigger))
        {
            return;
        }

        if (!meow.GetUserPermission(messageChain.FriendUin, targetCommand))
        {
            return;
        }

        var commandResult = await targetCommand.RunCommand(meow, messageChain, args).ConfigureAwait(false);
        if (commandResult.needSendMessage)
        {
            await meow.SendMessage(commandResult.messageChain).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public virtual void Remove()
    {
        Host = null;
    }

    ~PluginBase()
    {
        CommandParserDisposable?.Dispose();
    }
}