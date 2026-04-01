using Camille.Core.MiraiBase.Models.Base;
using Camille.Imp.MiraiBase.Message;
using Camille.Imp.MiraiBase.Message.MessageContainer;
using Meow.Core;

namespace Meow.Plugin.OllamaChatPlugin.Command;

public class OllamaClearCommand : IMeowCommand
{
    private readonly Action<long, bool, bool> _clearContextAction;

    public OllamaClearCommand(Action<long, bool, bool> clearContextAction)
    {
        _clearContextAction = clearContextAction;
    }

    public string CommandUid => "E1F2G3H4-A5B6-4C7D-8E9F-0A1B2C3D4E5F";
    public bool IsNeedAdmin => true;
    public string CommandTrigger => "ol-clear";
    public string CommandPrint => $"[{CommandTrigger}] 清除当前聊天上下文";
    public string CommandHelpDescription => """
                                            ol-clear 命令:
                                            清除当前对话的聊天记录上下文或用户画像。
                                            
                                            用法:
                                            - ol-clear: 清除全部 (聊天记录 + 用户画像)
                                            - ol-clear chat: 仅清除聊天记录
                                            - ol-clear profile: 仅清除用户画像 (总结结果)
                                            """;

    public Task<(bool needSendMessage, MessageChain messageChain)> RunCommand(Core.Meow meow, MiraiMsgContainerBase container, string? args)
    {
        long uin = 0;
        string type = "";

        if (container is GroupMiraiMsgContainer groupMsg)
        {
            uin = groupMsg.Sender?.Group?.Id ?? 0;
            type = "群聊";
        }
        else if (container is FriendMiraiMsgContainer friendMsg)
        {
            uin = friendMsg.Sender?.Id ?? 0;
            type = "私聊";
        }

        if (uin == 0)
        {
            return Task.FromResult((true, (MessageChain)"无法识别当前会话，清理失败。"));
        }

        bool clearChat = true;
        bool clearSummary = true;
        string actionDesc = "全部上下文";

        if (!string.IsNullOrWhiteSpace(args))
        {
            var subArg = args.Trim().ToLower();
            switch (subArg)
            {
                case "chat":
                    clearChat = true;
                    clearSummary = false;
                    actionDesc = "聊天记录";
                    break;
                case "profile":
                    clearChat = false;
                    clearSummary = true;
                    actionDesc = "用户画像";
                    break;
            }
        }

        _clearContextAction(uin, clearChat, clearSummary);
        return Task.FromResult((true, (MessageChain)$"已成功清理当前{type}的{actionDesc}。"));
    }
}
