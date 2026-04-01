using FreeSql.DataAnnotations;
using Masuit.Tools;
using Meow.Core.Model.Base;
using Newtonsoft.Json;

namespace Meow.Plugin.NeverStopTalkingPlugin.Models;

/// <summary>
/// 消息根据词袋计算出的向量
/// </summary>
public class BagOfWordVector : DatabaseRecordBase
{
    public BagOfWordVector()
    {
    }

    public BagOfWordVector(long bagOfWordId, BagOfWordType bagOfWordType, long uin, long msgId, string msgMd5,
        int maxCount, VectorElementIndex[] vectorElementIndex)
    {
        BagOfWordId = bagOfWordId;
        BagOfWordType = bagOfWordType;
        Uin = uin;
        MsgId = msgId;
        MsgMd5 = msgMd5;
        MaxCount = maxCount;
        VectorElementIndex = vectorElementIndex;
    }

    /// <summary>
    /// 词袋db id
    /// </summary>
    public long BagOfWordId { get; set; }

    /// <summary>
    /// 词袋类型
    /// </summary>
    public BagOfWordType BagOfWordType { get; set; }

    /// <summary>
    /// 如果是群词袋对应group id， 个人词袋对应 id
    /// </summary>
    public long Uin { get; set; }

    /// <summary>
    /// 消息 db id
    /// </summary>
    public long MsgId { get; set; }

    /// <summary>
    /// 计算出的消息的Md5 用于与bagOfWordId一起快速判断该消息是否已经被计算过向量
    /// </summary>
    public string MsgMd5 { get; set; }

    /// <summary>
    /// 向量长度
    /// </summary>
    public int MaxCount { get; set; }

    /// <summary>
    /// 向量中有技术的索引位置 以及命中次数, 用于结合向量长度生成向量, 节约存储和查询成本
    /// </summary>
    [Column(DbType = "longtext")]
    public string VectorElementIndexJson
    {
        get => JsonConvert.SerializeObject(VectorElementIndex);
        set => VectorElementIndex = value.IsNullOrEmpty() ? Array.Empty<VectorElementIndex>() : JsonConvert.DeserializeObject<VectorElementIndex[]>(value) ?? Array.Empty<VectorElementIndex>();
    }

    [Column(IsIgnore = true)]
    public VectorElementIndex[] VectorElementIndex { get; set; }
}

/// <summary>
/// 向量中有技术的索引位置 以及命中次数, 用于结合向量长度生成向量, 节约存储和查询成本
/// </summary>
/// <param name="I">Index简写: 索引</param>
/// <param name="C">Count简写: 命中次数</param>
public record VectorElementIndex(int I, int C);