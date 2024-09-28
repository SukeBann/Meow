using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using Lagrange.Core.Event;
using Lagrange.Core.Message;
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
        MessageReceivedDisposable = Host.OnMessageReceived
            .ObserveOn(ThreadPoolScheduler.Instance)
            .Subscribe(Handle);
    }

    private static void Handle((Core.Meow meow, MessageChain messageChain, MessageChain.MessageType messageType) @event)
    {
        var (meow, messageChain, messageType) = @event;

        string? output;
        switch (messageType)
        {
            case MessageChain.MessageType.Group:
            case MessageChain.MessageType.Temp:
                var group = meow.BotGroups.FirstOrDefault(x => x.GroupUin == messageChain.GroupUin);
                var groupName = group?.GroupName ?? group?.GroupUin.ToString() ?? "KnownGroup";
                var groupMemberName = messageChain?.GroupMemberInfo?.MemberCard ??
                                  messageChain?.GroupMemberInfo?.MemberName ?? "KnownMember";
                groupMemberName = groupMemberName.Replace(Environment.NewLine, "").Trim();
                var msgSymbol = messageType is MessageChain.MessageType.Temp ? "T" : "G";
                output =
                    $"{Environment.NewLine}{msgSymbol}[{groupName}]-[{groupMemberName}]{Environment.NewLine}{messageChain?.ToPreviewText()}";
                break;
            case MessageChain.MessageType.Friend:
                var friendName = messageChain?.FriendInfo?.Nickname ??
                                 messageChain?.FriendInfo?.Uin.ToString() ?? "KnownFriend";
                output = $"{Environment.NewLine}F[{friendName}]{Environment.NewLine}{messageChain?.ToPreviewText()}";
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(messageType), "未知的消息类型, 无法正确解析");
        }

        // 如果不替换掉的话 在WPF的RichTextBox输出中会报错
        output = MatchEmojiUnicode().Replace(output, string.Empty);
        try
        {
            meow.Info(output);
        }
        catch (Exception e)
        {
            meow.Error("插入日志失败", e);
        }
    }

    /// <inheritdoc />
    public override void Remove()
    {
        Host = null;
        MessageReceivedDisposable?.Dispose();
    }

    ~ConsolePrintMessagePlugin()
    {
        MessageReceivedDisposable?.Dispose();
    }
}