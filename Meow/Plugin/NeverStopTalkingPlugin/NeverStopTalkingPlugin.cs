using Lagrange.Core.Event;
using Lagrange.Core.Message;
using Meow.Core;

namespace Meow.Plugin.NeverStopTalkingPlugin;

/// <summary>
/// 非常想说话的Bot, 一秒钟不说都难受, 她只是想说话 她什么都没做错
/// </summary>
public class NeverStopTalkingPlugin: PluginBase
{
    /// <inheritdoc />
    public override string PluginName => "Never stop talking";

    /// <inheritdoc />
    public override string PluginDescription => "非常想说话的Bot, 一秒钟不说都难受, 她只是想说话 她什么都没做错";

    /// <inheritdoc />
    public override string PluginUid => "8E1F9C63-CB6C-4232-B617-09DAA73F8910";

    /// <inheritdoc />
    public override List<IMeowCommand> Commands { get; } = [new DontSayThatCommand()];

    /// <inheritdoc />
    public override void InjectPlugin(Core.Meow host)
    {
        base.InjectPlugin(host);
    }

    /// <inheritdoc />
    public override void Remove()
    {
        throw new NotImplementedException();
    }
}