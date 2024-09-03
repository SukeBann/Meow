namespace Meow.Plugin.NeverStopTalkingPlugin.Models;

/// <summary>
/// 消息根据词袋计算出的向量
/// </summary>
/// <param name="BagOfWordId">词袋db id</param>
/// <param name="BagOfWordType">词袋类型</param>
/// <param name="Uin">如果是群词袋对应group id， 个人词袋对应 id</param>
/// <param name="MsgId">消息 db id</param>
/// <param name="MsgMd5">计算出的消息的Md5 用于与bagOfWordId一起快速判断该消息是否已经被计算过向量</param>
/// <param name="MaxCount">向量长度</param>
/// <param name="VectorElementIndex">向量中有技术的索引位置 以及命中次数, 用于结合向量长度生成向量, 节约存储和查询成本</param>
public record BagOfWordVector(int BagOfWordId, BagOfWordType BagOfWordType, uint Uin, int MsgId, string MsgMd5, int MaxCount, VectorElementIndex[] VectorElementIndex);

/// <summary>
/// 向量中有技术的索引位置 以及命中次数, 用于结合向量长度生成向量, 节约存储和查询成本
/// </summary>
/// <param name="I">Index简写: 索引</param>
/// <param name="C">Count简写: 命中次数</param>
public record VectorElementIndex(int I, int C);