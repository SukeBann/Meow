using System.Collections.Concurrent;
using Lagrange.Core.Message;
using Masuit.Tools;
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
        Init();
    }

    #region Properties

    /// <summary>
    /// 数据库集合
    /// </summary>
    private const string NstBagOfWordManagerCollection = $"{nameof(NstBagOfWordManagerCollection)}";

    /// <summary>
    /// 词袋计算出来的词向量
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
    /// 词袋列表
    /// </summary>
    private List<BagOfWordRecord> BagOfWordRecordList { get; set; } = [];

    #endregion

    #region Methods

    /// <summary>
    /// 初始化时查询所有词袋到内存中
    /// </summary>
    private void Init()
    {
        BagOfWordRecordList = Query<BagOfWordRecord>(NstBagOfWordManagerCollection)
            .Where(x => !x.HasDelete).ToList();
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
        BagOfWordRecordList.Add(bagOfWordRecord);
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
    public void QueryBagOfWord(uint uin,
        BagOfWordType bagOfWordType,
        out string result)
    {
        result = "查询词袋失败!";
        var bagOfWordRecord = Query<BagOfWordRecord>(NstBagOfWordManagerCollection)
            .Where(x => x.BagOfWordType == bagOfWordType)
            .Where(x => x.Uin == uin)
            .FirstOrDefault();

        if (bagOfWordRecord is null)
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
        BagOfWordRecordList.RemoveWhere(x => x.DbId == bagOfWordRecord.DbId);
        return true;
    }

    /// <summary>
    /// 处理分词结果, 将分词结果尝试填充到词袋中, 如果词袋已经充满 则根据词袋计算消息向量
    /// </summary>
    /// <param name="messageChain">源消息链</param>
    /// <param name="textMessage">消息链中的纯文本数据</param>
    /// <param name="filterResult">分词后经过过滤的结果</param>
    /// <returns>包含了消息是否经过向量计算, 向量计算数据的处理结果</returns>
    public (MsgRecord msgRecord, List<BagOfWordVector> bagOfWordVectors) ProcessCutResult(MessageChain messageChain,
        string textMessage,
        List<string> filterResult)
    {
        uint groupId = 0;
        var senderId = messageChain.FriendUin;
        if (messageChain.Type is MessageChain.MessageType.Group)
        {
            groupId = messageChain.GroupUin ?? throw new Exception("处理消息失败, 消息链是群消息类型 但却没有包含群id");
        }

        var msgRecord = new MsgRecord(messageChain, textMessage, filterResult, senderId, groupId);
        // 尝试填充词袋
        TryFillBagWord(filterResult, senderId, groupId);
        // 计算所有对应词袋的词向量
        return (msgRecord, GetMsgVectors(msgRecord));
    }

    /// <summary>
    /// 尝试计算该消息对应词袋的词向量(如果有对应词袋的话)
    /// </summary>
    /// <param name="msgRecord">消息详情</param>
    public List<BagOfWordVector> GetMsgVectors(MsgRecord msgRecord)
    {
        var bagOfWordVectors = new List<BagOfWordVector>();
        var filterResult = msgRecord.CutResult;

        if (BagOfWordRecordList.FirstOrDefault(x => x is {BagOfWordType: BagOfWordType.Global, IsFull: true}) is
            { } global)
        {
            if (msgRecord.IsGroupMsg)
            {
                // TODO 全局词袋先只收集特定群
                var valid = new uint[] {726070631, 587914615};
                if (valid.Contains(msgRecord.GroupId))
                {
                    // 找不到相同的消息被相同词袋计算过向量
                    if (!GetCollection<BagOfWordVector>(NstBagOfWordVectorCollection)
                            .Find(x => x.BagOfWordId == 0 && x.msgMd5 == msgRecord.Md5)
                            .Any())
                    {
                        var calculationResult = CalculateWordVector(filterResult, global);
                        bagOfWordVectors.Add(new BagOfWordVector(0, msgRecord.DbId, msgRecord.Md5, calculationResult));
                    }
                }
            }
        }

        if (BagOfWordRecordList.FirstOrDefault(x =>
                x is {BagOfWordType: BagOfWordType.Group, IsFull: true} && x.Uin == msgRecord.GroupId) is
            { } group)
        {
            if (!GetCollection<BagOfWordVector>(NstBagOfWordVectorCollection)
                    .Find(x => x.BagOfWordId == group.DbId && x.msgMd5 == msgRecord.Md5)
                    .Any())
            {
                var calculationResult = CalculateWordVector(filterResult, group);
                bagOfWordVectors.Add(new BagOfWordVector(group.Uin, msgRecord.DbId, msgRecord.Md5, calculationResult));
            }
        }

        if (BagOfWordRecordList.FirstOrDefault(x =>
                x is {BagOfWordType: BagOfWordType.Personal, IsFull: true} && x.Uin == msgRecord.Sender) is
            { } personal)
        {
            if (!GetCollection<BagOfWordVector>(NstBagOfWordVectorCollection)
                    .Find(x => x.BagOfWordId == personal.DbId && x.msgMd5 == msgRecord.Md5)
                    .Any())
            {
                var calculationResult = CalculateWordVector(filterResult, personal);
                bagOfWordVectors.Add(
                    new BagOfWordVector(personal.Uin, msgRecord.DbId, msgRecord.Md5, calculationResult));
            }
        }

        InsertCollection(bagOfWordVectors, NstBagOfWordManagerCollection);

        return bagOfWordVectors;
    }

    /// <summary>
    /// 计算词向量
    /// </summary>
    /// <param name="words">需要计算词向量的单词列表</param>
    /// <param name="bagOfWordRecord">词袋记录</param>
    /// <returns>代表词向量的列表, 目前是byte类型 存储范围0-255</returns>
    /// <exception cref="ArgumentNullException">当词袋记录或单词列表为空时抛出异常</exception>
    /// <exception cref="InvalidOperationException">当词袋没有填满时抛出异常</exception>
    private double[] CalculateWordVector(List<string> words, BagOfWordRecord bagOfWordRecord)
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

        var vector = new double[bagOfWordRecord.MaxCount];
        var wordSet = new HashSet<string>(words); // 使用 HashSet 优化查找速度

        foreach (var (word, index) in bagOfWordRecord.BagOfWord)
        {
            if (wordSet.Contains(word))
            {
                vector[index]++;
            }
        }

        return vector;
    }

    /// <summary>
    /// 尝试将词列表添加到不同类型的词袋中。
    /// </summary>
    /// <param name="filterResult">需要添加到词袋中的词列表。</param>
    /// <param name="groupId">可选的群组ID，如果存在则尝试添加到群词袋</param>
    /// <param name="senderId">发送人Id 如果存在个人词袋则会尝试添加到个人词袋</param>
    private void TryFillBagWord(List<string> filterResult, uint senderId, uint? groupId)
    {
        var changeList = new List<BagOfWordRecord>();

        // 如果全局词袋不为空则往里面添加
        if (BagOfWordRecordList.FirstOrDefault(x => x.BagOfWordType == BagOfWordType.Global)
            is {Uin: 0, IsFull: false} global)
        {
            if (groupId != 0)
            {
                var valid = new uint[] {726070631, 587914615};
                if (valid.Contains(groupId ?? 0))
                {
                    if (global.TryAddBagOfWord(filterResult) > 0)
                    {
                        changeList.Add(global);
                    }
                }
            }
        }

        // 如果存在群词袋则尝试往里面添加
        if (groupId is not null
            && BagOfWordRecordList.FirstOrDefault(x => x.BagOfWordType is BagOfWordType.Group && x.Uin == groupId)
                is { } bagOfWordRecord)
        {
            if (bagOfWordRecord.TryAddBagOfWord(filterResult) > 0)
            {
                changeList.Add(bagOfWordRecord);
            }
        }

        // 如果存在个人袋则尝试往里面添加
        if (BagOfWordRecordList
                .FirstOrDefault(x => x.BagOfWordType is BagOfWordType.Personal && x.Uin == senderId)
            is { } personalRecord)
        {
            if (personalRecord.TryAddBagOfWord(filterResult) > 0)
            {
                changeList.Add(personalRecord);
            }
        }

        if (changeList.Count > 0)
        {
            Host.Info(
                $"需要更新的词袋：{string.Join(", ", changeList.Select(x => $"[{x.BagOfWordType} {x.Uin} {x.BagOfWord.Count}]"))}");
            UpdateCollection(changeList, NstBagOfWordManagerCollection);
        }
    }

    public List<(double similarity, int msgId)> PaginationQueryCalculation(uint bagOfWordId, Func<List<BagOfWordVector>, List<(double similarity, int msgId)>> pageData)
    {
        var currentPage = 1; // 当前页码
        const int pageSize = 1000; // 每页的数据量
        List<BagOfWordVector>? page = null;
        var result = new List<(double, int)>();
        do
        {
            var skip = (currentPage - 1) * pageSize;
            page = GetCollection<BagOfWordVector>(NstBagOfWordVectorCollection).Find(x => x.BagOfWordId == bagOfWordId).Skip(skip).ToList();
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