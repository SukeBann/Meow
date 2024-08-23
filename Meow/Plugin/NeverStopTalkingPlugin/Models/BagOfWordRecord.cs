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
    }

    /// <summary>
    /// 词袋
    /// <br/> key: 词
    /// <br/> value: 索引
    /// </summary>
    public ConcurrentDictionary<string, int> BagOfWord { get; } = new();

    /// <summary>
    /// 用于记录当前词袋添加到了哪个索引位置,方便更新字典中的索引 而不用便利字典
    /// </summary>
    public List<string> BagOfWordList { get; set; } = new();

    /// <summary>
    /// 尝试添加词到词袋中
    /// </summary>
    /// <param name="words"></param>
    public void TryAddBagOfWord(List<string> words)
    {
        // 先判断一遍词袋是否满了, 如果满了则直接return
        if (IsFull)
        {
            return;
        }
        
        foreach (var word in words)
        {
            // 被装满了就直接return, 这个字典是可能会被并发访问的 所以这个判断是必须的
            if (IsFull)
            {
                return;
            }
            // 尝试重复添加
            while (!BagOfWord.ContainsKey(word))
            {
                if (IsFull)
                {
                    return;
                }
                BagOfWordList.Add(word);
                BagOfWord.TryAdd(word, BagOfWordList.Count - 1);
            }
        }
    }

    /// <summary>
    /// 词袋容量
    /// </summary>
    public int MaxCount { get; }

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