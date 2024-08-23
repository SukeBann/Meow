using Meow.Core.Model.Base;

namespace Meow.Core.Model;

/// <summary>
/// 插件权限
/// </summary>
public class PluginPermission(bool isNeedAdminPermission, string uid, string pluginName): DatabaseRecordBase
{
    /// <summary>
    /// 是否需要管理员权限
    /// </summary>
    public bool IsNeedAdminPermission { get; set; } = isNeedAdminPermission;

    /// <summary>
    /// 插件UID
    /// </summary>
    public string Uid { get; set; } = uid;

    /// <summary>
    /// 插件名称
    /// </summary>
    public string PluginName { get; set; } = pluginName;

    /// <summary>
    /// 命令权限, key是command uid
    /// </summary>
    public Dictionary<string, CommandPermission> CommandPermissions { get; private set; } = new();

    /// <summary>
    /// 管理员权限白名单, 如果该插件需要管理员权限
    /// <br/>那么只有白名单用户和管理员才可以使用
    /// </summary>
    public List<uint> WhiteList { get; private set; } = new();

    /// <summary>
    /// 黑名单中的用户不能使用该插件下的任何功能
    /// </summary>
    public List<uint> BlackList { get; private set; } = new();
}