using System.Text;
using Lagrange.Core.Message;
using Masuit.Tools;
using Meow.Core;
using Meow.Utils;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Meow.Plugin.HelpPlugin;

public class HelpCommand : IMeowCommand
{
    /// <inheritdoc />
    public string CommandUid => "57B12AC6-8BEC-4A6A-95FD-15B475AAAA9E";

    /// <summary>
    /// 使用help命令获取所有命令信息时会缓存到这里
    /// </summary>
    private string? AllCommandHelpCache { get; set; }

    private int AllCommandHelpCacheSeed { get; set; }

    public bool IsNeedAdmin => false;

    /// <inheritdoc />
    public string CommandTrigger => "help";

    /// <inheritdoc />
    public string CommandPrint => $"[{CommandTrigger}] 获取命令帮助";

    /// <inheritdoc />
    public string CommandHelpDescription => """
                                        help 命令:

                                        获取bot的所有可用命令>
                                        示例：help 
                                        >> 结果为bot的所有可用命令

                                        获取目标命令的用法>
                                        help 目标命令
                                        示例： help help
                                        >>结果为help命令的使用方法
                                        """;

    /// <inheritdoc />
    public (bool needSendMessage, MessageChain messageChain) RunCommand(Core.Meow meow, MessageChain messageChain,
        string? args)
    {
        if (args.IsNullOrEmpty())
        {
            return (true, GetHelpAll(meow, messageChain));
        }

        var isSuccess = new CommandArgsCheckUtil(messageChain, args)
            .IsSuccess(out var message, out var chain, out var argStr, out var argList);
        if (!isSuccess)
        {
            return (true, messageChain.CreateSameTypeTextMessage(message));
        }

        var target = meow.Plugins.SelectMany(x => x.Commands)
            .FirstOrDefault(x => x.CommandTrigger == argStr);
        return target is null
            ? (true, messageChain.CreateSameTypeTextMessage($"未查询到命令{argStr}"))
            : (true, messageChain.CreateSameTypeTextMessage(target.CommandHelpDescription));
    }

    /// <summary>
    /// 获取无参数的help命令结果
    /// </summary>
    private MessageChain GetHelpAll(Core.Meow meow, MessageChain messageChain)
    {
        // ReSharper disable once InvertIf
        if (AllCommandHelpCache.IsNullOrEmpty() || meow.PluginChangeSeed != AllCommandHelpCacheSeed)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("获取到了当前所有可用命令, 使用help加对应命令名可以获取详细用法");
            
            var enumerable = meow.Plugins.SelectMany(x => x.Commands)
                .Select(x => x.CommandPrint)
                .Where(x => !x.IsNullOrEmpty())
                .ToList();
            foreach (var se in enumerable)
            {
                stringBuilder.AppendLine(se);
            }

            AllCommandHelpCache = stringBuilder.ToString();
            AllCommandHelpCacheSeed = meow.PluginChangeSeed;
        }

        return messageChain.CreateSameTypeTextMessage(AllCommandHelpCache!);
    }
}