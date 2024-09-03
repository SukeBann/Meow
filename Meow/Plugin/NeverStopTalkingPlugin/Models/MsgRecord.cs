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
    public MsgRecord(string textMsg, uint sender, uint groupId = 0)
    {
        TextMsg = textMsg;
        Sender = sender;
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
    /// 发送者
    /// </summary>
    public uint Sender { get; set; }
    
    /// <summary>
    /// 群id
    /// </summary>
    public uint GroupId { get; set; }
    
    /// <summary>
    /// 是否被计算过消息向量, 每条消息只计算一次
    /// </summary>
    public bool HaveVector { get; set; }
    
    /// <summary>
    /// 是否为群消息
    /// </summary>
    public bool IsGroupMsg => GroupId != 0;
}