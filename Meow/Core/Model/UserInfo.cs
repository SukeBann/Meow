using Meow.Core.Enum;
using Meow.Core.Model.Base;

namespace Meow.Core.Model;

/// <summary>
/// 用户信息
/// </summary>
public class UserInfo : DatabaseRecordBase
{
    public UserInfo() { }

    public UserInfo(uint uin, UserPermission userPermission)
    {
        Uin = uin;
        UserPermission = userPermission;
    }

    /// <summary>
    /// uid 俗称qq号
    /// </summary>
    public uint Uin { get; set; }

    /// <summary>
    /// 权限等级
    /// </summary>
    public UserPermission UserPermission { get; set; }
}