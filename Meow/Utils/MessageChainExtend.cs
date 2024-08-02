using Lagrange.Core.Message;

namespace Meow.Utils;

public static class MessageChainExtend
{
    /// <summary>
    /// 创建一个消息链类型相同的空消息链构造器, 消息目标与源消息发送者相同
    /// </summary>
    /// <param name="messageChain">源消息链</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /// <returns></returns>
    public static MessageBuilder CreateSameTypeMessageBuilder(this MessageChain messageChain)
    {
        return messageChain.Type switch
        {
            MessageChain.MessageType.Group => MessageBuilder.Group(messageChain.GroupUin ?? 0),
            MessageChain.MessageType.Temp or MessageChain.MessageType.Friend => MessageBuilder.Friend(messageChain
                .FriendUin),
            _ => throw new ArgumentOutOfRangeException("无法识别的消息链类型, 无法构建消息链")
        };
    }
}