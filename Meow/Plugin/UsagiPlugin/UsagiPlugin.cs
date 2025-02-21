using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Lagrange.Core.Message;
using Lagrange.Core.Message.Entity;
using Meow.Core.Model.Base;

namespace Meow.Plugin.UsagiPlugin;

/// <summary>
/// 使Bot发出Usagi的声音和表情包
/// </summary>
public class UsagiPlugin: PluginBase
{
    /// <inheritdoc />
    public override string PluginName => "Usagi";

    /// <inheritdoc />
    public override string PluginDescription => "使Bot发出Usagi的声音和表情包";

    /// <inheritdoc />
    public override string PluginUid => "3D78339D-2E7B-4AB7-B046-C5C24193A84B";
    
    /// <summary>
    /// 取消消息订阅
    /// </summary>
    private IDisposable? MsgSubscribeDisposable { get; set; }

    /// <inheritdoc />
    public override void InjectPlugin(Core.Meow host)
    {
        base.InjectPlugin(host);
        host.OnMessageReceived.ObserveOn(ThreadPoolScheduler.Instance).Subscribe(Handle);
    }

    private void Handle((Core.Meow meow, MessageChain messageChain, MessageChain.MessageType messageType) @tuple)
    {
        var (meow, messageChain, messageType) = @tuple;
        if (messageChain.Type is MessageChain.MessageType.Friend && messageChain.FriendUin == 1052700448)
        {
            foreach (var imageEntity in messageChain.Where(x => x is ImageEntity).Cast<ImageEntity>())
            {
                var imageEntityImageSize = imageEntity.ImageSize;
                Console.WriteLine();
            }
        }
    }

    /// <inheritdoc />
    public override void Remove()
    {
        base.Remove();
        MsgSubscribeDisposable?.Dispose();
    }
}