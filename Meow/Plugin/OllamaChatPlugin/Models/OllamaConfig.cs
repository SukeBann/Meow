using Newtonsoft.Json;

namespace Meow.Plugin.OllamaChatPlugin.Models;

public class OllamaConfig
{
    /// <summary>
    /// Ollama API 地址
    /// </summary>
    public string ApiUrl { get; set; } = "http://localhost:11434/api/chat";

    /// <summary>
    /// 使用的模型名称
    /// </summary>
    public string Model { get; set; } = "llama3";

    /// <summary>
    /// 预制提示词 (System Prompt) - 对话生成用
    /// </summary>
    [JsonIgnore]
    public string SystemPrompt { get; set; } = string.Empty;

    /// <summary>
    /// 总结提示词 (Summary Prompt) - 分析群组状态用
    /// </summary>
    [JsonIgnore]
    public string SummaryPrompt { get; set; } = string.Empty;

    /// <summary>
    /// 触发总结的频率（每收多少条消息总结一次）
    /// </summary>
    public int SummaryFrequency { get; set; } = 20;

    /// <summary>
    /// 触发概率 (0-1000)
    /// </summary>
    public int TriggerProbability { get; set; } = 500;

    /// <summary>
    /// 冷却时间（秒），防止发言太快
    /// </summary>
    public int CooldownSeconds { get; set; } = 60;

    /// <summary>
    /// 最大Token限制
    /// </summary>
    public int MaxTokens { get; set; } = 2000;

    /// <summary>
    /// 上下文消息条数限制
    /// </summary>
    public int ContextMessageCount { get; set; } = 20;

    /// <summary>
    /// 特定用户或群聊的上下文消息条数限制
    /// </summary>
    public Dictionary<long, int> TargetContextLimits { get; set; } = new();

    /// <summary>
    /// 允许使用插件的群聊白名单
    /// </summary>
    public List<long> AllowedGroups { get; set; } = new();

    /// <summary>
    /// 是否启用私聊对话
    /// </summary>
    public bool EnablePrivateChat { get; set; } = true;
}
