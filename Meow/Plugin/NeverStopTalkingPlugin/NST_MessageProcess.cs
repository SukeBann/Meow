﻿using System.Collections.Concurrent;
using System.Reflection.Metadata.Ecma335;
using JiebaNet.Segmenter;
using Lagrange.Core.Message;
using Lagrange.Core.Message.Entity;
using Masuit.Tools;
using Meow.Core.Model.Base;
using Meow.Plugin.NeverStopTalkingPlugin.Models;
using Meow.Utils;

namespace Meow.Plugin.NeverStopTalkingPlugin;

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
public class NstMessageProcess : HostDatabaseSupport
{
    /// <inheritdoc />
    public NstMessageProcess(NstForbiddenWordsManager forbiddenWordsManager, NstBagOfWordManager bagOfWordManager, Core.Meow host) : base(host)
    {
        ForbiddenWordsManager = forbiddenWordsManager;
        BagOfWordManager = bagOfWordManager;

        LoadAllMessageVector();
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
    private NstForbiddenWordsManager ForbiddenWordsManager { get; set; }
    
    /// <summary>
    /// 词袋管理器
    /// </summary>
    private NstBagOfWordManager BagOfWordManager { get; set; }

    /// <summary>
    /// 停用词
    /// </summary>
    private HashSet<string> StopWord { get; set; }

    /// <summary>
    /// 词袋对应的所有计算过消息向量的数据
    /// </summary>
    private List<BagOfWordVector> MessageVectorList { get; set; } = new();

    #endregion
    
    /// <summary>
    /// 处理消息链
    /// </summary>
    /// <param name="messageChain">消息链</param>
    /// <returns></returns>
    public (bool isSendBack, MessageChain messageChain) ProcessMessage(MessageChain messageChain)
    {
        var textMessage = ParseTextMessage(messageChain);
        var filterResult = new JiebaSegmenter().Cut(textMessage)
            .Where(x => !StopWord.Contains(x))
            .Where(x => ForbiddenWordsManager.CheckForbiddenWordsManager(x))
            .ToList();

        // 计算完消息向量
        var msgRecord = BagOfWordManager.ProcessCutResult(messageChain, textMessage, filterResult);
        FindMostSimilarMsg(msgRecord);
    }

    /// <summary>
    /// 从所有已经计算过向量的消息中寻找最相似的几条
    /// </summary>
    /// <param name="msgRecord"></param>
    /// <exception cref="NotImplementedException"></exception>
    private List<string> FindMostSimilarMsg(MsgRecord msgRecord)
    {
        
        
        foreach (var (bowId, value) in msgRecord.WordVector)
        {
            if (value is null)
            {
                continue;
            }

            foreach (var (_, _, vector) in MessageVectorList.Where(x => x.BagOfWordId == bowId))
            {
                
            }
        }
    }

    /// <summary>
    /// 获取所有计算过向量的消息
    /// </summary>
    private void LoadAllMessageVector()
    {
        foreach (var msgRecord in Query<MsgRecord>(NstMessageProcessMsgRecordCollection).Where(x => x.HaveAnyVector).ToList())
        {
            foreach (var (bowId, value) in msgRecord.WordVector)
            {
                // 向量为空直接退出
                if (value is null)
                {
                    continue;
                }
                MessageVectorList.Add(new BagOfWordVector(bowId, msgRecord.DbId, value));
            }
        }
    }

    /// <summary>
    /// 将消息链解析为纯文本
    /// </summary>
    /// <param name="messageChain">消息链</param>
    /// <returns></returns>
    private string ParseTextMessage(MessageChain messageChain)
    {
        return string.Join(" ", messageChain.Where(x => x is TextEntity)).Trim();
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