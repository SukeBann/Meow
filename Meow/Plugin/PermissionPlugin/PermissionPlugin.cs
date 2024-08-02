using Meow.Core;

namespace Meow.Plugin.PermissionPlugin;

/// <summary>
/// 权限管理插件
/// </summary>
public class PermissionPlugin : PluginBase
{
    /// <inheritdoc />
    public override string PluginName => "权限管理插件";
    
    /// <inheritdoc />
    public override string PluginDescription => "为管理员用户添加编辑其他用户权限的命令";

    /// <inheritdoc />
    public override string PluginUid => "ABE36EAD-2CA0-4477-8392-0FA7E9F11B0F";

    /// <inheritdoc />
    public override List<IMeowCommand> Commands { get; } = [new EditPermissionCommand()];

    /// <inheritdoc />
    public override bool IsNeedAdmin => true;
}