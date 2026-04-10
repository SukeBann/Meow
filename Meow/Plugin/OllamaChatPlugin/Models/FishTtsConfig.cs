namespace Meow.Plugin.OllamaChatPlugin.Models;

public class FishTtsConfig
{
    /// <summary>
    /// API Key 文件路径
    /// </summary>
    public string ApiKeyFilePath { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PluginResource", "OllamaChatPlugin",
        "fish_api_key.txt");
    
    /// <summary>
    /// Reference ID 文件路径（用于指定音色）
    /// </summary>
    public string ReferenceIdFilePath { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PluginResource", "OllamaChatPlugin",
        "fish_reference_id.txt");
    
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
}
