using LiteDB;
using Serilog;

namespace Meow.Utils;

public class MeowDatabase
{
    public MeowDatabase(string folderPath, string databaseName, ILogger logger)
    {
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        var path = Path.Combine(folderPath, $"{databaseName}.db");
        DatabaseName = databaseName;
        Logger = logger;
        Repository = new LiteRepository(path);
        logger.Information("数据库加载完毕:{DatabaseName}, Path:{Path}", databaseName, path);
    }

    private LiteRepository Repository { get; set; }

    private ILogger Logger { get; set; }

    /// <summary>
    /// 数据库名称
    /// </summary>
    public string DatabaseName { get; private set; }

    #region CRUD

    /// <summary>
    /// 插入单个对象到数据库集合中，返回插入对象的BsonValue。
    /// </summary>
    /// <typeparam name="T">需要插入的对象类型。</typeparam>
    /// <param name="target">需要插入的对象。</param>
    /// <param name="collectionName">目标数据库集合的名称。</param>
    /// <returns>返回插入对象的BsonValue。</returns>
    public BsonValue Insert<T>(T target, string collectionName)
    {
        return Repository.Insert<T>(target, collectionName);
    }

    /// <summary>
    /// 插入一系列对象到数据库集合中，返回插入对象的数量。
    /// </summary>
    /// <typeparam name="T">需要插入的对象类型。</typeparam>
    /// <param name="targetList">需要插入的对象集合。</param>
    /// <param name="collectionName">目标数据库集合的名称。</param>
    /// <returns>返回插入对象的数量。</returns>
    public int Insert<T>(IEnumerable<T> targetList, string collectionName)
    {
        return Repository.Insert<T>(targetList, collectionName);
    }

    /// <summary>
    /// 更新数据库集合中的一个对象，如果操作成功返回true。
    /// </summary>
    /// <typeparam name="T">需要更新的对象类型。</typeparam>
    /// <param name="target">需要更新的对象。</param>
    /// <param name="collectionName">目标数据库集合的名称。</param>
    /// <returns>如果操作成功返回true。</returns>
    public bool Update<T>(T target, string collectionName)
    {
        return Repository.Update(target, collectionName);
    }

    /// <summary>
    /// 更新数据库集合中的一系列对象，返回更新对象的数量。
    /// </summary>
    /// <typeparam name="T">需要更新的对象类型。</typeparam>
    /// <param name="target">需要更新的对象集合。</param>
    /// <param name="collectionName">目标数据库集合的名称。</param>
    /// <returns>返回更新对象的数量。</returns>
    public int Update<T>(IEnumerable<T> target, string collectionName)
    {
        return Repository.Update(target, collectionName);
    }

    /// <summary>
    /// 执行查询数据库集合，返回查询结果。
    /// </summary>
    /// <typeparam name="T">查询对象的类型。</typeparam>
    /// <param name="collectionName">目标数据库集合的名称。</param>
    /// <returns>查询结果。</returns>
    public ILiteQueryable<T> Query<T>(string collectionName)
    {
        return Repository.Query<T>(collectionName);
    }

    #endregion
}