namespace Meow.Plugin.NeverStopTalkingPlugin.Models;

/// <summary>
/// 消息根据词袋计算出的向量
/// </summary>
/// <param name="BagOfWordId">词袋db id</param>
/// <param name="MsgId">消息 db id</param>
/// <param name="Vector">计算的向量</param>
public record BagOfWordVector(int BagOfWordId, int MsgId, List<byte> Vector);