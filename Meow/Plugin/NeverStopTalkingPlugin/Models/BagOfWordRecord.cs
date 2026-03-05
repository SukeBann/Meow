using FreeSql.DataAnnotations;
using Masuit.Tools;
using Meow.Core.Model.Base;
using Newtonsoft.Json;

namespace Meow.Plugin.NeverStopTalkingPlugin.Models;

/// <summary>
/// 词袋记录
/// </summary>
public class BagOfWordRecord : DatabaseRecordBase
{
    public BagOfWordRecord()
    {
    }

    /// <summary>
    /// 创建词袋
    /// </summary>
    /// <param name="maxCount">最大长度</param>
    /// <param name="uin">群id或qq</param>
    /// <param name="bagOfWordType">词袋类型</param>
    public BagOfWordRecord(int maxCount, long uin, BagOfWordType bagOfWordType)
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
    [Column(StringLength = -1)]
    public string BagOfWordJson
    {
        get => JsonConvert.SerializeObject(BagOfWord);
        set => BagOfWord = value.IsNullOrEmpty() ? new() : JsonConvert.DeserializeObject<Dictionary<string, int>>(value) ?? new();
    }
    
    [Column(IsIgnore = true)]
    public Dictionary<string, int> BagOfWord { get; set; } = new();

    /// <summary>
    /// 尝试添加词到词袋中
    /// </summary>
    /// <param name="words"></param>
    public int TryAddBagOfWord(string[] words)
    {
        var addCount = 0;
        // 先判断一遍词袋是否满了, 如果满了则直接return
        if (IsFull)
        {
            return addCount;
        }

        foreach (var word in words)
        {
            // 被装满了就直接return
            RefreshIsFull();
            if (IsFull)
            {
                return addCount;
            }

            if (BagOfWord.ContainsKey(word))
            {
                continue;
            }

            if (BagOfWord.TryAdd(word, 0))
            {
                addCount++;
                BagOfWord[word] = BagOfWord.Count - 1;
            }
        }

        RefreshIsFull();
        return addCount;
    }

    /// <summary>
    /// 词袋容量
    /// </summary>
    public int MaxCount { get; set; }

    /// <summary>
    /// 是否已经装满
    /// </summary>
    [Column(IsIgnore = false)]
    public bool IsFull { get; set; }

    /// <summary>
    /// 更新是否装满状态
    /// </summary>
    public void RefreshIsFull() => IsFull = BagOfWord.Count >= MaxCount;

    /// <summary>
    /// 群id | qq | global:0
    /// </summary>
    public long Uin { get; set; }

    /// <summary>
    /// 词袋类型
    /// </summary>
    public BagOfWordType BagOfWordType { get; set; }
}