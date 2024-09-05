namespace Meow.Plugin.NeverStopTalkingPlugin.Models;

/// <summary>
/// 复读触发记录
/// </summary>
/// <param name="BagOfWordType">词袋类型，指示该记录所属的词袋类别</param>
/// <param name="Uin">群|用户 标识号</param>
/// <param name="TriggerFailedCount">触发失败的次数, 达到触发条件 但是没有取到消息or其他需要触发但没触发的情况</param>
/// <param name="LastTriggered">上次触发的时间</param>
public record NstTriggerRecord(BagOfWordType BagOfWordType, uint Uin, int TriggerFailedCount, DateTime? LastTriggered)
{
    /// <summary>触发失败的次数, 达到触发条件 但是没有取到消息or其他需要触发但没触发的情况</summary>
    public int TriggerFailedCount { get; set; } = TriggerFailedCount;

    /// <summary>上次触发的时间</summary>
    public DateTime? LastTriggered { get; set; } = LastTriggered;
}