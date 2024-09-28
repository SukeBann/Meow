using System.Diagnostics.CodeAnalysis;
using Lagrange.Core.Message;
using Lagrange.Core.Message.Entity;
using Masuit.Tools;

namespace Meow.Core;

// 插件加载
public partial class Meow
{
    /// <summary>
    /// 命令分隔符
    /// </summary>
    private char CommandPrompt { get; set; }

    /// <summary>
    /// 命令分隔符
    /// </summary>
    private char CommandArgsSeparator { get; set; }

    /// <summary>
    /// 插件列表
    /// </summary>
    public List<IMeowPlugin> Plugins { get; } = new();

    /// <summary>
    /// 插件UID字典
    /// </summary>
    private Dictionary<string, IMeowPlugin> PluginDict { get; } = new();

    /// <summary>
    /// 命令字典
    /// </summary>
    private Dictionary<string, IMeowCommand> CommandDict { get; } = new();

    /// <summary>
    /// 用于标记插件列表是否变动
    /// </summary>
    public int PluginChangeSeed { get; private set; }

    /// <summary>
    /// 像插件列表中添加插件并激活插件 
    /// </summary>
    public void LoadPlugin(IMeowPlugin plugin)
    {
        if (plugin.PluginUid.IsNullOrEmpty())
        {
            throw new Exception($"{plugin.PluginName}: 插件UID不能为空");
        }
        if (PluginDict.ContainsKey(plugin.PluginUid))
        {
            throw new Exception($"{plugin.PluginName}:不能重复加载相同UID插件: {plugin.PluginUid}");
        }

        TryLoadCommand(plugin);
        plugin.InjectPlugin(this);
        UpdatePluginPermission(plugin);
        
        PluginDict.Add(plugin.PluginUid, plugin);
        Plugins.Add(plugin);
        var random = new Random().Next(0, 10000);
        PluginChangeSeed = random == PluginChangeSeed ? random + 1 : random;
        Info($"成功加载插件: {plugin.PluginName}:\n\r{plugin.PluginDescription}");
    }

    /// <summary>
    /// 加载命令
    /// </summary>
    /// <param name="plugin"></param>
    private void TryLoadCommand(IMeowPlugin plugin)
    {
        if (!plugin.HaveAnyCommands)
        {
            return;
        }
        
        if (plugin.Commands.Any(pluginCommand => CommandDict.ContainsKey(pluginCommand.CommandUid)))
        {
            var message = $"插件: {plugin.PluginName}命令加载失败, 无法重复加载相同Uid的命令";
            Info(message);
            throw new Exception(message);
        }

        CommandDict.AddRange(plugin.Commands.ToDictionary(c => c.CommandUid, c => c));
    }

    /// <summary>
    /// 取消加载命令
    /// </summary>
    /// <param name="plugin"></param>
    private void UnLoadCommand(IMeowPlugin plugin)
    {
        foreach (var pluginCommand in plugin.Commands)
        {
            CommandDict.Remove(pluginCommand.CommandUid);
        }
    }

    /// <summary>
    /// 取消加载插件并从插件列表中移除
    /// </summary>
    public void UnLoadPlugin(IMeowPlugin plugin)
    {
        plugin.Remove();
        UnLoadCommand(plugin);
        PluginDict.Remove(plugin.PluginUid);
        Plugins.Remove(plugin);
        var random = new Random().Next(0, 10000);
        PluginChangeSeed = random == PluginChangeSeed ? random + 1 : random;
    }

    /// <summary>
    /// 解析消息连中的第一条文本消息 如果为命令提示符开头 则尝试解析命令触发文本和参数
    /// </summary>
    /// <param name="messageChain">被解析的消息链</param>
    /// <param name="commandTrigger">命令触发文, 返回值为null的时候可能为null</param>
    /// <param name="args">命令参数, 无论是否成功都可能为null</param>
    /// <returns></returns>
    private bool TryParseCommand(MessageChain messageChain,
        [MaybeNullWhen(false)] out string commandTrigger,
        out string? args)
    {
        commandTrigger = null;
        args = null;
        var firstMessage = messageChain.FirstOrDefault(x => x is TextEntity);
        if (firstMessage is not TextEntity textEntity || textEntity.Text.Length < 2 ||
            !textEntity.Text[0].Equals(CommandPrompt))
        {
            return false;
        }

        var command = textEntity.Text[1..];
        var strings = command.Split(CommandArgsSeparator, 2);
        switch (strings.Length)
        {
            case 1:
                commandTrigger = strings[0];
                return true;
            case 2:
                commandTrigger = strings[0];
                args = strings[1];
                return true;
            default:
                return false;
        }
    }
}