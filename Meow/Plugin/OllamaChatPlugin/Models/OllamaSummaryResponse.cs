using Newtonsoft.Json;

namespace Meow.Plugin.OllamaChatPlugin.Models;

public class UserPortrait
{
    [JsonProperty("用户名")]
    public string UserName { get; set; } = string.Empty;

    [JsonProperty("稳定风格总结")]
    public string StableStyle { get; set; } = string.Empty;

    [JsonProperty("当前状态总结")]
    public string CurrentState { get; set; } = string.Empty;

    [JsonProperty("常见表达特征")]
    public List<string> ExpressionFeatures { get; set; } = new();

    [JsonProperty("最近代表性消息")]
    public List<string> RepresentativeMessages { get; set; } = new();

    [JsonProperty("样本可信度")]
    public string Confidence { get; set; } = "低";
}

public class InteractionAnalysis
{
    [JsonProperty("成员A")]
    public string MemberA { get; set; } = string.Empty;

    [JsonProperty("成员B")]
    public string MemberB { get; set; } = string.Empty;

    [JsonProperty("互动熟悉度")]
    public int Familiarity { get; set; } = 1;

    [JsonProperty("关系说明")]
    public string Description { get; set; } = string.Empty;
}

public class GroupChatStatus
{
    [JsonProperty("当前话题")]
    public string CurrentTopic { get; set; } = string.Empty;

    [JsonProperty("当前氛围")]
    public string CurrentAtmosphere { get; set; } = string.Empty;

    [JsonProperty("核心发言人")]
    public List<string> CoreSpeakers { get; set; } = new();

    [JsonProperty("适合回复的对象")]
    public List<string> SuitableTarget { get; set; } = new();

    [JsonProperty("机器人此时适合的介入方式")]
    public string InterventionMode { get; set; } = string.Empty;

    [JsonProperty("本轮是否建议发言")]
    public bool SuggestSpeech { get; set; } = false;

    [JsonProperty("发言必要性")]
    public string SpeechNecessity { get; set; } = "低";

    [JsonProperty("建议回复长度")]
    public string SuggestedLength { get; set; } = "短";

    [JsonProperty("最后更新时间")]
    public DateTime LastUpdateTime { get; set; } = DateTime.UtcNow;
}

public class OllamaSummaryResponse
{
    [JsonProperty("群成员画像")]
    public List<UserPortrait> UserPortraits { get; set; } = new();

    [JsonProperty("互动熟悉度分析")]
    public List<InteractionAnalysis> InteractionAnalyses { get; set; } = new();

    [JsonProperty("当前群聊状态")]
    public GroupChatStatus GroupChatStatus { get; set; } = new();
}
