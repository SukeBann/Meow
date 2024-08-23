using Meow.Core;
using Meow.Core.Model.Base;

namespace Meow.Plugin.HelpPlugin;

/// <summary>
/// 为机器人提供Help帮助命令
/// </summary>
public class HelpPlugin : PluginBase
{
    /// <inheritdoc />
    public override string PluginName => "帮助插件";

    /// <inheritdoc />
    public override string PluginUid => "99BB819E-6793-4C56-9B12-9288D93DD19A";

    /// <inheritdoc />
    public override string PluginDescription => "提供help命令, 用于获取机器人的所有可用命令, 或获取具体命令的使用方法";

    /// <inheritdoc />
    public override List<IMeowCommand> Commands { get; } = [new HelpCommand()];
}