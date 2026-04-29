using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Camille.Core.MiraiBase.Models.Base;
using Camille.Core.MiraiBase.Models.BasicMessage;
using Camille.Imp.MiraiBase.Message;
using Camille.Imp.MiraiBase.Message.MessageContainer;
using Meow.Core;
using Meow.Plugin.OllamaChatPlugin.Service;
using Meow.Utils;
using File = System.IO.File;

namespace Meow.Plugin.OllamaChatPlugin.Command;

/// <summary>
/// 提供专业AI问答模式
/// </summary>
public class OllamaAgentCommand : IMeowCommand
{
    private const int MaxHistoryMessages = 40;
    private readonly string _agentPromptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
        "PluginResource", "OllamaChatPlugin", "agent_system_prompt.txt");

    private OllamaApiService? ApiService { get; set; }
    private FishTtsApiService? TtsApiService { get; set; }
    private ConcurrentDictionary<long, List<OllamaMessage>> UserMessageContext { get; set; } = new();
    private ConcurrentDictionary<long, SemaphoreSlim> UserSessionLocks { get; set; } = new();

    public OllamaAgentCommand(OllamaApiService apiService, FishTtsApiService? ttsApiService)
    {
        ApiService = apiService;
        TtsApiService = ttsApiService;
    }

    public string CommandUid => "25847717-034C-4517-9C01-56BA255F41F8";
    public bool IsNeedAdmin => false;
    public string CommandTrigger => "agent";
    public string CommandPrint => $"[{CommandTrigger}] 与专业模式的AI发起对话";

    public string CommandHelpDescription => """
                                            agent 命令:
                                            与专业模式的AI发起对话

                                            用法:
                                            - agent t 你好!: 命令后接对话文本开启对话(每个用户有独立的上下文)
                                            - agent v 你好!: 创建AI会用语音进行回复的对话
                                            - agent n 你好!: 创建新的会话(清除用户上下文)
                                            - agent nv 你好!: 语音创建新的会话(清除用户上下文)
                                            - agent nt 你好!: 文本创建新的会话(清除用户上下文)
                                            """;

    public async Task<(bool needSendMessage, MessageChain messageChain)> RunCommand(Core.Meow meow,
        MiraiMsgContainerBase container, string? args)
    {
        var success = new CommandArgsCheckUtil(container.MessageChain, args)
            .SplitArgsAndCheckLength(' ', 2, new Range(1, 2), "参数数量错误, 请检查参数格式")
            .ArgListMatch(0, ["t", "v", "n", "nv", "nt"])
            .IsSuccess(out var errorMsg, out var resultChain, out var argStr, out var argList);

        if (!success)
        {
            return (true, resultChain);
        }

        var uin = container switch
        {
            GroupMiraiMsgContainer groupMsg => groupMsg.Sender?.Group?.Id ?? 0,
            FriendMiraiMsgContainer friendMsg => friendMsg.Sender?.Id ?? 0,
            _ => 0
        };

        if (uin == 0)
        {
            return (true, (MessageChain) "无法识别当前会话, 请重试。");
        }

        var useVoiceReply = false;
        var createNewSession = false;

        switch (argList?.FirstOrDefault())
        {
            case "t":
                useVoiceReply = false;
                break;
            case "v":
                useVoiceReply = true;
                break;
            case "n":
            case "nt":
                useVoiceReply = false;
                createNewSession = true;
                break;
            case "nv":
                useVoiceReply = true;
                createNewSession = true;
                break;
            default:
                return (true, (MessageChain) "错误的命令参数。");
        }

        var userText = argList is {Count: >= 2} ? argList[1].Trim() : string.Empty;
        if (string.IsNullOrWhiteSpace(userText))
        {
            return (true, (MessageChain) "请输入要发送给 AI 的内容。");
        }

        var sessionLock = UserSessionLocks.GetOrAdd(uin, _ => new SemaphoreSlim(1, 1));
        await sessionLock.WaitAsync().ConfigureAwait(false);
        try
        {
            var messages = createNewSession
                ? ResetSession(uin)
                : UserMessageContext.GetOrAdd(uin, _ => CreateNewSessionMessages());

            EnsureSystemPrompt(messages);
            messages.Add(new OllamaMessage
            {
                Role = "user",
                Content = $"{(useVoiceReply ? "<!本次回复需要生成语音!>" : "")}{userText}",
                Timestamp = DateTime.Now
            });
            TrimHistory(messages);

            var response = await ApiService!.GetChatResponseAsync(messages.ToList()).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(response))
            {
                return (true, (MessageChain) "AI 暂时没有返回内容。");
            }

            response = NormalizeResponse(response);
            if (string.IsNullOrWhiteSpace(response))
            {
                return (true, (MessageChain) "AI 返回了空内容。");
            }

            var replyTextForContext = RemoveVoiceLabels(response);
            messages.Add(new OllamaMessage
            {
                Role = "assistant",
                Content = replyTextForContext,
                Timestamp = DateTime.Now
            });
            TrimHistory(messages);

            if (useVoiceReply && TtsApiService != null)
            {
                var voiceMessage = await TryCreateVoiceMessage(meow, response).ConfigureAwait(false);
                if (voiceMessage != null)
                {
                    return (true, voiceMessage);
                }
            }

            return (true, (MessageChain) replyTextForContext);
        }
        finally
        {
            sessionLock.Release();
        }
    }

    private List<OllamaMessage> ResetSession(long uin)
    {
        var messages = CreateNewSessionMessages();
        UserMessageContext[uin] = messages;
        return messages;
    }

    private List<OllamaMessage> CreateNewSessionMessages()
    {
        return
        [
            new OllamaMessage
            {
                Role = "system",
                Content = LoadAgentPrompt(),
                Timestamp = DateTime.Now
            }
        ];
    }

    private void EnsureSystemPrompt(List<OllamaMessage> messages)
    {
        if (messages.Count == 0 || !messages[0].Role.Equals("system", StringComparison.OrdinalIgnoreCase))
        {
            messages.Insert(0, new OllamaMessage
            {
                Role = "system",
                Content = LoadAgentPrompt(),
                Timestamp = DateTime.Now
            });
        }
    }

    private string LoadAgentPrompt()
    {
        return File.Exists(_agentPromptPath) ? File.ReadAllText(_agentPromptPath) : string.Empty;
    }

    private static void TrimHistory(List<OllamaMessage> messages)
    {
        while (messages.Count > MaxHistoryMessages + 1)
        {
            messages.RemoveAt(1);
        }
    }

    private static string NormalizeResponse(string response)
    {
        response = Regex.Replace(response, @"<think>[\s\S]*?</think>", "").Trim();
        if (response.Length > 1000)
        {
            response = response[..1000] + "... (回复过长已截断)";
        }

        response = response.Replace("\u0000", "");
        response = Regex.Replace(response, @"(\r?\n){3,}", "\n\n");

        if (response.StartsWith("```") && response.EndsWith("```"))
        {
            var match = Regex.Match(response, @"```(?:\w+)?\s*([\s\S]*?)\s*```");
            if (match.Success)
            {
                response = match.Groups[1].Value.Trim();
            }
        }

        return response;
    }

    private static string RemoveVoiceLabels(string response)
    {
        response = Regex.Replace(response, @"<voice_intent>.*?</voice_intent>", "", RegexOptions.Singleline).Trim();
        response = Regex.Replace(response, @"<v>.*?</v>", "");
        return response.Trim();
    }

    private async Task<MessageChain?> TryCreateVoiceMessage(Core.Meow meow, string response)
    {
        var ttsText = Regex.Replace(response, @"<voice_intent>.*?</voice_intent>", "", RegexOptions.Singleline).Trim();
        ttsText = ttsText.Replace("<v>", "[").Replace("</v>", "]");

        var fileName = $"tts_{Guid.NewGuid():N}.mp3";
        var filePath = Path.Combine(Path.GetTempPath(), fileName);
        try
        {
            if (!await TtsApiService!.SynthesizeToFileAsync(ttsText, filePath).ConfigureAwait(false))
            {
                return null;
            }

            return [new Voice {Path = filePath}];
        }
        catch (Exception e)
        {
            meow.Error("Send Voice Msg Error", e);
            return null;
        }
    }
}
