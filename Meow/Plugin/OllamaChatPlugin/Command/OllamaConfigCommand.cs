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
    private readonly Action<string> _updateRoleAction;

    public OllamaConfigCommand(OllamaConfig config, Action saveConfigAction, Action<string> updateRoleAction)
    {
        _config = config;
        _saveConfigAction = saveConfigAction;
        _updateRoleAction = updateRoleAction;
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
                                            示例：ol-cfg set role 你是一个高冷的人
                                            示例：ol-cfg set freq 10
                                            
                                            可用配置项:
                                            - probability: 触发概率 (0-1000)
                                            - model: 模型名称
                                            - prompt: 预制对话提示词
                                            - role: 机器人角色设定 (支持多次编辑)
                                            - summary_prompt: 总结分析提示词
                                            - freq: 总结触发频率 (消息数)
                                            - cooldown: 冷却时间 (秒)
                                            - context_limit: 设置全局上下文条数上限 (默认20, 最大60)
                                            - target_limit: 设置指定群或用户的上下文上限 (示例: ol-cfg set target_limit 123456 30)
                                            - allow_group: 添加允许的群聊 (示例: ol-cfg set allow_group 123456)
                                            - remove_group: 移除允许的群聊 (示例: ol-cfg set remove_group 123456)
                                            - enable_private: 是否启用私聊 (true/false)
                                            """;

    public Task<(bool needSendMessage, MessageChain messageChain)> RunCommand(Core.Meow meow, MiraiMsgContainerBase container, string? args)
    {
        var success = new CommandArgsCheckUtil(container.MessageChain, args)
            .SplitArgsAndCheckLength(' ', 3, new Range(1, 3), "参数数量错误, 请检查参数格式")
            .ArgListMatch(0, ["list", "set"])
            .IsSuccess(out var errorMsg, out var resultChain, out var argStr, out var argList);

        if (!success)
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
            sb.AppendLine($"上下文条数: {_config.ContextMessageCount}");
            sb.AppendLine($"特定目标限制: {string.Join(", ", _config.TargetContextLimits.Select(x => $"{x.Key}:{x.Value}"))}");
            sb.AppendLine($"总结频率: {_config.SummaryFrequency}");
            sb.AppendLine($"冷却时间: {_config.CooldownSeconds}s");
            sb.AppendLine($"私聊启用: {_config.EnablePrivateChat}");
            sb.AppendLine($"允许群聊: {string.Join(", ", _config.AllowedGroups)}");
            sb.AppendLine($"对话提示词: {_config.SystemPrompt}");
            sb.AppendLine($"总结提示词: {_config.SummaryPrompt}");
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
                    return Task.FromResult((true, (MessageChain)$"已将对话提示词设置为: {value}"));
                case "role":
                    _updateRoleAction(value);
                    return Task.FromResult((true, (MessageChain)$"已将角色设定设置为: {value}"));
                case "summary_prompt":
                    _config.SummaryPrompt = value;
                    _saveConfigAction();
                    return Task.FromResult((true, (MessageChain)$"已将总结提示词设置为: {value}"));
                case "freq":
                    if (int.TryParse(value, out var freq))
                    {
                        _config.SummaryFrequency = freq;
                        _saveConfigAction();
                        return Task.FromResult((true, (MessageChain)$"已将总结频率设置为: {freq}"));
                    }
                    break;
                case "cooldown":
                    if (int.TryParse(value, out var cd))
                    {
                        _config.CooldownSeconds = cd;
                        _saveConfigAction();
                        return Task.FromResult((true, (MessageChain)$"已将冷却时间设置为: {cd}s"));
                    }
                    break;
                case "context_limit":
                    if (int.TryParse(value, out var limit))
                    {
                        if (limit is < 1 or > 60)
                        {
                            return Task.FromResult((true, (MessageChain)"上下文限制必须在 1 到 60 之间"));
                        }
                        _config.ContextMessageCount = limit;
                        _saveConfigAction();
                        return Task.FromResult((true, (MessageChain)$"已将全局上下文限制设置为: {limit}"));
                    }
                    break;
                case "target_limit":
                    var targetArgs = value.Split(' ');
                    if (targetArgs.Length >= 2 && long.TryParse(targetArgs[0], out var targetId) && int.TryParse(targetArgs[1], out var tLimit))
                    {
                        if (tLimit is < 1 or > 60)
                        {
                            return Task.FromResult((true, (MessageChain)"上下文限制必须在 1 到 60 之间"));
                        }
                        _config.TargetContextLimits[targetId] = tLimit;
                        _saveConfigAction();
                        return Task.FromResult((true, (MessageChain)$"已将目标 {targetId} 的上下文限制设置为: {tLimit}"));
                    }
                    break;
                case "allow_group":
                    if (long.TryParse(value, out var groupId))
                    {
                        if (!_config.AllowedGroups.Contains(groupId))
                        {
                            _config.AllowedGroups.Add(groupId);
                            _saveConfigAction();
                            return Task.FromResult((true, (MessageChain)$"已添加允许群聊: {groupId}"));
                        }
                        return Task.FromResult((true, (MessageChain)$"群聊 {groupId} 已在白名单中"));
                    }
                    break;
                case "remove_group":
                    if (long.TryParse(value, out var removeGroupId))
                    {
                        if (_config.AllowedGroups.Remove(removeGroupId))
                        {
                            _saveConfigAction();
                            return Task.FromResult((true, (MessageChain)$"已移除允许群聊: {removeGroupId}"));
                        }
                        return Task.FromResult((true, (MessageChain)$"群聊 {removeGroupId} 不在白名单中"));
                    }
                    break;
                case "enable_private":
                    if (bool.TryParse(value, out var enablePrivate))
                    {
                        _config.EnablePrivateChat = enablePrivate;
                        _saveConfigAction();
                        return Task.FromResult((true, (MessageChain)$"已将私聊启用设置为: {enablePrivate}"));
                    }
                    break;
            }
            return Task.FromResult((true, (MessageChain)"配置项不存在或值格式错误"));
        }

        return Task.FromResult((true, (MessageChain)CommandHelpDescription));
    }
}
