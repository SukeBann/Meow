using FreeSql.DataAnnotations;
using Masuit.Tools;
using Meow.Core.Model.Base;
using Meow.Plugin.OllamaChatPlugin.Service;
using Newtonsoft.Json;

namespace Meow.Plugin.OllamaChatPlugin.Models;

public class OllamaChatContext : DatabaseRecordBase
{
    /// <summary>
    /// 群号或私聊QQ号
    /// </summary>
    public long Uin { get; set; }

    /// <summary>
    /// 对话历史 (JSON 序列化存储)
    /// </summary>
    [Column(StringLength = -1)]
    public string HistoryJson
    {
        get => JsonConvert.SerializeObject(History);
        set => History = value.IsNullOrEmpty() ? new() : JsonConvert.DeserializeObject<List<OllamaMessage>>(value) ?? new();
    }

    [Column(IsIgnore = true)]
    public List<OllamaMessage> History { get; set; } = new();
}
