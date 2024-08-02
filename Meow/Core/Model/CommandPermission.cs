using LiteDB;

namespace Meow.Core.Model;

public class CommandPermission(bool isNeedAdminPermission, string commandName, string commandUid)
{
    [BsonId]
    public int DbId { get; set; }
    
    /// <summary>
    /// 是否需要管理员权限
    /// </summary>
    public bool IsNeedAdminPermission { get; set; } = isNeedAdminPermission;

    /// <summary>
    /// 命令名称
    /// </summary>
    public string CommandName { get; set; } = commandName;

    /// <summary>
    /// 命令UID
    /// </summary>
    public string CommandUid { get; set; } = commandUid;

    /// <summary>
    /// 管理员权限白名单, 如果该命令需要管理员权限
    /// <br/>那么只有白名单用户和管理员才可以使用
    /// </summary>
    public List<uint> WhiteList { get; set; } = new();

    /// <summary>
    /// 黑名单中的用户不能使用该命令
    /// </summary>
    public List<uint> BlackList { get; set; } = new();
}