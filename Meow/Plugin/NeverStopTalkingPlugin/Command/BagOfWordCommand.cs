using Lagrange.Core.Message;
using Meow.Core;
using Meow.Core.Model.Base;
using Meow.Plugin.NeverStopTalkingPlugin.Models;
using Meow.Plugin.NeverStopTalkingPlugin.Service;
using Meow.Utils;

namespace Meow.Plugin.NeverStopTalkingPlugin.Command;

/// <summary>
/// 词袋管理
/// </summary>
public class BagOfWordCommand : HostDatabaseSupport, IMeowCommand
{
    /// <inheritdoc />
    public BagOfWordCommand(Core.Meow host, BagOfWordManager bagOfWordManager) : base(host)
    {
        BagOfWordManager = bagOfWordManager;
    }

    /// <summary>
    /// 词袋管理器
    /// </summary>
    private BagOfWordManager BagOfWordManager { get; set; }

    /// <inheritdoc />
    public string CommandUid => "8B3F95A4-F7BC-4A4B-B38B-C9153899B496";

    /// <inheritdoc />
    public string CommandTrigger => "BOW";

    /// <inheritdoc />
    public string CommandPrint => $"[{CommandTrigger}] 词袋管理";

    /// <inheritdoc />
    public string CommandHelpDescription => $"""
                                             {CommandTrigger} 命令:

                                             添加专属词袋>
                                             示例{CommandTrigger} [add|remove] [Group|Personal] 目标[群号|QQ号]  
                                             >> 为目标[群|个人] [添加|删除]一个专属词袋

                                             示例{CommandTrigger} query [Group|Personal|Global] 目标[群号|qq|Global的话这里为0]  
                                             >> 查询[目标群|个人|全局]的词袋状态
                                             >> 输出: 词袋id    状态：未创建|未填充完成|运行中 词袋大小 创建时间

                                             """;

    /// <inheritdoc />
    public bool IsNeedAdmin => true;

    /// <inheritdoc />
    public (bool needSendMessage, MessageChain messageChain) RunCommand(Core.Meow meow, MessageChain messageChain,
        string? args)
    {
        var success = new CommandArgsCheckUtil(messageChain, args)
            .SplitArgsAndCheckLength(' ', 3, "参数数量错误, 请检查参数格式")
            .ArgListMatch(0, ["add", "remove", "query"])
            .ArgListMatch(1, ["Group", "Personal", "Global"])
            .ArgListRegexMatch(2, @"^([1-9][0-9]*|0)$", "只能为正整数")
            .IsSuccess(out var errorMsg, out var resultMessage, out var arg, out var splitResult);
        if (!success)
        {
            return (true, resultMessage);
        }

        var action = splitResult[0];
        var targetTypeStr = splitResult[1];
        if (!targetTypeStr.TryConvertToEnum<BagOfWordType>(out var type))
        {
            return (true, messageChain.CreateSameTypeTextMessage($"错误的目标类型: {targetTypeStr}"));
        }

        if (!uint.TryParse(splitResult[2], out var target))
        {
            return (true, messageChain.CreateSameTypeTextMessage($"无法正常解析的uin: {splitResult[3]}"));
        }

        return action switch
        {
            "add" => (true, Add(messageChain, type, target)),
            "remove" => (true, Remove(messageChain, type, target)),
            "query" => (true, Query(messageChain, type, target)),
            _ => (true, messageChain.CreateSameTypeTextMessage("参数异常"))
        };
    }

    /// <summary>
    /// 添加词袋
    /// </summary>
    private MessageChain Add(MessageChain messageChain, BagOfWordType targetType, uint target)
    {
        switch (targetType)
        {
            case BagOfWordType.Group:
            {
                var success = BagOfWordManager.CreateBagOfWord(target, BagOfWordType.Group, out var errorMsg);
                return success
                    ? messageChain.CreateSameTypeTextMessage($"成功创建[Group:{target}]词袋")
                    : messageChain.CreateSameTypeTextMessage(errorMsg);
            }
            case BagOfWordType.Personal:
            {
                var success = BagOfWordManager.CreateBagOfWord(target,
                    BagOfWordType.Personal,
                    out var errorMsg);
                return success
                    ? messageChain.CreateSameTypeTextMessage($"成功创建[Personal:{target}]词袋")
                    : messageChain.CreateSameTypeTextMessage(errorMsg);
            }
            case BagOfWordType.Global:
            {
                var isSuccess = BagOfWordManager.CreateBagOfWord(target, BagOfWordType.Global, out var errorMsg);
                return isSuccess
                    ? messageChain.CreateSameTypeTextMessage($"成功创建[Global]词袋")
                    : messageChain.CreateSameTypeTextMessage(errorMsg);
            }
            default:
                return messageChain.CreateSameTypeTextMessage("参数异常");
        }
    }

    /// <summary>
    /// 查询词袋
    /// </summary>
    private MessageChain Query(MessageChain messageChain, BagOfWordType targetType, uint target)
    {
        var msg = "词袋查询执行异常";
        switch (targetType)
        {
            case BagOfWordType.Group:
            {
                BagOfWordManager.QueryBagOfWord(target, BagOfWordType.Group, out msg);
                break;
            }
            case BagOfWordType.Personal:
            {
                BagOfWordManager.QueryBagOfWord(target,
                    BagOfWordType.Personal,
                    out msg);
                break;
            }
            case BagOfWordType.Global:
            {
                BagOfWordManager.QueryBagOfWord(target, BagOfWordType.Global, out msg);
                break;
            }
            default:
                msg = "查询词袋异常";
                break;
        }

        return messageChain.CreateSameTypeTextMessage(msg);
    }

    /// <summary>
    /// 移除
    /// </summary>
    private MessageChain Remove(MessageChain messageChain, BagOfWordType targetType, uint target)
    {
        switch (targetType)
        {
        
            case BagOfWordType.Group:
            {
                var success = BagOfWordManager.RemoveBagOfWord(target, BagOfWordType.Group);
                return success
                    ? messageChain.CreateSameTypeTextMessage($"成功移除[Group:{target}]词袋")
                    : messageChain.CreateSameTypeTextMessage("移除失败");
            }
            case BagOfWordType.Personal:
            {
                var success = BagOfWordManager.RemoveBagOfWord(target, BagOfWordType.Personal);
                return success
                    ? messageChain.CreateSameTypeTextMessage($"成功移除[Personal:{target}]词袋")
                    : messageChain.CreateSameTypeTextMessage("移除失败");
            }
            case BagOfWordType.Global:
                return messageChain.CreateSameTypeTextMessage("全局词袋无法移除");
            default:
                return messageChain.CreateSameTypeTextMessage("参数异常");
        }
    }
}