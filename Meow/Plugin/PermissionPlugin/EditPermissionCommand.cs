using Meow.Core.Enum;
using Camille.Core.MiraiBase.Models.Base;
using Camille.Imp.MiraiBase.Message;
using Meow.Core;
using Meow.Utils;

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
    public Task<(bool needSendMessage, MessageChain messageChain)> RunCommand(Core.Meow meow,
        MiraiMsgContainerBase container,
        string? args)
    {
        if (string.IsNullOrWhiteSpace(args))
        {
            return Task.FromResult((true, (MessageChain)CommandHelpDescription));
        }

        var split = args.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (split.Length < 1)
        {
            return Task.FromResult((true, (MessageChain)CommandHelpDescription));
        }

        var subCommand = split[0].ToLower();
        return subCommand switch
        {
            "change" => HandleChange(meow, split.Skip(1).ToArray()),
            "edit" => HandleEdit(meow, split.Skip(1).ToArray()),
            _ => Task.FromResult((true, (MessageChain)$"未知子命令: {subCommand}\n{CommandHelpDescription}"))
        };
    }

    private Task<(bool, MessageChain)> HandleChange(Core.Meow meow, string[] args)
    {
        if (args.Length < 2)
        {
            return Task.FromResult((true, (MessageChain)"参数不足。用法：permission change <qq> <admin|user|blacklist>"));
        }

        if (!long.TryParse(args[0], out var qq))
        {
            return Task.FromResult((true, (MessageChain)"无效的 QQ 号码。"));
        }

        if (!Enum.TryParse<UserPermission>(args[1], true, out var permission))
        {
            return Task.FromResult((true, (MessageChain)$"无效的权限等级: {args[1]}。可选值：admin, user, blacklist"));
        }

        meow.EditUserPermission(qq, permission);
        return Task.FromResult((true, (MessageChain)$"成功将用户 {qq} 的权限修改为 {permission}"));
    }

    private Task<(bool, MessageChain)> HandleEdit(Core.Meow meow, string[] args)
    {
        if (args.Length < 4)
        {
            return Task.FromResult((true,
                (MessageChain)"参数不足。用法：permission edit <whiteList|blackList> <plugin|command> <qq> <uid>"));
        }

        var listType = args[0].ToLower();
        var targetType = args[1].ToLower();
        if (!long.TryParse(args[2], out var qq))
        {
            return Task.FromResult((true, (MessageChain)"无效的 QQ 号码。"));
        }

        var uid = args[3];

        try
        {
            if (targetType == "plugin")
            {
                var plugin = meow.Plugins.FirstOrDefault(x => x.PluginUid == uid);
                if (plugin == null)
                {
                    return Task.FromResult((true, (MessageChain)$"未找到 UID 为 {uid} 的插件。"));
                }

                if (listType == "whitelist")
                {
                    meow.EditWhiteList(qq, true, plugin);
                }
                else if (listType == "blacklist")
                {
                    meow.EditBlackList(qq, true, plugin);
                }
                else
                {
                    return Task.FromResult((true, (MessageChain)$"无效的名单类型: {listType}。可选值：whiteList, blackList"));
                }
            }
            else if (targetType == "command")
            {
                // 需要找到该命令所属的插件
                IMeowPlugin? targetPlugin = null;
                IMeowCommand? targetCommand = null;

                foreach (var plugin in meow.Plugins)
                {
                    targetCommand = plugin.Commands.FirstOrDefault(c => c.CommandUid == uid);
                    if (targetCommand != null)
                    {
                        targetPlugin = plugin;
                        break;
                    }
                }

                if (targetPlugin == null || targetCommand == null)
                {
                    return Task.FromResult((true, (MessageChain)$"未找到 UID 为 {uid} 的命令。"));
                }

                if (listType == "whitelist")
                {
                    meow.EditWhiteList(qq, true, targetPlugin, targetCommand);
                }
                else if (listType == "blacklist")
                {
                    meow.EditBlackList(qq, true, targetPlugin, targetCommand);
                }
                else
                {
                    return Task.FromResult((true, (MessageChain)$"无效的名单类型: {listType}。可选值：whiteList, blackList"));
                }
            }
            else
            {
                return Task.FromResult((true, (MessageChain)$"无效的目标类型: {targetType}。可选值：plugin, command"));
            }

            return Task.FromResult((true, (MessageChain)$"成功修改用户 {qq} 在 {targetType}({uid}) 中的 {listType}"));
        }
        catch (Exception e)
        {
            return Task.FromResult((true, (MessageChain)$"操作失败: {e.Message}"));
        }
    }
}