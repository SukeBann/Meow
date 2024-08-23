using Meow.Core.Enum;
using Meow.Core.Model.Base;

namespace Meow.Core.Model;

/// <summary>
/// 用户信息
/// </summary>
public class UserInfo(uint uin, UserPermission userPermission): DatabaseRecordBase
{
    /// <summary>
    /// uid 俗称qq号
    /// </summary>
    public uint Uin { get; private set; } = uin;

    /// <summary>
    /// 权限等级
    /// </summary>
    public UserPermission UserPermission { get; set; } = userPermission;
}