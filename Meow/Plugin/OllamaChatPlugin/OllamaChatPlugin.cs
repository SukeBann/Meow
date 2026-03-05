using Camille.Core.MiraiBase.Contract;
using Camille.Core.MiraiBase.Models.BasicMessage;
using Camille.Imp.Extension;
using Camille.Imp.MiraiBase.Message.MessageContainer;
using Meow.Core;
using Meow.Core.Model.Base;
using Meow.Plugin.OllamaChatPlugin.Command;
using Meow.Plugin.OllamaChatPlugin.Models;
using Meow.Plugin.OllamaChatPlugin.Service;
using Newtonsoft.Json;

namespace Meow.Plugin.OllamaChatPlugin;

public class OllamaChatPlugin : PluginBase
{
    public override string PluginName => "Ollama Chat Plugin";
    public override string PluginDescription => "接入Ollama API实现AI聊天功能，模拟群员对话。";
    public override string PluginUid => "B7D1A2C4-E5F6-4A8B-9C0D-1E2F3A4B5C6D";

    public override List<IMeowCommand> Commands { get; } = new();

    private OllamaConfig _config;
    private OllamaApiService _apiService;
    private readonly string _configPath;
    private IDisposable? _messageSubscription;
    private readonly Random _random = new();

    // 简单的内存上下文维护，Key为群号或私聊QQ号
    private readonly Dictionary<long, List<OllamaMessage>> _context = new();

    public OllamaChatPlugin()
    {
        _configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PluginResource", "OllamaChatPlugin", "config.json");
        LoadConfig();
    }

    private void LoadConfig()
    {
        try
        {
            if (System.IO.File.Exists(_configPath))
            {
                var json = System.IO.File.ReadAllText(_configPath);
                _config = JsonConvert.DeserializeObject<OllamaConfig>(json) ?? new OllamaConfig();
            }
            else
            {
                _config = new OllamaConfig();
                SaveConfig();
            }
        }
        catch (Exception)
        {
            _config = new OllamaConfig();
        }
    }

    private void SaveConfig()
    {
        try
        {
            var dir = Path.GetDirectoryName(_configPath);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir!);
            }
            var json = JsonConvert.SerializeObject(_config, Formatting.Indented);
            System.IO.File.WriteAllText(_configPath, json);
        }
        catch (Exception e)
        {
            Bot?.Error("Failed to save OllamaChatPlugin config", e);
        }
    }

    public override void InjectPlugin(Core.Meow host)
    {
        Commands.Add(new OllamaConfigCommand(_config, SaveConfig));
        base.InjectPlugin(host);
        _apiService = new OllamaApiService(host, _config);

        _messageSubscription = host.OnMiraiMessageReceived.Subscribe(async x =>
        {
            await HandleMessage(x).ConfigureAwait(false);
        });
    }

    private async Task HandleMessage(IMiraiMessageContainer container)
    {
        if (Bot == null) return;

        // 获取纯文本内容
        var textMsg = container.MessageChain.GetPlainMessage().Trim();
        if (string.IsNullOrEmpty(textMsg)) return;

        long uin = 0;

        var role = "user";
        if (container is GroupMiraiMsgContainer groupMsg)
        {
            uin = groupMsg.Sender?.Group?.Id ?? 0;
            role = groupMsg.Sender?.MemberName ?? "role";
        }
        else if (container is FriendMiraiMsgContainer friendMsg)
        {
            uin = friendMsg.Sender?.Id ?? 0;
            role = friendMsg.Sender?.Nickname ?? "role";
        }

        if (uin == 0) return;

        bool shouldTrigger = false;

        // 检查At
        if (_config.TriggerOnAt)
        {
            if (container.MessageChain.Any(x => x is At at && at.Target == Bot.BotQq))
            {
                shouldTrigger = true;
            }
        }

        // 检查概率
        if (!shouldTrigger)
        {
            if (_random.Next(0, 1000) < _config.TriggerProbability)
            {
                shouldTrigger = true;
            }
        }

        // 记录上下文
        UpdateContext(uin, role, textMsg);

        if (shouldTrigger)
        {
            await ReplyWithAi(container, uin).ConfigureAwait(false);
        }
    }

    private void UpdateContext(long uin, string role, string content)
    {
        if (!_context.ContainsKey(uin))
        {
            _context[uin] = new List<OllamaMessage>();
        }

        var messages = _context[uin];
        messages.Add(new OllamaMessage { Role = role, Content = content });

        // 限制长度
        if (messages.Count > _config.ContextMessageCount)
        {
            messages.RemoveAt(0);
        }
    }

    private async Task ReplyWithAi(IMiraiMessageContainer container, long uin)
    {
        var messages = new List<OllamaMessage>
        {
            new OllamaMessage { Role = "system", Content = _config.SystemPrompt }
        };
        
        if (_context.TryGetValue(uin, out var history))
        {
            messages.AddRange(history);
        }

        var response = await _apiService.GetChatResponseAsync(messages).ConfigureAwait(false);
        if (!string.IsNullOrEmpty(response))
        {
            // 记录AI的回复到上下文
            UpdateContext(uin, "assistant", response);

            // 发送回复
            container.MessageChain = new Camille.Core.MiraiBase.Models.Base.MessageChain { new Plain(response) };
            await container.SendToAsync(Bot).ConfigureAwait(false);
        }
    }

    public override void Remove()
    {
        _messageSubscription?.Dispose();
        base.Remove();
    }
}
