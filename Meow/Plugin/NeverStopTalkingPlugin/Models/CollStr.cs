namespace Meow.Plugin.NeverStopTalkingPlugin.Models;

/// <summary>
/// 数据库集合字符串
/// </summary>
public static class CollStr
{
    /// <summary>
    /// 词袋计算出来的词向量集合
    /// </summary>
    public const string NstBagOfWordVectorCollection = $"{nameof(NstBagOfWordVectorCollection)}";

    /// <summary>
    /// 词袋集合
    /// </summary>
    public const string NstBagOfWordManagerCollection = $"{nameof(NstBagOfWordManagerCollection)}";

    /// <summary>
    /// 违禁词数据库集合
    /// </summary>
    public const string NstForbiddenWordsManagerCollection = nameof(NstForbiddenWordsManagerCollection);

    /// <summary>
    /// 消息记录存储数据库集合名称
    /// </summary>
    public const string NstMessageProcessMsgRecordCollection = nameof(NstMessageProcessMsgRecordCollection);
}