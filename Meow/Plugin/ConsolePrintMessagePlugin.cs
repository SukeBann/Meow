using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using Camille.Core.MiraiBase.Contract;
using Camille.Core.MiraiBase.Models.Base;
using Camille.Imp.MiraiBase.Message;
using Camille.Imp.MiraiBase.Message.MessageContainer;
using Meow.Core;
using Meow.Core.Model.Base;

namespace Meow.Plugin;

/// <summary>
/// 将Bot接收到的消息输出到对应Bot的控制台输出中
/// </summary>
public partial class ConsolePrintMessagePlugin : PluginBase
{
    /// <inheritdoc />
    public override string PluginName => "控制台消息打印插件";

    /// <inheritdoc />
    public override string PluginUid => "9178A380-4A38-4F93-859C-23E5C6BD2C82";

    /// <inheritdoc />
    public override string PluginDescription => "提供将接收到的消息链打印到控制台的功能";

    /// <summary>
    /// 释放消息接收
    /// </summary>
    private IDisposable? MessageReceivedDisposable { get; set; }

    /// <inheritdoc />
    public override bool IsNeedAdmin => false;

    /// <inheritdoc />
    public override List<IMeowCommand> Commands { get; } = new();

    /// <summary>
    /// 匹配控制字符
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex(@"[\x00-\x08\x0B\x0C\x0E-\x1F\x7F]+")]
    private static partial Regex MatchEmojiUnicode();

    /// <inheritdoc />
    public override void InjectPlugin(Core.Meow host)
    {
        base.InjectPlugin(host);
        MessageReceivedDisposable = Bot?.OnMiraiMessageReceived
            .ObserveOn(ThreadPoolScheduler.Instance)
            .Subscribe(Handle);
    }

    private void Handle(IMiraiMessageContainer containerBase)
    {
        var messageChain = containerBase.MessageChain;
        string? output;
        switch (containerBase)
        {
            case FriendMiraiMsgContainer friendMiraiMsgContainer:
                output = $"[F]-[{(containerBase as FriendMiraiMsgContainer)?.Sender?.Nickname}] {messageChain.GetPlainMessage()}";
                break;
            case GroupMiraiMsgContainer groupMiraiMsgContainer:
            case TempMiraiMsgContainer tempMiraiMsgContainer:
                var msgSymbol = containerBase is TempMiraiMsgContainer ? "T" : "G";
                var sender = containerBase is GroupMiraiMsgContainer group ? $"[{group.Sender?.Group.Name}-{group.Sender?.MemberName}]" : (containerBase as TempMiraiMsgContainer)?.Sender?.Nickname;
                output =
                $"[{msgSymbol}]-[{sender}] {messageChain.GetPlainMessage()}";
                break;
            default:
                output = $"[U] {messageChain.GetPlainMessage()}";
                break;
        }

        // 如果不替换掉的话 在WPF的RichTextBox输出中会报错
        output = MatchEmojiUnicode().Replace(output, string.Empty);
        try
        {
            Bot?.Info(output);
        }
        catch (Exception e)
        {
            Bot?.Error("插入日志失败", e);
        }
    }

    /// <inheritdoc />
    public override void Remove()
    {
        Bot = null;
        MessageReceivedDisposable?.Dispose();
    }

    ~ConsolePrintMessagePlugin()
    {
        MessageReceivedDisposable?.Dispose();
    }
}