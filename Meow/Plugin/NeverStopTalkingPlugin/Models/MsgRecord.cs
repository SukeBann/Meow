using System.Collections.Concurrent;
using System.Reflection.Metadata.Ecma335;
using Lagrange.Core.Message;
using Masuit.Tools.Security;
using Meow.Core.Model.Base;

namespace Meow.Plugin.NeverStopTalkingPlugin.Models;

/// <summary>
/// 消息记录
/// </summary>
public class MsgRecord: DatabaseRecordBase
{
    public MsgRecord(object rawData, string textMsg, List<string> cutResult, uint sender, uint groupId = 0)
    {
        RawData = rawData;
        TextMsg = textMsg;
        Md5 = textMsg.MDString();
        Sender = sender;
        CutResult = cutResult;
        if (groupId != 0)
        {
            GroupId = groupId;
        }
    }

    /// <summary>
    /// 纯文本消息
    /// </summary>
    public string TextMsg { get; set; }
    
    /// <summary>
    /// <see cref="TextMsg"/>的Md5-hash结果
    /// </summary>
    public string Md5 { get; set; }
    
    /// <summary>
    /// 源数据 类型实际上是<see cref="MessageChain"/> LiteDb找不到构造函数, 暂时用object代替
    /// </summary>
    public object RawData { get; set; }
    
    /// <summary>
    /// 发送者
    /// </summary>
    public uint Sender { get; set; }
    
    /// <summary>
    /// 群id
    /// </summary>
    public uint GroupId { get; set; }

    /// <summary>
    /// 是否为群消息
    /// </summary>
    public bool IsGroupMsg => GroupId != 0;

    /// <summary>
    /// 被对应id词袋计算过消息向量， 那个词袋的id会存在这
    /// </summary>
    public List<int> ComputedBagOfWords { get; set; } = [];

    /// <summary>
    /// 分词结果
    /// </summary>
    public List<string> CutResult { get; set; }
}