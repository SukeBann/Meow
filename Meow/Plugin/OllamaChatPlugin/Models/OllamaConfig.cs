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
    /// 预制提示词 (System Prompt)
    /// </summary>
    public string SystemPrompt { get; set; } = "你是一个群聊成员，说话幽默风趣，简短有力。";

    /// <summary>
    /// 触发概率 (0-1000)
    /// </summary>
    public int TriggerProbability { get; set; } = 50;

    /// <summary>
    /// 是否被At时强制触发
    /// </summary>
    public bool TriggerOnAt { get; set; } = true;

    /// <summary>
    /// 最大Token限制
    /// </summary>
    public int MaxTokens { get; set; } = 500;

    /// <summary>
    /// 上下文消息条数限制
    /// </summary>
    public int ContextMessageCount { get; set; } = 10;
}
