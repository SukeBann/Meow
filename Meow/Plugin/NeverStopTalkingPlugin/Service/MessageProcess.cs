using System.Text;
using System.Text.RegularExpressions;
using Castle.Core.Logging;
using JiebaNet.Segmenter;
using Lagrange.Core.Message;
using Lagrange.Core.Message.Entity;
using LiteDB;
using Masuit.Tools;
using Meow.Core.Model.Base;
using Meow.Plugin.NeverStopTalkingPlugin.Models;
using Meow.Utils;
using Serilog;

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
public partial class MessageProcess : HostDatabaseSupport
{
    /// <inheritdoc />
    public MessageProcess(ForbiddenWordsManager forbiddenWordsManager, BagOfWordManager bagOfWordManager,
        Core.Meow host) : base(host)
    {
        ForbiddenWordsManager = forbiddenWordsManager;
        BagOfWordManager = bagOfWordManager;

        ComputeMessageVector();
        StopWord = [];
        _ = LoadStopWord();
    }

    #region Properties

    /// <summary>
    /// 消息记录存储数据库集合名称
    /// </summary>
    private const string NstMessageProcessMsgRecordCollection = nameof(NstMessageProcessMsgRecordCollection);

    /// <summary>
    /// 违禁词管理器
    /// </summary>
    private ForbiddenWordsManager ForbiddenWordsManager { get; set; }

    /// <summary>
    /// 词袋管理器
    /// </summary>
    private BagOfWordManager BagOfWordManager { get; set; }

    /// <summary>
    /// 停用词
    /// </summary>
    private HashSet<string> StopWord { get; set; }

    /// <summary>
    /// 分词器
    /// </summary>
    private JiebaSegmenter WordCutter { get; set; } = new();

    /// <summary>
    /// 匹配纯英文或数字
    /// </summary>
    private Regex MatchEnglishAndNumbers { get; set; } = En_Num_Pattern();

    /// <summary>
    /// 匹配表情
    /// </summary>
    private readonly Regex EmojiPattern = GEmojiPattern();

    [GeneratedRegex(@"^[a-zA-Z0-9]+$")]
    private static partial Regex En_Num_Pattern();

    // ReSharper disable once StringLiteralTypo
    [GeneratedRegex(
        @"[\uD800-\uDBFF][\uDC00-\uDFFF]|[\u2600-\u26FF]|[\u2700-\u27BF]|[\uE000-\uF8FF]|[\u2100-\u214F]|[\u203C\u2049]",
        RegexOptions.Compiled)]
    private static partial Regex GEmojiPattern();

    #endregion

    /// <summary>
    /// 处理消息链
    /// </summary>
    /// <param name="messageChain">消息链</param>
    /// <returns></returns>
    public (bool isSendBack, MessageChain messageChain) ProcessMessage(MessageChain messageChain)
    {
        // 不处理自身消息, 获取textMessage为空 filterResult为空的清空下 就退出
        if (GetTextMsg(messageChain, out var textMessage, out var filterResult))
        {
            return (false, messageChain);
        }

        // 计算完消息向量, 先保存
        var msgRecord = BagOfWordManager.ProcessCutResult(messageChain, textMessage, filterResult);
        Insert(msgRecord.msgRecord, NstMessageProcessMsgRecordCollection);

        const double threshold = 0.5d;
        var waitingList = FindMostSimilarMsg(msgRecord.bagOfWordVectors)
            .Where(x => x.similarity > threshold)
            .Take(5)
            .ToList();

        // 查找列表为空时退出
        var count = waitingList.Count;
        if (count == 0)
        {
            return (false, messageChain);
        }

        // 多条随机取一条
        var index = new Random().Next(0, count - 1);
        var (similarity, msgId) = waitingList[index];
        var firstOrDefault = Query<MsgRecord>(NstMessageProcessMsgRecordCollection).Where(x => x.DbId == msgId)
            .FirstOrDefault();
        if (firstOrDefault is null)
        {
            return (false, messageChain);
        }

        // TODO测试阶段先不发
        var textMsg = firstOrDefault.TextMsg;
        var message = $"源消息: {textMessage}\n相似消息: {textMsg}\n相似度: {similarity}";
        Host.Info(message);
        if (messageChain.GroupUin == 749396837)
        {
            // TODO 测试用
            return (true, messageChain.CreateSameTypeTextMessage(textMsg));
        }

        return (false, messageChain.CreateSameTypeTextMessage(textMsg));
    }

    /// <summary>
    /// 获取文本消息并进行处理。
    /// </summary>
    /// <param name="messageChain">消息链。</param>
    /// <param name="textMessage">输出参数，解析后的文本消息。</param>
    /// <param name="filterResult">输出参数，经过筛选后的结果列表。</param>
    /// <returns>如果消息不需要进一步处理，返回 true；否则返回 false。</returns>
    private bool GetTextMsg(MessageChain messageChain, out string textMessage, out List<string> filterResult)
    {
        filterResult = null;
        textMessage = string.Empty;

        // 不处理自身消息
        if (messageChain.FriendUin == Host.MeowBot.BotUin)
        {
            return true;
        }

        textMessage = ParseTextMessage(messageChain);
        if (textMessage.IsNullOrEmpty())
        {
            return true;
        }

        filterResult = WordCutter.Cut(textMessage)
            .Where(x => !StopWord.Contains(x))
            .Where(x => !ForbiddenWordsManager.CheckForbiddenWordsManager(x))
            .ToList();

        // 移除空格、空字符、只有数字或者英文的字符
        filterResult.RemoveWhere(x =>
            x.IsNullOrEmpty() || string.IsNullOrWhiteSpace(x) || MatchEnglishAndNumbers.IsMatch(x));

        if (filterResult.Count < 1)
        {
            return true;
        }

        Host.Info($"分词筛选后结果: {string.Join(",", filterResult)}");
        return false;
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
                    bagOfWordVector.Vector));
            totalResult.AddRange(result);
        }

        return totalResult;
    }

    /// <summary>
    /// 计算消息向量
    /// <br/> 查询未删除的消息记录
    /// <br/> 对每个消息记录，使用词袋管理器获取消息向量, 并存储进向量集合里面
    /// </summary>
    private void ComputeMessageVector()
    {
        foreach (var msgRecord in GetCollection<MsgRecord>(NstMessageProcessMsgRecordCollection)
                     .Find(x => !x.HasDelete))
        {
            BagOfWordManager.GetMsgVectors(msgRecord);
        }
    }


    /// <summary>
    /// 将消息链解析为纯文本
    /// </summary>
    /// <param name="messageChain">消息链</param>
    /// <returns></returns>
    private string ParseTextMessage(MessageChain messageChain)
    {
        var rawData = string.Join(" ",
                messageChain.Where(x => x is TextEntity)
                    .Select(x => x.ToPreviewText()))
            .Trim()
            .Replace(Environment.NewLine, "")
            .Replace("\r", "")
            .Replace("\n", "");

        // 不去掉这些表情 消息存储进LiteDb会报错
        return EmojiPattern.Replace(rawData, string.Empty);
    }

    /// <summary>
    /// 加载停用词
    /// </summary>
    /// <returns>一个表示异步操作的任务</returns>
    /// <exception cref="IOException">当停用词文件路径不存在时抛出</exception>
    private async Task LoadStopWord()
    {
        // 获取应用程序当前路径
        var appCurrentPath = StaticValue.AppCurrentPath;
        // 拼接停用词文件路径
        var filePath = Path.Combine(appCurrentPath, "PluginResource", "NeverStopTalkingPlugin", "停用词.txt");
        // 检查文件是否存在
        if (!File.Exists(filePath))
        {
            // 记录错误信息并抛出异常
            Host.Error($"插件[{nameof(NeverStopTalkingPlugin)}]加载中出现异常, 停用词文件路径不存在");
            throw new IOException($"加载停用词失败, {filePath}");
        }

        // 打开文件流
        await using var fileStream = new FileStream(filePath, FileMode.Open);
        // 使用StreamReader读取文件内容
        using var streamReader = new StreamReader(fileStream);
        // 循环读取每一行，直到文件末尾
        while (!streamReader.EndOfStream)
        {
            var stopWord = await streamReader.ReadLineAsync();

            // 如果当前行为空，则跳过
            if (stopWord.IsNullOrEmpty())
            {
                continue;
            }

            // 将非空停用词添加到StopWord集合中
            StopWord.Add(stopWord!);
        }

        // 记录加载完成的信息，包含加载的停用词数量
        Host.Info($"停用词加载完毕, 一共加载了：{StopWord.Count}个停用词");
    }
}