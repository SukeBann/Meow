using Lagrange.Core.Message;

namespace Meow.Core;

/// <summary>
/// 机器人命令接口定义
/// </summary>
public interface IMeowCommand
{
    /// <summary>
    /// 插件UID
    /// </summary>
    public string CommandUid { get; }

    /// <summary>
    /// 命令的触发文本
    /// <br/>比如help 触发帮助命令
    /// </summary>
    public string CommandTrigger { get; }
    
    /// <summary>
    /// 打印命令简述
    /// </summary>
    public string CommandPrint { get; }

    /// <summary>
    /// 命令描述
    /// </summary>
    public string CommandHelpDescription { get; }

    /// <summary>
    /// 是否需要管理员权限
    /// </summary>
    public bool IsNeedAdmin { get; }

    /// <summary>
    /// 执行命令, 并返回用于发送的消息链
    /// </summary>
    /// <param name="meow">在哪个bot上执行</param>
    /// <param name="messageChain">触发命令的消息链</param>
    /// <param name="args">参数</param>
    public Task<(bool needSendMessage, MessageChain messageChain)> RunCommand(Meow meow, MessageChain messageChain,
        string? args);
}