using System.Text;
using Camille.Core.MiraiBase.Models.Base;
using Camille.Imp.MiraiBase.Message;
using Meow.Core;
using Meow.Plugin.OllamaChatPlugin.Models;
using Meow.Utils;

namespace Meow.Plugin.OllamaChatPlugin.Command;

public class OllamaConfigCommand : IMeowCommand
{
    private readonly OllamaConfig _config;
    private readonly Action _saveConfigAction;

    public OllamaConfigCommand(OllamaConfig config, Action saveConfigAction)
    {
        _config = config;
        _saveConfigAction = saveConfigAction;
    }

    public string CommandUid => "C2D3E4F5-A1B2-4C3D-8E9F-0A1B2C3D4E5F";
    public bool IsNeedAdmin => true;
    public string CommandTrigger => "ol-cfg";
    public string CommandPrint => $"[{CommandTrigger}] Ollama插件配置管理";

    public string CommandHelpDescription => """
                                            ol-cfg 命令:
                                            用于管理Ollama AI聊天插件的配置。

                                            查看当前配置>
                                            示例：ol-cfg list
                                            
                                            修改配置项>
                                            示例：ol-cfg set probability 100
                                            示例：ol-cfg set model llama3
                                            示例：ol-cfg set prompt 你是一个可爱的助手
                                            
                                            可用配置项:
                                            - probability: 触发概率 (0-1000)
                                            - model: 模型名称
                                            - prompt: 预制提示词
                                            - at: 是否At触发 (true/false)
                                            """;

    public Task<(bool needSendMessage, MessageChain messageChain)> RunCommand(Core.Meow meow, MiraiMsgContainerBase container, string? args)
    {
        var check = new CommandArgsCheckUtil(container.MessageChain, args);
        if (!check.IsSuccess(out var errorMsg, out var resultChain, out var argStr, out var argList))
        {
            return Task.FromResult((true, resultChain));
        }

        if (argList[0] == "list")
        {
            var sb = new StringBuilder();
            sb.AppendLine("--- Ollama 当前配置 ---");
            sb.AppendLine($"API地址: {_config.ApiUrl}");
            sb.AppendLine($"模型: {_config.Model}");
            sb.AppendLine($"触发概率: {_config.TriggerProbability}/1000");
            sb.AppendLine($"At触发: {_config.TriggerOnAt}");
            sb.AppendLine($"上下文条数: {_config.ContextMessageCount}");
            sb.AppendLine($"提示词: {_config.SystemPrompt}");
            return Task.FromResult((true, (MessageChain)sb.ToString().Trim()));
        }

        if (argList[0] == "set" && argList.Count >= 3)
        {
            var key = argList[1].ToLower();
            var value = string.Join(" ", argList.Skip(2));

            switch (key)
            {
                case "probability":
                    if (int.TryParse(value, out var p))
                    {
                        _config.TriggerProbability = p;
                        _saveConfigAction();
                        return Task.FromResult((true, (MessageChain)$"已将触发概率设置为: {p}"));
                    }
                    break;
                case "model":
                    _config.Model = value;
                    _saveConfigAction();
                    return Task.FromResult((true, (MessageChain)$"已将模型设置为: {value}"));
                case "prompt":
                    _config.SystemPrompt = value;
                    _saveConfigAction();
                    return Task.FromResult((true, (MessageChain)$"已将提示词设置为: {value}"));
                case "at":
                    if (bool.TryParse(value, out var at))
                    {
                        _config.TriggerOnAt = at;
                        _saveConfigAction();
                        return Task.FromResult((true, (MessageChain)$"已将At触发设置为: {at}"));
                    }
                    break;
            }
            return Task.FromResult((true, (MessageChain)"配置项不存在或值格式错误"));
        }

        return Task.FromResult((true, (MessageChain)CommandHelpDescription));
    }
}
