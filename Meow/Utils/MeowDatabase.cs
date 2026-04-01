using FreeSql;
using Meow.Core.Model.Base;
using Meow.Plugin.OllamaChatPlugin.Models;
using Serilog;

namespace Meow.Utils;

public class MeowDatabase
{
    public MeowDatabase(string folderPath, string databaseName, ILogger logger, string databaseType = "Sqlite", string? connectionString = null)
    {
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        var path = Path.Combine(folderPath, $"{databaseName}.db");
        DatabaseName = databaseName;
        Logger = logger;

        if (Logger != null)
        {
            Logger.Information("数据库初始化中:{DatabaseName}, Type:{DatabaseType}", databaseName, databaseType);
        }

        var dataType = databaseType.ToLower() switch
        {
            "sqlite" => DataType.Sqlite,
            "mysql" => DataType.MySql,
            _ => DataType.Sqlite
        };

        var finalConnectionString = connectionString;
        if (dataType == DataType.Sqlite && string.IsNullOrEmpty(finalConnectionString))
        {
            finalConnectionString = $"Data Source={path}";
        }
        
        FreeSql = new FreeSqlBuilder()
            .UseConnectionString(dataType, finalConnectionString)
            .UseAutoSyncStructure(true)
            .Build();

        // 注册 JSON 序列化
        FreeSql.Aop.ConfigEntityProperty += (s, e) =>
        {
            if ((e.Property.PropertyType.IsGenericType && 
                 (e.Property.PropertyType.GetGenericTypeDefinition() == typeof(Dictionary<,>) || 
                  e.Property.PropertyType.GetGenericTypeDefinition() == typeof(List<>))) ||
                e.Property.PropertyType == typeof(OllamaSummaryResponse))
            {
                e.ModifyResult.StringLength = -1;
                e.ModifyResult.IsIgnore = false;
            }
        };

        logger?.Information("数据库加载完毕:{DatabaseName}, Type:{DatabaseType}, ConnectionString:{ConnectionString}", databaseName, dataType, finalConnectionString);
    }

    public IFreeSql FreeSql { get; private set; }

    private ILogger? Logger { get; set; }

    public void SetLogger(ILogger logger)
    {
        Logger = logger;
    }

    /// <summary>
    /// 数据库名称
    /// </summary>
    public string DatabaseName { get; private set; }

    #region CRUD

    /// <summary>
    /// 插入单个对象到数据库集合中，返回插入对象的ID。
    /// </summary>
    /// <typeparam name="T">需要插入的对象类型。</typeparam>
    /// <param name="target">需要插入的对象。</param>
    /// <param name="collectionName">目标数据库集合的名称(SQLite中暂不直接使用，但为了兼容性保留)。</param>
    /// <returns>返回插入对象的ID。</returns>
    public long Insert<T>(T target, string collectionName) where T : class
    {
        if (target is DatabaseRecordBase record)
        {
            record.SetCreateTime();
            record.RefreshUpdateTime();
        }

        var identity = FreeSql.Insert<T>().AppendData(target).ExecuteIdentity();
        Logger.Debug("Insert Result ID: {Id}", identity);
        if (target is DatabaseRecordBase recordBase)
        {
            recordBase.DbId = identity;
        }
        return identity;
    }

    /// <summary>
    /// 插入一系列对象到数据库集合中，返回插入对象的数量。
    /// </summary>
    /// <typeparam name="T">需要插入的对象类型。</typeparam>
    /// <param name="targetList">需要插入的对象集合。</param>
    /// <param name="collectionName">目标数据库集合的名称。</param>
    /// <returns>返回插入对象的数量。</returns>
    public int Insert<T>(IEnumerable<T> targetList, string collectionName) where T : class
    {
        var enumerable = targetList.ToList();
        foreach (var target in enumerable)
        {
            if (target is DatabaseRecordBase record)
            {
                record.SetCreateTime();
                record.RefreshUpdateTime();
            }
        }

        var insert = (int)FreeSql.Insert<T>().AppendData(enumerable).ExecuteAffrows();
        Logger.Debug("Insert Count: {InsertCount}", insert);
        return insert;
    }

    /// <summary>
    /// 更新数据库集合中的一个对象，如果操作成功返回true。
    /// </summary>
    /// <typeparam name="T">需要更新的对象类型。</typeparam>
    /// <param name="target">需要更新的对象。</param>
    /// <param name="collectionName">目标数据库集合的名称。</param>
    /// <returns>如果操作成功返回true。</returns>
    public bool Update<T>(T target, string collectionName) where T : class
    {
        if (target is DatabaseRecordBase record)
        {
            record.RefreshUpdateTime();
        }

        var update = FreeSql.Update<T>().SetSource(target).ExecuteAffrows();
        Logger.Debug("Update result {UpdateResult}", update > 0);
        return update > 0;
    }

    /// <summary>
    /// 更新数据库集合中的一系列对象，返回更新对象的数量。
    /// </summary>
    /// <typeparam name="T">需要更新的对象类型。</typeparam>
    /// <param name="targetList">目标对象集合</param>
    /// <param name="collectionName">目标数据库集合的名称。</param>
    /// <returns>返回更新对象的数量。</returns>
    public int Update<T>(IEnumerable<T> targetList, string collectionName) where T : class
    {
        var enumerable = targetList.ToList();
        var updateCount = 0;
        foreach (var target in enumerable)
        {
            if (target is DatabaseRecordBase record)
            { 
                record.RefreshUpdateTime();
            }
        }

        updateCount = (int)FreeSql.Update<T>().SetSource(enumerable).ExecuteAffrows();
        
        Logger.Debug("Update Count {UpdateCount}", updateCount);
        return updateCount;
    }

    /// <summary>
    /// 执行查询数据库集合，返回查询结果。
    /// </summary>
    /// <typeparam name="T">查询对象的类型。</typeparam>
    /// <param name="collectionName">目标数据库集合的名称。</param>
    /// <returns>查询结果。</returns>
    public ISelect<T> Query<T>(string collectionName) where T : class
    {
        return FreeSql.Select<T>();
    }

    #endregion
}