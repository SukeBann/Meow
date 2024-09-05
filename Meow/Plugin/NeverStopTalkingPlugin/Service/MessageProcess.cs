using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using JiebaNet.Segmenter;
using Lagrange.Core.Message;
using Lagrange.Core.Message.Entity;
using Masuit.Tools;
using Meow.Core.Model.Base;
using Meow.Plugin.NeverStopTalkingPlugin.Models;
using Meow.Utils;

namespace Meow.Plugin.NeverStopTalkingPlugin.Service;

        /// <summary>
        /// 消息处理, 接收插件处理到的消息
        /// <br/> 先查询该消息的文本是否被处理过(数据库md5对比), 如果处理过就不再响应（防复读机）,
        /// <br/> 先进行屏蔽词查询, 如果有屏蔽词则不处理该消息
        /// <br/> 将消息分词, 从中筛取掉所有停用词, 判断一个合适的大小， 比如分词结果要大于两个以上才存储
        /// <br/>
        /// <br/> 消息处理器会把分词结果与消息源放入词袋处理器中, 处理器会根据词袋类型处理消息, 并返回消息向量
        /// <br/> 词袋会有两种类型 一种为群聊词袋, 容量相对来说较大, 收集的是所有人的消息
        /// <br/> 另外一种是个人词袋, 容量较小, 收集的是个人的消息, 对于群聊词袋来说 更能体现个人说话风格, 但是如果聊天消息过少， 可能词袋得花很久才会生成
        /// <br/>
        /// <br/> 词袋处理器会返回一个结果, 如果词袋未被填充完成 则该消息只会被存储, 并且用于扩充词袋 不会被响应
        /// <br/> 否则词袋会返回一条消息向量, 消息处理器可以使用消息向量去查找最相似的消息
        /// </summary>
public class MessageProcess : HostDatabaseSupport
{
    /// <inheritdoc />
    public MessageProcess(BagOfWordManager bagOfWordManager, TextCutter textCutter, 
        Core.Meow host) : base(host)
    {
        BagOfWordManager = bagOfWordManager;
        BagOfWordManager.BoWBusyStateChange.Subscribe(isBusy => IsBowManagerBusy = isBusy);
        TextCutter = textCutter;

        var msgCollection = GetCollection<MsgRecord>(CollStr.NstMessageProcessMsgRecordCollection);
        msgCollection.EnsureIndex(x => x.DbId);
        msgCollection.EnsureIndex(x => x.HaveVector);
        msgCollection.EnsureIndex(x => x.HasDelete);

        ComputeMessageVector();
    }

    #region properties
    
    /// <summary>
    /// 词袋管理器是否繁忙
    /// </summary>
    private bool IsBowManagerBusy { get; set; }
    
    /// <summary>
    /// 词袋管理器
    /// </summary>
    private BagOfWordManager BagOfWordManager { get; set; }

    /// <summary>
    /// 分词器
    /// </summary>
    private TextCutter TextCutter { get; set; }
    
    /// <summary>
    /// 触发几率千分数 如果这个值是18 那么触发几率就是 18/1000
    /// </summary>
    private int TriggerProbabilityPerThousand { get; set; } = 100;

    /// <summary>
    /// 触发几率随机数获取器
    /// </summary>
    private readonly Random _triggerRandom = new();
    
    /// <summary>
    /// 触发记录，包含与消息分词和触发条件相关的所有记录
    /// </summary>
    private List<NstTriggerRecord> TriggerRecords { get; set; } = new();

    #endregion

    /// <summary>
    /// 处理消息链
    /// </summary>
    /// <param name="messageChain">消息链</param>
    /// <returns></returns>
    public (bool isSendBack, MessageChain messageChain) ProcessMessage(MessageChain messageChain)
    {
        if (IsBowManagerBusy)
        {
            return (false, messageChain);
        }
        
        // 不处理自身消息, 获取textMessage为空 filterResult为空的清空下 就退出
        if (TextCutter.GetTextMsg(messageChain, out var textMessage, out var filterResult))
        {
            return (false, messageChain);
        }

        var msgRecord = BagOfWordManager.ProcessCutResult(messageChain, textMessage, filterResult);
        // 计算完消息向量, 先保存
        Insert(msgRecord, CollStr.NstMessageProcessMsgRecordCollection);
        var bagOfWordVector = BagOfWordManager.GetMsgVectors(msgRecord, filterResult);

        if (bagOfWordVector.Count < 1)
        {
            return (false, messageChain);
        }

        if (!RepeaterTrigger(bagOfWordVector, msgRecord, out var result))
        {
            return (false, messageChain);
        }

        var textMsg = result.repeaterMsg!.TextMsg;
        var message = $"\n源消息: {textMessage}\n相似消息: {textMsg}\n相似度: {result.similarity}";
        Host.Info(message);
        // 限制发哪几个群
        return messageChain.GroupUin is 749396837 or 726070631 or 587914615 or 942033342
            ? (true, messageChain.CreateSameTypeTextMessage(textMsg))
            : (false, messageChain.CreateSameTypeTextMessage(textMsg));
    }
    
    /// <summary>
    /// 根据设置的触发几率决定该次是否触发
    /// 触发机制有软保底, 比如某次成功达成了触发机制,
    /// 但是缺没有找到相似消息, 那下一次就会强制触发, 直到触发了一次为止
    /// </summary>
    /// <param name="allVector">要做匹配的向量集</param>
    /// <param name="msgRecord">当前消息</param>
    /// <param name="result">触发复读时找到的相似详细</param>
    /// <returns></returns>
    private bool RepeaterTrigger(List<BagOfWordVector> allVector, MsgRecord msgRecord,  out (double similarity, MsgRecord? repeaterMsg) result)
    {
        result = (0, null);
        
        if (!TryToTrigger(msgRecord, out var nstTriggerRecord))
        {
            return false;
        }

        const double threshold = 0.75d;
        var waitingList = FindMostSimilarMsg(allVector)
            .Where(x => x.msgId != msgRecord.DbId && x.similarity > threshold)
            .Take(10)
            .ToList();

        // 查找列表为空时退出, 设置当前触发对象的触发失败次数+1
        var count = waitingList.Count;
        if (count == 0)
        {
            nstTriggerRecord.TriggerFailedCount++;
            return false;
        }

        // 多条随机取一条
        var index = new Random().Next(0, count - 1);
        var (similarity, msgId) = waitingList[index];
        var firstMsg = GetCollection<MsgRecord>(CollStr.NstMessageProcessMsgRecordCollection).FindOne(x => x.DbId == msgId);
        if (firstMsg is null)
        {
            nstTriggerRecord.TriggerFailedCount++;
            return false;
        }

        result = (similarity, firstMsg);
        nstTriggerRecord.TriggerFailedCount = 0;
        nstTriggerRecord.LastTriggered = DateTime.Now;
        return true;
    }

    /// <summary>
    /// 尝试触发操作。
    /// </summary>
    /// <param name="msgRecord">消息记录对象。</param>
    /// <param name="nstTriggerRecord">触发记录输出参数。</param>
    /// <returns>如果触发成功，返回 true；否则返回 false。</returns>
    /// <remarks>
    /// 该方法根据消息记录和一定的几率（以及强制触发条件）决定是否触发某一操作。
    /// </remarks>
    private bool TryToTrigger(MsgRecord msgRecord, [MaybeNullWhen(false)]out NstTriggerRecord nstTriggerRecord)
    {
        // 初始化'强制触发'标志，默认为false
        var isForceTrigger = false;

        // 生成一个介于0和1000之间的随机数
        var randomNum = _triggerRandom.Next(0, 1000);

        // 根据消息类型设置词袋类型
        var bagOfWordType = msgRecord.IsGroupMsg ? BagOfWordType.Group : BagOfWordType.Personal;

        // 根据消息类型获取唯一标识符（群ID或发送者ID）
        var uin = msgRecord.IsGroupMsg ? msgRecord.GroupId : msgRecord.Sender;

        // 查找与当前消息对应的触发记录
        nstTriggerRecord = TriggerRecords.FirstOrDefault(x => x.Uin == uin && x.BagOfWordType == bagOfWordType);

        // 如果没有找到对应的触发记录，则创建一个新的记录并添加到列表中
        if (nstTriggerRecord is null)
        {
            nstTriggerRecord = new NstTriggerRecord(bagOfWordType, uin, 0, default);
            TriggerRecords.Add(nstTriggerRecord);
        }
        else
        {
            // 如果触发失败次数大于等于3次，则强制触发
            if (nstTriggerRecord.TriggerFailedCount >= 3)
            {
                Host.Info($"{nstTriggerRecord.Uin}-{nstTriggerRecord.TriggerFailedCount}: isForceTrigger: true");
                isForceTrigger = true;
            }
        }

        // 如果不是强制触发，且随机数大于触发概率阈值，则返回false，表示不触发
        if (!isForceTrigger && randomNum > TriggerProbabilityPerThousand)
        {
            Host.Info($"return: {randomNum}, {nstTriggerRecord.TriggerFailedCount}");
            return false;
        }

        // 返回true，表示触发
        return true;
    }

    /// <summary>
    /// 从所有已经计算过向量的消息中寻找最相似的几条
    /// </summary>
    /// <param name="bagOfWordVectors">需要对比哪些向量</param>
    private List<(double similarity, int msgId)> FindMostSimilarMsg(List<BagOfWordVector> bagOfWordVectors)
    {
        var totalResult = new List<(double similarity, int msgId)>();
        foreach (var bagOfWordVector in bagOfWordVectors)
        {
            var wordVectorCalculate = new WordVectorCalculate();
            var result = BagOfWordManager.PaginationQueryCalculation(bagOfWordVector.BagOfWordId,
                page => wordVectorCalculate.GetSimilarString(page,
                    bagOfWordVector));
            totalResult.AddRange(result);
        }

        // 去重
        return totalResult
            .GroupBy(item => item.msgId)
            .Select(group => group.OrderByDescending(item => item.similarity).First())
            .ToList();
    }

    /// <summary>
    /// 计算消息向量
    /// <br/> 查询未删除的消息记录
    /// <br/> 对每个消息记录，使用词袋管理器获取消息向量, 并存储进向量集合里面
    /// </summary>
    private void ComputeMessageVector()
    {
        foreach (var msgRecord in GetCollection<MsgRecord>(CollStr.NstMessageProcessMsgRecordCollection)
                     .Find(x => !x.HasDelete && !x.HaveVector))
        {
            if (TextCutter.CutPlainText(msgRecord.TextMsg, out var filterResult))
            {
                continue;
            }

            BagOfWordManager.GetMsgVectors(msgRecord, filterResult);
            // 如果消息被计算了 就update
            if (msgRecord.HaveVector)
            {
                Update(msgRecord, CollStr.NstMessageProcessMsgRecordCollection);
                Host.Info("Updated message vector.");
            }
        }
    }
}