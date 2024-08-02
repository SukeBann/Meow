using LiteDB;
using Meow.Core.Enum;

namespace Meow.Core.Model;

/// <summary>
/// 用户信息
/// </summary>
public class UserInfo(uint uin, UserPermission userPermission)
{
    [BsonId]
    public int DbId { get; set; }
    
    /// <summary>
    /// uid 俗称qq号
    /// </summary>
    public uint Uin { get; private set; } = uin;

    /// <summary>
    /// 权限等级
    /// </summary>
    public UserPermission UserPermission { get; set; } = userPermission;

    /// <summary>
    /// 是否被删除
    /// </summary>
    public bool HasDelete { get; set; }
}