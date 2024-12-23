﻿using Lagrange.Core.Message;
using Meow.Core;
using Meow.Plugin.NeverStopTalkingPlugin.Service;
using Meow.Utils;

namespace Meow.Plugin.NeverStopTalkingPlugin.Command;

/// <summary>
/// 为插件<see cref="NeverStopTalkingPlugin"/>添加 违禁词管理功能
/// </summary>
public class NstDontSayThatCommand(ForbiddenWordsManager forbiddenWordsManager) : IMeowCommand
{
    private ForbiddenWordsManager ForbiddenWordsManager { get; } = forbiddenWordsManager;

    /// <inheritdoc />
    public string CommandUid => "371EE19D-009A-43F9-A712-4DDF8AD71248";

    /// <inheritdoc />
    public string CommandTrigger => "dontSayThat";

    /// <inheritdoc />
    public string CommandPrint => $"[{CommandTrigger}] 屏蔽词管理";

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
    public Task<(bool needSendMessage, MessageChain messageChain)> RunCommand(Core.Meow meow, MessageChain messageChain,
        string? args)
    {
        var sender = messageChain.FriendUin;
        var emptyMessage = messageChain.CreateSameTypeMessageBuilder();

        var argsCheck = new CommandArgsCheckUtil(messageChain, args);
        var checkResult = argsCheck
            .SplitArgsAndCheckLength(' ', 2, new Range(2, 2), "参数数量错误, 请检查参数格式")
            .ArgListMatch(0, ["add", "remove"])
            .ArgListLength(1, 4, 1)
            .IsSuccess(out var msg, out var errorMessageChain, out var arg, out var splitResult);

        if (!checkResult)
        {
            return Task.FromResult((true, errorMessageChain));
        }

        var action = splitResult[0];
        var forbiddenWord = splitResult[1];
        if (action == "add")
        {
            ForbiddenWordsManager.AddForbiddenWord(forbiddenWord, sender);
        }
        else
        {
            ForbiddenWordsManager.RemoveForbiddenWord(forbiddenWord);
        }

        emptyMessage.Text($"命令已执行 {(action == "add" ? "添加" : "删除")}违禁词: {forbiddenWord}");
        return Task.FromResult((true, emptyMessage.Build()));
    }
}