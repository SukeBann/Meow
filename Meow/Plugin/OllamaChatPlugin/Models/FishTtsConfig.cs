namespace Meow.Plugin.OllamaChatPlugin.Models;

public class FishTtsConfig
{
    /// <summary>
    /// TTS API 地址
    /// </summary>
    public string ApiUrl { get; set; } = "https://api.fish.audio/v1/tts";

    /// <summary>
    /// 模型名称
    /// </summary>
    public string Model { get; set; } = "s2-pro";

    /// <summary>
    /// 输出音频格式
    /// </summary>
    public string Format { get; set; } = "mp3";

    /// <summary>
    /// Api Key
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// 音色ID
    /// </summary>
    public string? ReferenceId { get; set; }
}
