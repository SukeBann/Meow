using LiteDB;
using Meow.Utils;

namespace Meow.Core.Model.Base;

/// <summary>
/// HostDatabaseSupport类提供了一组子类可用的数据库操作方法。
/// </summary>
public abstract class HostDatabaseSupport
{
    /// <summary>
    /// 获取此实例操作的MeowDatabase对象。
    /// </summary>
    private MeowDatabase Database { get; }
    protected Meow Host { get; }

    /// <summary>
    /// 用指定的Meow对象初始化HostDatabaseSupport类的新实例。
    /// </summary>
    /// <param name="host">此实例将用于操作的Meow对象。</param>
    protected HostDatabaseSupport(Meow host)
    {
        Host = host;
        Database = host.Database;
    }

    /// <summary>
    /// 将对象插入到数据库集合中并返回其BsonValue。
    /// </summary>
    /// <typeparam name="T">要插入的对象的类型。</typeparam>
    /// <param name="target">要插入的对象。</param>
    /// <param name="collectionName">目标数据库集合的名称。</param>
    /// <returns>插入对象的BsonValue。</returns>
    protected virtual BsonValue Insert<T>(T target, string collectionName)
    {
        return Database.Insert(target, collectionName);
    }

    /// <summary>
    /// 将一系列对象插入到数据库集合中并返回插入对象的数量。
    /// </summary>
    /// <typeparam name="T">要插入的对象的类型。</typeparam>
    /// <param name="targetList">要插入的对象集合。</param>
    /// <param name="collectionName">目标数据库集合的名称。</param>
    /// <returns>插入对象的数量。</returns>
    public virtual int Insert<T>(IEnumerable<T> targetList, string collectionName)
    {
        return Database.Insert(targetList, collectionName);
    }

    /// <summary>
    /// 更新数据库集合中的一个对象，如果操作成功返回true。
    /// </summary>
    /// <typeparam name="T">需要更新的对象的类型。</typeparam>
    /// <param name="target">需要更新的对象。</param>
    /// <param name="collectionName">目标数据库集合的名称。</param>
    /// <returns>如果操作成功返回true。</returns>
    protected virtual bool Update<T>(T target, string collectionName)
    {
        return Database.Update(target, collectionName);
    }

    /// <summary>
    /// 更新数据库集合中的一系列对象，返回更新对象的数量。
    /// </summary>
    /// <typeparam name="T">需要更新的对象的类型。</typeparam>
    /// <param name="target">需要更新的对象集合。</param>
    /// <param name="collectionName">目标数据库集合的名称。</param>
    /// <returns>更新对象的数量。</returns>
    protected virtual int UpdateCollection<T>(IEnumerable<T> target, string collectionName)
    {
        return Database.Update(target, collectionName);
    }

    /// <summary>
    /// 查询数据库集合，返回查询结果。
    /// </summary>
    /// <typeparam name="T">查询对象的类型。</typeparam>
    /// <param name="collectionName">目标数据库集合的名称。</param>
    /// <returns>查询结果。</returns>
    protected virtual ILiteQueryable<T> Query<T>(string collectionName)
    {
        return Database.Query<T>(collectionName);
    }
}