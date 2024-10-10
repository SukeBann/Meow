using Meow.Core;
using Meow.Core.Model.Base;
using Meow.Plugin.NeverStopTalkingPlugin.Command;
using Meow.Plugin.NeverStopTalkingPlugin.Models;
using Meow.Plugin.NeverStopTalkingPlugin.Service;

namespace Meow.Plugin.NeverStopTalkingPlugin;

/* TODO 可以考虑给这个插件单独弄个Readme
 * 这个插件提供功能让bot记录所有收到信息, 每次收到消息都会把消息分词, 并记录到数据库中
 * 分词结果会筛选掉其中的停用词, 剩下的词会被添加到一个固定大小的词袋中
 * 当词袋被填充到指定的大小时, 就可以启动词袋模型计算了
 * 之后收到的消息以及之前存储的消息会被拿去做向量计算, 生成长度与词袋相等的向量值
 * 计算完的消息会存储好向量值与词袋hashCode 与对应的词袋映射计算关系, 因为只有相同词袋计算出来的向量才可以拿来做比较
 * 程序中会读取定量的最新数据到程序中, 如果有新的消息被计算了向量值存储到数据库,
 * 则它也会被加入缓存, 同时移除掉最老的数据, 是的, 这就是个队列
 *
 * 当接收到新消息, 并且成功达成了触发条件时, 消息会被拿去做分词, 然后根据词袋做向量计算
 * 然后拿这个向量值与缓存队列中的数据求解余弦相似度, 最终得到一个0-1的数字 越接近1代表两个句子越相似
 * 默认的规则是设置一个相似度阈值 会查找所有数据并取出相似度最高的那一个,
 * 还有其他规则
 * 1. 取出大于阈值的第一条消息
 * 2. 取出阈值区间内最大的一条,
 * 3. 无视比较规则开始胡说八道 随机从队列中取一条
 * 4.
 *
 * 最后会发生什么事情你肯定也知道了, 她会大声的把这个消息说出去
 */

/// <summary>
/// 当启用这个插件之后 bot会变成非常想说话的Bot.
/// <br/>一秒钟不说都难受, 她只是想说话 她什么都没做错
/// </summary>
public class NeverStopTalkingPlugin : PluginBase
{
    /// <inheritdoc />
    public override string PluginName => "Never stop talking";

    /// <inheritdoc />
    public override string PluginDescription => "非常想说话的Bot, 一秒钟不说都难受, 她只是想说话 她什么都没做错";

    /// <inheritdoc />
    public override string PluginUid => "8E1F9C63-CB6C-4232-B617-09DAA73F8910";

    /// <summary>
    /// 消息处理器
    /// </summary>
    private MessageProcess? MessageProcess { get; set; }

    /// <inheritdoc />
    public override List<IMeowCommand> Commands { get; } = new(){};

    private IDisposable? MessageProcessDisposable { get; set; }

    /// <inheritdoc />
    public override void InjectPlugin(Core.Meow host)
    {
        var forbiddenWordsManager = new ForbiddenWordsManager(host);
        var textCutter = new TextCutter(host, forbiddenWordsManager);
        var nstBagOfWordManager = new BagOfWordManager(host, textCutter);

        Commands.Add(new NstDontSayThatCommand(forbiddenWordsManager));
        Commands.Add(new BagOfWordCommand(host, nstBagOfWordManager));
        
        MessageProcess = new MessageProcess(nstBagOfWordManager, textCutter, host);
        MessageProcessDisposable = host.OnMessageReceived.Subscribe(x =>
        {
            MessageProcess.EnqueueMessage(x.messageChain);
        });
        
        base.InjectPlugin(host);
    }

    /// <inheritdoc />
    public override void Remove()
    {
        MessageProcessDisposable?.Dispose();
        base.Remove();
    }
}