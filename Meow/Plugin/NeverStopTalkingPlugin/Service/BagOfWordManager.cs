using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Eventing.Reader;
using System.Security.Cryptography.X509Certificates;
using AngleSharp.Dom;
using Lagrange.Core.Message;
using Masuit.Tools;
using Masuit.Tools.Security;
using Meow.Core.Model.Base;
using Meow.Plugin.NeverStopTalkingPlugin.Models;

namespace Meow.Plugin.NeverStopTalkingPlugin.Service;

/// <summary>
/// 词袋管理器
/// </summary>
public class BagOfWordManager : HostDatabaseSupport
{
    /// <inheritdoc />
    public BagOfWordManager(Core.Meow host) : base(host)
    {
        var vectorCollection = GetCollection<BagOfWordVector>(NstBagOfWordVectorCollection);
        vectorCollection.EnsureIndex(x => x.MsgMd5);
        vectorCollection.EnsureIndex(x => x.Uin);
        vectorCollection.EnsureIndex(x => x.BagOfWordType);

        var bowCollection = GetCollection<BagOfWordRecord>(NstBagOfWordManagerCollection);
        bowCollection.EnsureIndex(x => x.Uin);
        bowCollection.EnsureIndex(x => x.IsFull);
        bowCollection.EnsureIndex(x => x.DbId);
        bowCollection.EnsureIndex(x => x.HasDelete);
        bowCollection.EnsureIndex(x => x.BagOfWordType);

        GetFillBagOfWordId();
    }

    #region Properties

    /// <summary>
    /// 词袋集合
    /// </summary>
    private const string NstBagOfWordManagerCollection = $"{nameof(NstBagOfWordManagerCollection)}";

    /// <summary>
    /// 词袋计算出来的词向量集合
    /// </summary>
    private const string NstBagOfWordVectorCollection = $"{nameof(NstBagOfWordVectorCollection)}";

    /// <summary>
    /// 群词袋最大数量
    /// </summary>
    private const int GroupBowMaxCount = 5000;

    /// <summary>
    /// 个人词袋最大数量
    /// </summary>
    private const int PersonalBowMaxCount = 5000;

    /// <summary>
    /// 已经装满的词袋
    /// <br/> 词袋id
    /// <br/> 词袋类型
    /// <br/> 词袋uin
    /// </summary>
    private HashSet<(BagOfWordType type, uint uin)> FullWordBagInfo { get; } = [];

    #endregion

    #region Methods

    /// <summary>
    /// 获取已经装满的词袋id
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    private void GetFillBagOfWordId()
    {
        var fullBagOfWordDbId = GetCollection<BagOfWordRecord>(NstBagOfWordManagerCollection)
            .Find(x => x.IsFull && !x.HasDelete)
            .Select(x => (x.BagOfWordType, x.Uin)).ToArray();

        FullWordBagInfo.AddRangeIfNotContains(fullBagOfWordDbId);
    }

    /// <summary>
    /// 查询并返回指定类型和 UIN(群id或个人id)的词袋记录。
    /// </summary>
    /// <param name="wordRecord">输出参数，查询到的词袋记录，如果未找到则为 null。</param>
    /// <param name="type">词袋类型。</param>
    /// <param name="uin">群id或个人id。</param>
    /// <param name="isFull">词袋是否已满的标志，默认为 false。</param>
    /// <param name="hasDelete">词袋是否已被删除的标志，默认为 false。</param>
    /// <returns>如果找到符合条件的记录，则返回 true；否则返回 false。</returns>
    private bool QueryBagOfWordRecord([MaybeNullWhen(false)] out BagOfWordRecord wordRecord, BagOfWordType type,
        uint uin, bool isFull = false, bool hasDelete = false)
    {
        wordRecord = GetCollection<BagOfWordRecord>(NstBagOfWordManagerCollection)
            .FindOne(x => x.BagOfWordType == type && x.Uin == uin
                                                  && x.IsFull == isFull && x.HasDelete == hasDelete);
        return wordRecord is not null;
    }

    /// <summary>
    /// 创建新词袋
    /// </summary>
    /// <param name="uin">群号或者qq</param>
    /// <param name="bagOfWordType">词袋类型</param>
    /// <returns>
    /// <br/> true: 成功创建
    /// <br/> false: 创建失败 <see cref="message"/>会提示错误信息
    /// </returns>
    public bool CreateBagOfWord(uint uin, BagOfWordType bagOfWordType, out string message)
    {
        message = string.Empty;

        var queryable = Query<BagOfWordRecord>(NstBagOfWordManagerCollection)
            .Where(x => x.BagOfWordType == bagOfWordType)
            .Where(x => x.Uin == uin);
        // 全局词袋只能有一个 并且uin为0
        if (bagOfWordType is BagOfWordType.Global)
        {
            queryable = Query<BagOfWordRecord>(NstBagOfWordManagerCollection)
                .Where(x => x.BagOfWordType == BagOfWordType.Global)
                .Where(x => x.Uin == 0);
        }

        if (queryable.FirstOrDefault() != null)
        {
            message = $"[词袋已经存在, 无法创建] uin: {uin}, 词袋类型:{bagOfWordType}";
            Host.Error(message);
            return false;
        }

        var bagOfWordRecord = new BagOfWordRecord(bagOfWordType is BagOfWordType.Global or BagOfWordType.Group
                ? GroupBowMaxCount
                : PersonalBowMaxCount,
            bagOfWordType is BagOfWordType.Global ? 0 : uin,
            bagOfWordType);

        Insert(bagOfWordRecord, NstBagOfWordManagerCollection);
        return true;
    }

    /// <summary>
    /// 查询词袋
    /// </summary>
    /// <param name="uin">群号或者qq</param>
    /// <param name="bagOfWordType">词袋类型</param>
    /// <param name="result">查询结果文本</param>
    /// <returns>
    /// <br/> true: 查询成功
    /// <br/> false: 查询失败
    /// </returns>
    public void QueryBagOfWordCommand(uint uin,
        BagOfWordType bagOfWordType,
        out string result)
    {
        result = "查询词袋失败!";
        if (!QueryBagOfWordRecord(out var bagOfWordRecord, bagOfWordType, uin))
        {
            return;
        }

        result =
            $"""
             词袋Uin: {bagOfWordRecord.Uin}
             词袋类型:{bagOfWordRecord.BagOfWordType}
             容量:{bagOfWordRecord.MaxCount}
             当前词袋填充状态:{bagOfWordRecord.BagOfWord.Count}/{bagOfWordRecord.MaxCount}";
             """;
    }

    /// <summary>
    /// 删除词袋
    /// </summary>
    /// <param name="uin">群号或者qq</param>
    /// <param name="bagOfWordType">词袋类型</param>
    /// <returns>
    /// <br/> true: 成功删除
    /// <br/> false: 删除失败
    /// </returns>
    public bool RemoveBagOfWord(uint uin, BagOfWordType bagOfWordType)
    {
        if (bagOfWordType is BagOfWordType.Global)
        {
            return false;
        }

        var bagOfWordRecord = Query<BagOfWordRecord>(NstBagOfWordManagerCollection)
            .Where(x => x.BagOfWordType == bagOfWordType)
            .Where(x => x.Uin == uin)
            .FirstOrDefault();
        if (bagOfWordRecord == null)
        {
            return false;
        }

        bagOfWordRecord.HasDelete = true;
        Update(bagOfWordRecord, NstBagOfWordManagerCollection);
        return true;
    }

    /// <summary>
    /// 处理分词结果, 将分词结果尝试填充到词袋中, 如果词袋已经充满 则根据词袋计算消息向量
    /// </summary>
    /// <param name="messageChain">源消息链</param>
    /// <param name="textMessage">消息链中的纯文本数据</param>
    /// <param name="filterResult">分词后经过过滤的结果</param>
    /// <returns>包含了消息是否经过向量计算, 向量计算数据的处理结果</returns>
    public MsgRecord ProcessCutResult(MessageChain messageChain,
        string textMessage,
        string[] filterResult)
    {
        uint groupId = 0;
        var senderId = messageChain.FriendUin;
        if (messageChain.Type is MessageChain.MessageType.Group)
        {
            groupId = messageChain.GroupUin ?? throw new Exception("处理消息失败, 消息链是群消息类型 但却没有包含群id");
        }

        var msgRecord = new MsgRecord(textMessage, senderId, groupId);
        // 尝试填充词袋
        TryFillBagWord(filterResult, senderId, groupId);
        // 计算所有对应词袋的词向量
        return msgRecord;
    }

    /// <summary>
    /// 尝试计算该消息对应词袋的词向量(如果有对应词袋的话)
    /// </summary>
    /// <param name="msgRecord">消息详情</param>
    /// <param name="filterResult">分词后的筛选结果</param>
    public List<BagOfWordVector> GetMsgVectors(MsgRecord msgRecord, string[] filterResult)
    {
        var bagOfWordVectors = new List<BagOfWordVector>();
        var messageMd5 = string.Join(",", filterResult).MDString();

        if (msgRecord.DbId < 1)
        {
            return bagOfWordVectors;
        }

        if (msgRecord.IsGroupMsg)
        {
            // 全局词袋只收集这两个群
            var validGroupIds = new uint[] {726070631, 587914615};
            if (validGroupIds.Contains(msgRecord.GroupId))
            {
                AddBagOfWordVector(BagOfWordType.Global, msgRecord.DbId, 0, messageMd5, bagOfWordVectors);
            }
        }

        AddBagOfWordVector(BagOfWordType.Personal, msgRecord.DbId, msgRecord.Sender, messageMd5, bagOfWordVectors);
        AddBagOfWordVector(BagOfWordType.Group, msgRecord.DbId, msgRecord.GroupId, messageMd5, bagOfWordVectors);

        if (bagOfWordVectors.Count > 0)
        {
            InsertCollection(bagOfWordVectors, NstBagOfWordVectorCollection);
        }

        return bagOfWordVectors;

        // local method, 用于计算不同词袋的向量
        void AddBagOfWordVector(BagOfWordType type, int msgId, uint bowUin, string md5, List<BagOfWordVector> result)
        {
            // 所需的词袋没有装满就退出
            if (!FullWordBagInfo.Contains((type, bowUin)))
            {
                return;
            }

            // 被计算过就不在计算
            if (GetCollection<BagOfWordVector>(NstBagOfWordVectorCollection)
                .Exists(x => x.BagOfWordType == type && x.Uin == bowUin && x.MsgMd5 == md5))
            {
                msgRecord.HaveVector = true;
                return;
            }

            if (!QueryBagOfWordRecord(out var bow, type, bowUin, true))
            {
                return;
            }

            var calculatedVector = CalculateWordVector(filterResult, bow);
            msgRecord.HaveVector = true;
            result.Add(new BagOfWordVector(bow.DbId, type, bowUin, msgId, md5, calculatedVector.MaxCount,
                calculatedVector.VectorElementIndex));
        }
    }

    /// <summary>
    /// 计算词向量
    /// </summary>
    /// <param name="words">需要计算词向量的单词列表</param>
    /// <param name="bagOfWordRecord">词袋记录</param>
    /// <returns>计算结果 MaxCount List[(index, count)]代表向量中有值位置的索引与技术></returns>
    /// <exception cref="ArgumentNullException">当词袋记录或单词列表为空时抛出异常</exception>
    /// <exception cref="InvalidOperationException">当词袋没有填满时抛出异常</exception>
    private (int MaxCount, VectorElementIndex[] VectorElementIndex) CalculateWordVector(string[] words,
        BagOfWordRecord bagOfWordRecord)
    {
        if (bagOfWordRecord == null)
        {
            throw new ArgumentNullException(nameof(bagOfWordRecord), "词袋记录不能为空");
        }

        if (words == null)
        {
            throw new ArgumentNullException(nameof(words), "单词列表不能为空");
        }

        if (!bagOfWordRecord.IsFull)
        {
            throw new InvalidOperationException("无法使用没有填满的词袋进行词向量计算");
        }

        var indexCount = new Dictionary<int, int>();
        foreach (var word in words)
        {
            if (!bagOfWordRecord.BagOfWord.TryGetValue(word, out var index))
            {
                continue;
            }

            // 如果不存在添加一条计数, 如过存在 计数+1
            if (!indexCount.TryAdd(index, 1))
            {
                indexCount[index]++;
            }
        }

        return (bagOfWordRecord.MaxCount, indexCount.Select((x, _) => new VectorElementIndex(x.Key, x.Value)).ToArray());
    }

    /// <summary>
    /// 尝试将词列表添加到不同类型的词袋中。
    /// </summary>
    /// <param name="words">需要添加到词袋中的词列表。</param>
    /// <param name="senderId">发送人Id 如果存在个人词袋则会尝试添加到个人词袋</param>
    /// <param name="groupId">可选的群组ID，如果存在则尝试添加到群词袋</param>
    private void TryFillBagWord(string[] words, uint senderId, uint groupId)
    {
        const uint GlobalGroup1 = 726070631;
        const uint GlobalGroup2 = 587914615;
        var validGlobalGroups = new[] {GlobalGroup1, GlobalGroup2};

        var changeList = new List<BagOfWordRecord>();

        // 只添加特定群的语料到全局词袋
        if (validGlobalGroups.Contains(groupId))
        {
            AddWordsToBag(words, BagOfWordType.Global, 0, changeList);
        }

        // 添加个人和群词袋
        AddWordsToBag(words, BagOfWordType.Personal, senderId, changeList);
        AddWordsToBag(words, BagOfWordType.Group, groupId, changeList);

        if (changeList.Count > 0)
        {
            Host.Info(
                $"需要更新的词袋：{string.Join(", ", changeList.Select(x => $"[{x.BagOfWordType} {x.Uin} {x.BagOfWord.Count}]"))}");
            UpdateCollection(changeList, NstBagOfWordManagerCollection);
        }
    }

    /// <summary>
    /// 将指定的单词数组添加到词袋中。
    /// </summary>
    /// <param name="words">要添加的单词数组。</param>
    /// <param name="bagOfWordType">词袋的类型。</param>
    /// <param name="uin">词袋对应的唯一标识符。</param>
    /// <param name="changeList">用于记录已更改的词袋记录的列表。</param>
    private void AddWordsToBag(string[] words, BagOfWordType bagOfWordType, uint uin, List<BagOfWordRecord> changeList)
    {
        // 词袋装满就直接返回
        if (FullWordBagInfo.Contains((bagOfWordType, uin)))
        {
            return;
        }

        if (!QueryBagOfWordRecord(out var bagOfWordRecord, bagOfWordType, uin))
        {
            return;
        }

        if (bagOfWordRecord.TryAddBagOfWord(words) <= 0)
        {
            return;
        }

        changeList.Add(bagOfWordRecord);
        if (bagOfWordRecord.IsFull)
        {
            FullWordBagInfo.Add((bagOfWordType, uin));
        }
    }

    /// <summary>
    /// 根据词袋 ID 对消息向量进行分页查询和相似度计算。
    /// </summary>
    /// <param name="bagOfWordId">词袋的数据库 ID。</param>
    /// <param name="pageData">
    /// 一个委托方法，用于处理每页的数据，并返回一个包含相似度和消息 ID 的元组列表。
    /// </param>
    /// <returns>一个包含相似度和消息 ID 的元组列表。</returns>
    /// <remarks>
    /// 该方法分页查询指定词袋 ID 的消息向量，每页返回指定数量的消息向量，并通过委托方法进行相似度计算。
    /// 委托方法 `pageData` 应该接受一个包含消息向量的列表，并返回一个包含相似度和消息 ID 的元组列表。
    /// </remarks>
    public List<(double similarity, int msgId)> PaginationQueryCalculation(int bagOfWordId,
        Func<List<BagOfWordVector>, List<(double similarity, int msgId)>> pageData)
    {
        var currentPage = 1; // 当前页码
        const int pageSize = 1000; // 每页的数据量
        List<BagOfWordVector>? page = null;
        var result = new List<(double, int)>();
        do
        {
            var skip = (currentPage - 1) * pageSize;
            page = GetCollection<BagOfWordVector>(NstBagOfWordVectorCollection).Find(x => x.BagOfWordId == bagOfWordId)
                .Skip(skip).ToList();
            if (page.Count == 0)
            {
                continue;
            }

            var data = pageData(page);
            result.AddRange(data);
            currentPage++;
        } while (page.Count != 0);

        return result;
    }

    #endregion
}