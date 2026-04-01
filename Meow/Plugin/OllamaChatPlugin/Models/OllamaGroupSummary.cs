using FreeSql.DataAnnotations;
using Masuit.Tools;
using Meow.Core.Model.Base;
using Newtonsoft.Json;

namespace Meow.Plugin.OllamaChatPlugin.Models;

public class OllamaGroupSummary : DatabaseRecordBase
{
    /// <summary>
    /// 群号或私聊QQ号
    /// </summary>
    public long Uin { get; set; }

    /// <summary>
    /// 总结结果 (JSON 序列化存储)
    /// </summary>
    [Column(StringLength = -1)]
    public string SummaryJson
    {
        get => JsonConvert.SerializeObject(Summary);
        set => Summary = value.IsNullOrEmpty() ? new() : JsonConvert.DeserializeObject<OllamaSummaryResponse>(value) ?? new();
    }

    [Column(IsIgnore = true)]
    public OllamaSummaryResponse Summary { get; set; } = new();
}
