using System.Reactive.Linq;
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

                                             查询词袋 信息>
                                             示例{CommandTrigger} query [Group|Personal|Global] 目标[群号|qq|Global的话这里为0]  
                                             >> 查询[目标群|个人|全局]的词袋状态
                                             >> 输出: 词袋id 词袋类型 词袋大小 词袋生成的向量个数 创建时间 是否被删除 状态：填充数量/最大数量 
                                             
                                             词袋编辑相关(耗时操作)>
                                             示例{CommandTrigger} msg [Group|Personal|Global] 目标[群号|qq|Global的话这里为0] 
                                             >> 查询[目标群|个人|全局]所有信息可以构建什么大小的词袋
                                             >> 输出: 目标类型 目标ID 总计消息数量 可构建词袋大小 
                                             
                                             示例{CommandTrigger} rebuild [Group|Personal|Global] 目标[群号|qq|Global的话这里为0] 词袋大小
                                             >> 重新构建目标的词袋为指定大小的词袋, 如果词袋构建完之后是满的则重新计算所有相关消息向量
                                             >> 输出: 目标类型 目标ID 构建后状态 重新计算消息向量数量 

                                             """;

    /// <inheritdoc />
    public bool IsNeedAdmin => true;

    /// <inheritdoc />
    public async Task<(bool needSendMessage, MessageChain messageChain)> RunCommand(Core.Meow meow,
        MessageChain messageChain,
        string? args)
    {
        var success = new CommandArgsCheckUtil(messageChain, args)
            .SplitArgsAndCheckLength(' ', 4, new Range(1, 4), "参数数量错误, 请检查参数格式")
            .ArgListMatch(0, ["add", "remove", "query", "msg", "rebuild"])
            .ArgListMatch(1, ["Group", "Personal", "Global"])
            .ArgListRegexMatch(2, @"^([1-9][0-9]*|0)$", "uin只能为正整数")
            .RegexWhenParamIs(1, "rebuild", 3, @"^([1-9][0-9]*|0)$", "指定的词袋大小只能为正整数")
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
            "msg" => (true, await Msg(messageChain, type, target).ConfigureAwait(false)),
            "rebuild" => (true, await Rebuild(messageChain, type, target).ConfigureAwait(false)),
            _ => (true, messageChain.CreateSameTypeTextMessage("参数异常"))
        };
    }

    private async Task<MessageChain> Msg(MessageChain messageChain, BagOfWordType type, uint target)
    {
        var queryMsgCutBagOfWordCount = await BagOfWordManager.QueryMsgCutBagOfWordCount(type, target).ConfigureAwait(false);
        return messageChain.CreateSameTypeTextMessage(queryMsgCutBagOfWordCount);
    }

    private async Task<MessageChain> Rebuild(MessageChain messageChain, BagOfWordType result, uint target)
    {
        BagOfWordManager.BoWBusyStateChange.OnNext(true);
        var rebuildBagOfWord = await BagOfWordManager.RebuildBagOfWord(result, target).ConfigureAwait(false);
        BagOfWordManager.BoWBusyStateChange.OnNext(false);
        return messageChain.CreateSameTypeTextMessage(rebuildBagOfWord);
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
                BagOfWordManager.QueryBagOfWordCommand(target, BagOfWordType.Group, out msg);
                break;
            }
            case BagOfWordType.Personal:
            {
                BagOfWordManager.QueryBagOfWordCommand(target,
                    BagOfWordType.Personal,
                    out msg);
                break;
            }
            case BagOfWordType.Global:
            {
                BagOfWordManager.QueryBagOfWordCommand(target, BagOfWordType.Global, out msg);
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