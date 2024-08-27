using System.Collections.Concurrent;
using Meow.Core.Model.Base;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Meow.Plugin.NeverStopTalkingPlugin.Models;

/// <summary>
/// 词袋记录
/// </summary>
public class BagOfWordRecord : DatabaseRecordBase
{
    /// <summary>
    /// 创建词袋
    /// </summary>
    /// <param name="maxCount">最大长度</param>
    /// <param name="uin">群id或qq</param>
    /// <param name="bagOfWordType">词袋类型</param>
    public BagOfWordRecord(int maxCount, uint uin, BagOfWordType bagOfWordType)
    {
        MaxCount = maxCount;
        BagOfWordType = bagOfWordType;
        Uin = uin;
        CreateTime = DateTime.Now;
    }

    /// <summary>
    /// 词袋
    /// <br/> key: 词
    /// <br/> value: 索引
    /// </summary>
    public ConcurrentDictionary<string, int> BagOfWord { get; set; } = new();
    
    /// <summary>
    /// 当前添加到哪个索引
    /// </summary>
    public int CurrentIndex { get; set; } = 0;

    /// <summary>
    /// 尝试添加词到词袋中
    /// </summary>
    /// <param name="words"></param>
    public int TryAddBagOfWord(List<string> words)
    {
        var addCount = 0;
        // 先判断一遍词袋是否满了, 如果满了则直接return
        if (IsFull)
        {
            return addCount;
        }
        
        foreach (var word in words)
        {
            // 被装满了就直接return, 这个字典是可能会被并发访问的 所以这个判断是必须的
            if (IsFull)
            {
                return addCount;
            }
            
            // 尝试重复添加
            while (!BagOfWord.ContainsKey(word))
            {
                if (IsFull)
                {
                    return addCount;
                }
                BagOfWord.TryAdd(word, 0);
            }

            addCount++;
            BagOfWord[word] = CurrentIndex++;
        }

        return addCount;
    }

    /// <summary>
    /// 词袋容量
    /// </summary>
    public int MaxCount { get; set; }

    /// <summary>
    /// 是否已经装满
    /// </summary>
    public bool IsFull => BagOfWord.Count == MaxCount;

    /// <summary>
    /// 群id | qq | global:0
    /// </summary>
    public uint Uin { get; set; }

    /// <summary>
    /// 词袋类型
    /// </summary>
    public BagOfWordType BagOfWordType { get; private set; }
}