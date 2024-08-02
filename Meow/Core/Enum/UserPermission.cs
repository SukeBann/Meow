namespace Meow.Core.Enum;

/// <summary>
/// 使用权限等级
/// <br/>权限依次往下降低
/// <br/>管理员可使用全部功能
/// <br/>用户权限可使用大部分非管理员功能, 这是默认的权限
/// <br/>黑名单用户会被屏蔽全部功能
/// </summary>
public enum UserPermission
{
    /// <summary>
    /// 管理员
    /// </summary>
    Admin,
    
    /// <summary>
    /// 用户
    /// </summary>
    User, 
    
    /// <summary>
    /// 黑名单用户
    /// </summary>
    Blacklist, 
}