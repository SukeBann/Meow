namespace Meow.Plugin.NeverStopTalkingPlugin.Models;

public class NstConfig
{
    /// <summary>
    /// 触发几率 (0-1000)
    /// </summary>
    public int TriggerProbability { get; set; } = 600;

    /// <summary>
    /// 触发冷却时间 (秒)
    /// </summary>
    public int CooldownSeconds { get; set; } = 60;

    /// <summary>
    /// 是否可以发言
    /// </summary>
    public bool CanSpeak { get; set; } = true;
}
