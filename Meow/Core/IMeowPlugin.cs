namespace Meow.Core;

/// <summary>
/// 插件都要继承这个接口
/// </summary>
public interface IMeowPlugin
{
    /// <summary>
    /// 插件名称
    /// </summary>
    public string PluginName { get; }
    
    /// <summary>
    /// 插件UID
    /// </summary>
    public string PluginUid { get; }

    /// <summary>
    /// 是否需要管理员权限
    /// </summary>
    public bool IsNeedAdmin { get; }
    
    /// <summary>
    /// 插件说明
    /// </summary>
    public string PluginDescription { get; }
    
    /// <summary>
    /// 插件包含的所有命令列表
    /// </summary>
    public List<IMeowCommand> Commands { get; }
    
    /// <summary>
    /// 将插件实例注入meow实例 
    /// </summary>
    public void InjectPlugin(Meow host);
    
    /// <summary>
    /// 从Meow实例中移除该插件
    /// </summary>
    public void Remove();
}