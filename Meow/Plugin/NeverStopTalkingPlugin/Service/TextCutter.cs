using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using JiebaNet.Segmenter;
using Lagrange.Core.Message;
using Lagrange.Core.Message.Entity;
using Masuit.Tools;
using Meow.Utils;

namespace Meow.Plugin.NeverStopTalkingPlugin.Service;

/// <summary>
/// 文本处理器
/// </summary>
public partial class TextCutter
{
    /// <summary>
    /// 停用词
    /// </summary>
    private HashSet<string> StopWord { get; }

    /// <summary>
    /// 违禁词管理器
    /// </summary>
    private ForbiddenWordsManager ForbiddenWordsManager { get; }

    /// <summary>
    /// 插件宿主
    /// </summary>
    private Core.Meow Host { get; }

    /// <summary>
    /// 分词器
    /// </summary>
    private JiebaSegmenter WordCutter { get; } = new();

    /// <summary>
    /// 匹配纯英文或数字
    /// </summary>
    private Regex MatchEnglishAndNumbers { get; } = En_Num_Pattern();

    /// <summary>
    /// 匹配表情
    /// </summary>
    private readonly Regex _emojiPattern = GEmojiPattern();

    [GeneratedRegex("^[a-zA-Z0-9]+$")]
    private static partial Regex En_Num_Pattern();

    // ReSharper disable once StringLiteralTypo
    [GeneratedRegex(
        @"[\uD800-\uDBFF][\uDC00-\uDFFF]|[\u2600-\u26FF]|[\u2700-\u27BF]|[\uE000-\uF8FF]|[\u2100-\u214F]|[\u203C\u2049]",
        RegexOptions.Compiled)]
    private static partial Regex GEmojiPattern();

    public TextCutter(Core.Meow host, ForbiddenWordsManager forbiddenWordsManager)
    {
        Host = host;
        ForbiddenWordsManager = forbiddenWordsManager;
        StopWord = [];
        _ = LoadStopWord();
    }

    /// <summary>
    /// 将消息链解析为纯文本
    /// </summary>
    /// <param name="messageChain">消息链</param>
    /// <returns></returns>
    private string ConvertToPlainText(MessageChain messageChain)
    {
        var rawData = string.Join(" ",
                messageChain.Where(x => x is TextEntity)
                    .Select(x => x.ToPreviewText()))
            .Trim()
            .Replace("\r", "")
            .Replace("\n", "");

        // 不去掉这些表情 消息存储进LiteDb会报错
        return _emojiPattern.Replace(rawData, string.Empty);
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
            var stopWord = await streamReader.ReadLineAsync().ConfigureAwait(false);

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

    /// <summary>
    /// 分词
    /// </summary>
    /// <param name="textMessage"></param>
    /// <param name="filterResult"></param>
    /// <returns></returns>
    public bool CutPlainText(string textMessage, [MaybeNullWhen(true)]out string[] filterResult)
    {
        var cutResult = WordCutter.Cut(textMessage, cutAll: true)
            .Where(x => !StopWord.Contains(x))
            .ToList();

        if (cutResult.Any(x => ForbiddenWordsManager.CheckForbiddenWordsManager(x)))
        {
            Host.Info($"识别到违禁词, 不处理该条消息: {textMessage}");
            filterResult = null;
            return true;
        }

        // 移除空格、空字符、只有数字或者英文的字符
        cutResult.RemoveWhere(x =>
            x.IsNullOrEmpty() || string.IsNullOrWhiteSpace(x) || MatchEnglishAndNumbers.IsMatch(x));

        // 全部转为小写, liteDB 狗屎数据库 存储字典的时候会把字典里面的键忽略大小写
        filterResult = cutResult.Select(x => x.ToLower()).ToArray();

        return filterResult.Length < 1;
    }
    
    
    /// <summary>
    /// 获取文本消息并进行处理。
    /// </summary>
    /// <param name="messageChain">消息链。</param>
    /// <param name="textMessage">输出参数，解析后的文本消息。</param>
    /// <param name="filterResult">输出参数，经过筛选后的结果列表。</param>
    /// <returns>如果消息不需要进一步处理，返回 true；否则返回 false。</returns>
    public bool GetTextMsg(MessageChain messageChain,
        out string textMessage,
        [MaybeNullWhen(true)] out string[] filterResult)
    {
        filterResult = null;
        textMessage = string.Empty;

        // 不处理自身消息
        if (messageChain.FriendUin == Host.MeowBot.BotUin)
        {
            return true;
        }

        textMessage = ConvertToPlainText(messageChain);
        if (textMessage.IsNullOrEmpty())
        {
            return true;
        }

        // 分词
        if (CutPlainText(textMessage, out filterResult))
        {
            return true;
        }

        Host.Info($"分词筛选后结果: {string.Join(",", filterResult)}");
        return false;
    }

}