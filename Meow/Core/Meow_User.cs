using Masuit.Tools;
using Meow.Core.Enum;
using Meow.Core.Model;

namespace Meow.Core;

// 用户相关功能, 权限表
public partial class Meow
{
    /// <summary>
    /// 用户信息数据库集合
    /// </summary>
    private const string MeowUserInfoCollection = nameof(MeowUserInfoCollection);
    /// <summary>
    /// 插件权限数据库集合
    /// </summary>
    private const string MeowPluginPermissionCollection = nameof(MeowPluginPermissionCollection);

    /// <summary>
    /// key为uid 用户信息
    /// </summary>
    private Dictionary<uint, UserInfo> UserInfoDict { get; set; } = new();

    /// <summary>
    /// key为uid 插件权限
    /// </summary>
    private Dictionary<string, PluginPermission> PluginPermissionDict { get; set; } = new();

    /// <summary>
    /// key为uid 命令权限
    /// </summary>
    private Dictionary<string, CommandPermission> CommandPermissionDict { get; set; } = new();

    /// <summary>
    /// 更新插件权限, 如果插件没有权限信息 则添加到数据库中
    /// </summary>
    /// <param name="plugin"></param>
    private void UpdatePluginPermission(IMeowPlugin plugin)
    {
        var uid = plugin.PluginUid;
        if (PluginPermissionDict.ContainsKey(uid))
        {
            return;
        }

        var pluginPermission = new PluginPermission(plugin.IsNeedAdmin, plugin.PluginUid, plugin.PluginName);
        PluginPermissionDict.Add(plugin.PluginUid, pluginPermission);

        foreach (var command in plugin.Commands)
        {
            if (CommandPermissionDict.ContainsKey(command.CommandUid))
            {
                continue;
            }

            var commandPermission = new CommandPermission(command.IsNeedAdmin, command.CommandTrigger,
                command.CommandUid);
            pluginPermission.CommandPermissions.Add(commandPermission.CommandUid, commandPermission);
            CommandPermissionDict.Add(command.CommandUid, commandPermission);
        }

        Info("添加一条新的插件权限信息到数据库");
        Database.Insert(pluginPermission, MeowPluginPermissionCollection);
    }

    /// <summary>
    /// 从数据库读取插件权限
    /// </summary>
    private void LoadPluginPermissionFromDb()
    {
        foreach (var pluginPermission in Database.Query<PluginPermission>(MeowPluginPermissionCollection).ToList())
        {
            PluginPermissionDict.Add(pluginPermission.Uid, pluginPermission);
            CommandPermissionDict.AddRange(pluginPermission.CommandPermissions);
        }
    }

    /// <summary>
    /// 从数据库加载用户数据
    /// </summary>
    private void LoadUserInfoFromDb()
    {
        var userInfos = Database.Query<UserInfo>(MeowUserInfoCollection)
            .Where(x => !x.HasDelete).ToList();
        if (userInfos.Count < 1)
        {
            return;
        }

        UserInfoDict.AddRange(userInfos
            .ToDictionary(x => x.Uin, y => y));
    }

    /// <summary>
    /// 从数据库获取指定id用户, 如果没有则创建一个
    /// </summary>
    /// <param name="uin"></param>
    /// <returns></returns>
    private UserInfo GetUser(uint uin)
    {
        if (UserInfoDict.TryGetValue(uin, out var user))
        {
            return user;
        }
        var userInfo = new UserInfo(uin, UserPermission.User);
        if (Database.Query<UserInfo>(MeowUserInfoCollection)
                .Where(x => !x.HasDelete)
                .Where(x => x.Uin == uin)
                .FirstOrDefault() is null)
        {
            Database.Insert(userInfo, MeowUserInfoCollection);
        }
        UserInfoDict.Add(user.Uin, user);
        
        Info($"创建新用户:{uin}, 权限等级：{UserPermission.User}");
        return userInfo;
    }

    /// <summary>
    /// 获取指定uin用户是否拥有指定插件的权限
    /// </summary>
    /// <param name="uin"></param>
    public bool GetUserPermission(uint uin, IMeowPlugin plugin)
    {
        var userInfo = GetUser(uin);
        if (userInfo.UserPermission is UserPermission.Blacklist)
        {
            Info($"用户{userInfo.Uin}被全局标记为黑名单用户, 无法获取执行权限");
            return false;
        }

        if (!PluginPermissionDict.TryGetValue(plugin.PluginUid, out var permission))
        {
            throw new Exception($"插件权限表中无法找到目标插件的信息, {plugin.PluginName}-{plugin.PluginUid}");
        }

        if (permission.BlackList.Contains(uin))
        {
            Info($"用户{userInfo.Uin}被插件{permission.PluginName}黑名单阻止");
            return false;
        }

        if (!plugin.IsNeedAdmin)
        {
            return true;
        }

        if (userInfo.UserPermission is UserPermission.Admin)
        {
            return true;
        }

        if (permission.WhiteList.Contains(uin))
        {
            Info($"用户{userInfo.Uin}被被插件{permission.PluginName}白名单赋予执行权限");
            return true;
        }

        Info($"用户{userInfo.Uin}被被插件{permission.PluginName}阻止执行, 需要管理员权限执行");
        return false;
    }


    /// <summary>
    /// 获取指定uin用户是否拥有指定命令的权限
    /// </summary>
    /// <param name="uin"></param>
    public bool GetUserPermission(uint uin, IMeowCommand command)
    {
        var userInfo = GetUser(uin);
        if (userInfo.UserPermission is UserPermission.Blacklist)
        {
            Info($"用户{userInfo.Uin}被全局标记为黑名单用户, 无法获取执行权限");
            return false;
        }

        if (!CommandPermissionDict.TryGetValue(command.CommandUid, out var permission))
        {
            throw new Exception($"命令权限表中无法找到目标命令的信息, {command.CommandTrigger}-{command.CommandUid}");
        }

        if (permission.BlackList.Contains(uin))
        {
            Info($"用户{userInfo.Uin}被命令{command.CommandTrigger}黑名单阻止");
            return false;
        }

        if (!command.IsNeedAdmin)
        {
            return true;
        }

        if (userInfo.UserPermission is UserPermission.Admin)
        {
            return true;
        }

        if (permission.WhiteList.Contains(uin))
        {
            Info($"用户{userInfo.Uin}被被插件{permission.CommandName}白名单赋予执行权限");
            return true;
        }

        Info($"用户{userInfo.Uin}被被插件{permission.CommandName}阻止执行, 需要管理员权限执行");
        return false;
    }

    /// <summary>
    /// 编辑用户权限
    /// </summary>
    /// <param name="uin"></param>
    /// <param name="permission"></param>
    public void EditUserPermission(uint uin, UserPermission permission)
    {
        var userInfo = GetUser(uin);
        userInfo.UserPermission = permission;
        Database.Update(userInfo, MeowUserInfoCollection);
    }

    /// <summary>
    /// 编辑指定插件或命令的白名单
    /// </summary>
    /// <param name="uin">用户id</param>
    /// <param name="isAdd">是否为添加操作, 如果为false则是从名单中移除</param>
    /// <param name="plugin">插件</param>
    /// <param name="command">命令</param>
    public void EditWhiteList(uint uin, bool isAdd, IMeowPlugin plugin, IMeowCommand? command = null)
    {
        var userInfo = GetUser(uin);
        if (userInfo.UserPermission is UserPermission.Admin)
        {
            return;
        }

        if (!PluginPermissionDict.TryGetValue(plugin.PluginUid, out var pluginPermission))
        {
            throw new Exception($"插件权限表中无法找到目标插件的信息, {plugin.PluginName}-{plugin.PluginUid}");
        }

        if (isAdd)
        {
            pluginPermission.WhiteList.Add(uin);
        }
        else
        {
            pluginPermission.WhiteList.Remove(uin);
        }


        if (command is not null)
        {
            if (!CommandPermissionDict.TryGetValue(command.CommandUid, out var commandPermission)
                || !pluginPermission.CommandPermissions.TryGetValue(command.CommandUid, out var childCommandPermission))
            {
                throw new Exception($"命令权限表中无法找到目标命令的信息, {command.CommandTrigger}-{command.CommandUid}");
            }

            if (isAdd)
            {
                commandPermission.WhiteList.Add(uin);
                childCommandPermission.WhiteList.Add(uin);
            }
            else
            {
                commandPermission.WhiteList.Remove(uin);
                childCommandPermission.WhiteList.Remove(uin);
            }
        }

        Database.Update(pluginPermission, MeowPluginPermissionCollection);
    }


    /// <summary>
    /// 编辑指定插件或命令的黑名单
    /// </summary>
    /// <param name="uin">用户id</param>
    /// <param name="isAdd">是否为添加操作, 如果为false则是从名单中移除</param>
    /// <param name="plugin">插件</param>
    /// <param name="command">命令</param>
    public void EditBlackList(uint uin, bool isAdd, IMeowPlugin plugin = null, IMeowCommand? command = null)
    {
        var userInfo = GetUser(uin);
        if (userInfo.UserPermission is UserPermission.Admin)
        {
            return;
        }

        if (!PluginPermissionDict.TryGetValue(plugin.PluginUid, out var pluginPermission))
        {
            throw new Exception($"插件权限表中无法找到目标插件的信息, {plugin.PluginName}-{plugin.PluginUid}");
        }

        if (isAdd)
        {
            pluginPermission.BlackList.Add(uin);
        }
        else
        {
            pluginPermission.BlackList.Remove(uin);
        }


        if (command is not null)
        {
            if (!CommandPermissionDict.TryGetValue(command.CommandUid, out var commandPermission)
                || !pluginPermission.CommandPermissions.TryGetValue(command.CommandUid, out var childCommandPermission))
            {
                throw new Exception($"命令权限表中无法找到目标命令的信息, {command.CommandTrigger}-{command.CommandUid}");
            }

            if (isAdd)
            {
                commandPermission.BlackList.Add(uin);
                childCommandPermission.BlackList.Add(uin);
            }
            else
            {
                commandPermission.BlackList.Remove(uin);
                childCommandPermission.BlackList.Remove(uin);
            }
        }

        Database.Update(pluginPermission, MeowPluginPermissionCollection);
    }
}