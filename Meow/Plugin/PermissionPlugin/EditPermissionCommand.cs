using Lagrange.Core.Message;
using Meow.Core;

namespace Meow.Plugin.PermissionPlugin;

public class EditPermissionCommand : IMeowCommand
{
    /// <inheritdoc />
    public string CommandUid => "CF82259F-B2DD-419B-8FEE-CBD50419C1BE";

    /// <inheritdoc />
    public string CommandTrigger => "permission";
    
    /// <inheritdoc />
    public string CommandPrint => $"[{CommandTrigger}] 权限管理";

    /// <inheritdoc />
    public string CommandHelpDescription => """
                                        permission 命令:

                                        管理员用户修改其他用户的权限>
                                        示例：permission change 被修改权限的qq admin 
                                        >> 将被修改权限的用户权限修改为管理员

                                        修改用户的部分插件或命令黑白名单>
                                        示例：permission edit whiteList|blackList plugin|command 被修改权限的qq 插件或命令的Uid
                                        >> 用户将被修改指定插件或命令的黑白名单
                                        """;

    /// <inheritdoc />
    public bool IsNeedAdmin => true;

    /// <inheritdoc />
    public (bool needSendMessage, MessageChain messageChain) RunCommand(Core.Meow meow, MessageChain messageChain,
        string? args)
    {
        throw new NotImplementedException();
    }
}