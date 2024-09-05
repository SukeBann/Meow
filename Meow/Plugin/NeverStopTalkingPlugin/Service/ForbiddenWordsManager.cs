using Meow.Core.Model.Base;
using Meow.Plugin.NeverStopTalkingPlugin.Models;

namespace Meow.Plugin.NeverStopTalkingPlugin.Service;

/// <summary>
/// 违禁词管理
/// </summary>
public class ForbiddenWordsManager : HostDatabaseSupport
{
    /// <inheritdoc />
    public ForbiddenWordsManager(Meow.Core.Meow host) : base(host)
    {
        const int expectedElements = 5000;
        // ForbiddenWordsFilter = FilterBuilder.Build(expectedElements, 0.01);
        ForbiddenWordsFilter = [];
        LoadForbiddenWordFromDb();
    }

    #region Properties

    /// <summary>
    /// 违禁词过滤器
    /// </summary>
    private HashSet<string> ForbiddenWordsFilter { get; }

    #endregion

    /// <summary>
    /// 从数据库中读取违禁词
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    private void LoadForbiddenWordFromDb()
    {
        var forbiddenWordRecords = Query<ForbiddenWordRecord>(CollStr.NstForbiddenWordsManagerCollection)
            .Where(x => !x.HasDelete)
            .Select(x => x.ForbiddenWord).ToList();
        foreach (var forbiddenWordRecord in forbiddenWordRecords)
        {
            ForbiddenWordsFilter.Add(forbiddenWordRecord);
        }

        Host.Info($"添加违禁词成功, 总共添加了{forbiddenWordRecords.Count}个");
    }

    /// <summary>
    /// 检查文本中是否包含违禁词
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public bool CheckForbiddenWordsManager(string message)
    {
        return ForbiddenWordsFilter.Contains(message);
    }

    /// <summary>
    /// 添加违禁词
    /// </summary>
    /// <param name="word"></param>
    /// <param name="sender"></param>
    public void AddForbiddenWord(string word, uint sender)
    {
        var record = Query<ForbiddenWordRecord>(CollStr.NstForbiddenWordsManagerCollection)
            .Where(x => x.ForbiddenWord == word)
            .FirstOrDefault();

        if (record != null)
        {
            // 有记录 并且不是被删除就直接返回
            if (!record.HasDelete)
            {
                return;
            }

            // 否则取消删除
            Update(record.SetDeleteState(false), CollStr.NstForbiddenWordsManagerCollection);
            ForbiddenWordsFilter.Add(record.ForbiddenWord);
        }
        else
        {
            record = new ForbiddenWordRecord(sender, word);
            Insert(record, CollStr.NstForbiddenWordsManagerCollection);
            ForbiddenWordsFilter.Add(record.ForbiddenWord);
        }
    }

    /// <summary>
    /// 删除[逻辑删除]违禁词
    /// </summary>
    /// <param name="word"></param>
    public void RemoveForbiddenWord(string word)
    {
        var record = Query<ForbiddenWordRecord>(CollStr.NstForbiddenWordsManagerCollection)
            .Where(x => x.ForbiddenWord == word)
            .FirstOrDefault();

        if (record == null)
        {
            return;
        }

        if (record.HasDelete)
        {
            return;
        }

        // 有记录并且不是被删除状态 就把记录删除, 否则都直接return
        Update(record.SetDeleteState(true), CollStr.NstForbiddenWordsManagerCollection);
        // 因为布隆过滤器删除元素异常困难, 所以移除掉违禁词之后要重启才会生效, 我认为这是可以接受的
    }
}