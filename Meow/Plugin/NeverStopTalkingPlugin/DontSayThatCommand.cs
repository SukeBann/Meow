using Lagrange.Core.Message;
using Masuit.Tools;
using Meow.Core;
using Meow.Utils;

namespace Meow.Plugin.NeverStopTalkingPlugin;

/// <summary>
/// 为插件<see cref="NeverStopTalkingPlugin"/>添加 违禁词管理功能
/// </summary>
public class DontSayThatCommand : IMeowCommand
{
    /// <inheritdoc />
    public string CommandUid => "371EE19D-009A-43F9-A712-4DDF8AD71248";

    /// <inheritdoc />
    public string CommandTrigger => "dontSayThat";

    /// <inheritdoc />
    public string CommandPrint => "[dontSayThat] 屏蔽词管理";

    /// <inheritdoc />
    public bool IsNeedAdmin => true;


    /// <inheritdoc />
    public string CommandHelpDescription => """
                                        dontSayThat 命令:

                                        不让机器人在胡说八道时说出违禁词>
                                        示例：dontSayThat add 牛马(违禁词字数范围1 - 4) 
                                        >> 此后机器人便不会在胡说八道时说出带牛马的词

                                        移除违禁词>
                                        示例：dontSayThat remove 牛马 
                                        >> 移除之前添加的某个违禁词
                                        """;

    /// <inheritdoc />
    public (bool needSendMessage, MessageChain messageChain) RunCommand(Core.Meow meow, MessageChain messageChain,
        string? args)
    {
        var sender = messageChain.FriendUin;
        var emptyMessage = messageChain.CreateSameTypeMessageBuilder();

        if (args.IsNullOrEmpty())
        {
            emptyMessage.Text("命令参数不能为空");
            emptyMessage.Build();
            return (true, emptyMessage.Build());
        }

        var splitArgs = args!.Split(' ', 2);
        if (splitArgs is not ["add" or "remove", _] && splitArgs[1].Length is >= 1 and <= 4)
        {
            emptyMessage.Text("参数错误, 第1个参数必须是add或者remove, 第二个参数违禁词的长度必须大于等于1 小于等于4");
            emptyMessage.Build();
            return (true, emptyMessage.Build());
        }

        var action = splitArgs[0];
        var forbiddenWord = splitArgs[1];
        var manager = new ForbiddenWordsManager(meow, sender);
        if (action == "add")
        {
            manager.AddForbiddenWord(forbiddenWord);
        }
        else
        {
            manager.RemoveForbiddenWord(forbiddenWord);
        }

        emptyMessage.Text($"命令已执行 {(action == "add" ? "添加" : "删除")}违禁词: {forbiddenWord}");
        return (true, emptyMessage.Build());
    }
}