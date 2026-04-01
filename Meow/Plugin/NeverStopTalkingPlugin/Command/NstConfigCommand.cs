using System.Text;
using Camille.Core.MiraiBase.Models.Base;
using Camille.Imp.MiraiBase.Message;
using Meow.Core;
using Meow.Plugin.NeverStopTalkingPlugin.Models;
using Meow.Utils;

namespace Meow.Plugin.NeverStopTalkingPlugin.Command;

public class NstConfigCommand : IMeowCommand
{
    private readonly NstConfig _config;
    private readonly Action _saveConfigAction;

    public NstConfigCommand(NstConfig config, Action saveConfigAction)
    {
        _config = config;
        _saveConfigAction = saveConfigAction;
    }

    public string CommandUid => "6A7B8C9D-E0F1-4A2B-8C3D-9E0A1B2C3D4E";
    public bool IsNeedAdmin => true;
    public string CommandTrigger => "nst-cfg";
    public string CommandPrint => $"[{CommandTrigger}] NeverStopTalking插件配置管理";

    public string CommandHelpDescription => """
                                            nst-cfg 命令:
                                            用于管理 Never Stop Talking 插件的配置。

                                            查看当前配置>
                                            示例：nst-cfg list
                                            
                                            修改配置项>
                                            示例：nst-cfg set probability 600
                                            示例：nst-cfg set cooldown 60
                                            示例：nst-cfg set speak true
                                            
                                            可用配置项:
                                            - probability: 触发概率 (0-1000)
                                            - cooldown: 冷却时间 (秒)
                                            - speak: 是否允许发言 (true/false)
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
            sb.AppendLine("--- Never Stop Talking 当前配置 ---");
            sb.AppendLine($"触发概率: {_config.TriggerProbability}/1000");
            sb.AppendLine($"冷却时间: {_config.CooldownSeconds}s");
            sb.AppendLine($"允许发言: {_config.CanSpeak}");
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
                case "cooldown":
                    if (int.TryParse(value, out var cd))
                    {
                        _config.CooldownSeconds = cd;
                        _saveConfigAction();
                        return Task.FromResult((true, (MessageChain)$"已将冷却时间设置为: {cd}s"));
                    }
                    break;
                case "speak":
                    if (bool.TryParse(value, out var canSpeak))
                    {
                        _config.CanSpeak = canSpeak;
                        _saveConfigAction();
                        return Task.FromResult((true, (MessageChain)$"已将允许发言设置为: {canSpeak}"));
                    }
                    break;
            }
            return Task.FromResult((true, (MessageChain)"配置项不存在或值格式错误"));
        }

        return Task.FromResult((true, (MessageChain)CommandHelpDescription));
    }
}
