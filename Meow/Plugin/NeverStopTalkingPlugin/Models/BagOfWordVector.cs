namespace Meow.Plugin.NeverStopTalkingPlugin.Models;

/// <summary>
/// 消息根据词袋计算出的向量
/// </summary>
/// <param name="BagOfWordId">词袋db id</param>
/// <param name="MsgId">消息 db id</param>
/// <param name="msgMd5">计算出的消息的Md5 用于与bagOfWordId一起快速判断该消息是否已经被计算过向量</param>
/// <param name="Vector">计算的向量</param>
public record BagOfWordVector(uint BagOfWordId, int MsgId, string msgMd5, double[] Vector);