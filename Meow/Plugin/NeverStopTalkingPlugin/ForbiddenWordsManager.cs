using Meow.Plugin.NeverStopTalkingPlugin;

/// <summary>
/// 违禁词管理
/// </summary>
/// <param name="meow"></param>
/// <param name="sender"></param>
public class ForbiddenWordsManager(Meow.Core.Meow meow, uint sender)
{
    /// <summary>
    /// 违禁词数据库集合
    /// </summary>
    public const string ForbiddenWordsCollection = nameof(ForbiddenWordsCollection);

    /// <summary>
    /// 添加违禁词
    /// </summary>
    /// <param name="word"></param>
    public void AddForbiddenWord(string word)
    {
        var record = meow.Database.Db.Query<ForbiddenWordRecord>(ForbiddenWordsCollection)
                    .Where(x => x.ForbiddenWord == word)
                    .FirstOrDefault();

        if (record != null)
        {
            if (!record.HasDelete)
            {
                return;
            }

            record.HasDelete = false;
            meow.Database.Db.Update(record, ForbiddenWordsCollection);
        }
        else
        {
            var forbiddenWordRecord = new ForbiddenWordRecord(sender, word, false);
            meow.Database.Db.Insert(forbiddenWordRecord, ForbiddenWordsCollection);
        }
    }

    /// <summary>
    /// 删除[逻辑删除]违禁词
    /// </summary>
    /// <param name="word"></param>
    public void RemoveForbiddenWord(string word)
    {
        var record = meow.Database.Db.Query<ForbiddenWordRecord>(ForbiddenWordsCollection)
                    .Where(x => x.ForbiddenWord == word)
                    .FirstOrDefault();

        if (record != null)
        {
            if (record.HasDelete)
            {
                return;
            }

            record.HasDelete = true;
            meow.Database.Db.Update(record, ForbiddenWordsCollection);
        }
        else
        {
            var forbiddenWordRecord = new ForbiddenWordRecord(sender, word, true);
            meow.Database.Db.Insert(forbiddenWordRecord, ForbiddenWordsCollection);
        }
    }
}