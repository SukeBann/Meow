using FreeSql;
using LiteDB;
using Meow.Core.Model;
using Meow.Core.Model.Base;
using Meow.Plugin.NeverStopTalkingPlugin.Models;
using Meow.Plugin.OllamaChatPlugin.Models;

Console.WriteLine("Meow Data Migration Tool");
Console.WriteLine("1. LiteDB -> SQLite");
Console.WriteLine("2. SQLite -> MySQL");
Console.Write("Choose mode (1 or 2): ");
string mode = Console.ReadLine() ?? "1";

if (mode == "1")
{
    MigrateLiteDbToSqlite();
}
else if (mode == "2")
{
    MigrateSqliteToMySql();
}
else
{
    Console.WriteLine("Invalid mode.");
}

static void MigrateLiteDbToSqlite()
{
    Console.WriteLine("Mode: LiteDB -> SQLite");
    Console.Write("Input LiteDB Path: ");
    string liteDbPath = Console.ReadLine() ?? throw new Exception("path can't be null");

    Console.Write("Input Sqlite Path: ");
    string sqliteDbPath = Console.ReadLine() ?? throw new Exception("path can't be null");

    if (!File.Exists(liteDbPath))
    {
        Console.WriteLine($"Error: LiteDB file not found at {liteDbPath}");
        return;
    }

    var fsql = new FreeSqlBuilder()
        .UseConnectionString(DataType.Sqlite, $"Data Source={sqliteDbPath}")
        .UseAutoSyncStructure(true)
        .Build();

    ConfigureFreeSql(fsql);

    // Sync all table structures before migration
    SyncStructures(fsql);

    using (var litedb = new LiteDatabase(liteDbPath))
    {
        bool allSuccess = true;
        allSuccess &= MigrateCollectionFromLiteDb<UserInfo>(litedb, fsql, "MeowUserInfoCollection");
        
        // PluginPermission and CommandPermission are stored together in LiteDB (CommandPermission as a Dictionary property)
        // FreeSql will split them into two tables in SQLite due to the model configuration.
        allSuccess &= MigrateCollectionFromLiteDb<PluginPermission>(litedb, fsql, "MeowPluginPermissionCollection");
        
        allSuccess &= MigrateCollectionFromLiteDb<BagOfWordRecord>(litedb, fsql, CollStr.NstBagOfWordManagerCollection);
        allSuccess &= MigrateCollectionFromLiteDb<ForbiddenWordRecord>(litedb, fsql, CollStr.NstForbiddenWordsManagerCollection);
        allSuccess &= MigrateCollectionFromLiteDb<MsgRecord>(litedb, fsql, CollStr.NstMessageProcessMsgRecordCollection);
        allSuccess &= MigrateCollectionFromLiteDb<BagOfWordVector>(litedb, fsql, CollStr.NstBagOfWordVectorCollection);
        
        // OllamaChatContext and OllamaGroupSummary were added later, check if they exist in LiteDB
        // They use class name as collection name by default in LiteDB if not specified
        MigrateCollectionFromLiteDb<OllamaChatContext>(litedb, fsql, nameof(OllamaChatContext));
        MigrateCollectionFromLiteDb<OllamaGroupSummary>(litedb, fsql, nameof(OllamaGroupSummary));

        if (allSuccess)
        {
            Console.WriteLine("\nMigration completed successfully!");
        }
        else
        {
            Console.WriteLine("\nMigration completed with errors. Please check the logs above.");
        }
    }
}

static void MigrateSqliteToMySql()
{
    Console.WriteLine("Mode: SQLite -> MySQL");
    Console.Write("Input SQLite Path: ");
    string sqliteDbPath = Console.ReadLine() ?? throw new Exception("path can't be null");

    Console.Write("Input MySQL Connection String (Must include 'Database=your_db_name'): ");
    string mysqlConnStr = Console.ReadLine() ?? throw new Exception("connection string can't be null");

    if (!mysqlConnStr.ToLower().Contains("database="))
    {
        Console.WriteLine("Error: Connection string must include 'Database=your_db_name'.");
        return;
    }

    if (!File.Exists(sqliteDbPath))
    {
        Console.WriteLine($"Error: SQLite file not found at {sqliteDbPath}");
        return;
    }

    var sqliteFsql = new FreeSqlBuilder()
        .UseConnectionString(DataType.Sqlite, $"Data Source={sqliteDbPath}")
        .Build();

    var mysqlFsql = new FreeSqlBuilder()
        .UseConnectionString(DataType.MySql, mysqlConnStr)
        .UseAutoSyncStructure(true)
        .Build();

    ConfigureFreeSql(sqliteFsql);
    ConfigureFreeSql(mysqlFsql);

    // Sync all table structures on both source and target databases
    SyncStructures(sqliteFsql);
    SyncStructures(mysqlFsql);

    Console.WriteLine("Starting migration...");

    bool allSuccess = true;
    allSuccess &= MigrateTableBetweenFsql<UserInfo>(sqliteFsql, mysqlFsql);
    allSuccess &= MigrateTableBetweenFsql<PluginPermission>(sqliteFsql, mysqlFsql);
    allSuccess &= MigrateTableBetweenFsql<CommandPermission>(sqliteFsql, mysqlFsql);
    allSuccess &= MigrateTableBetweenFsql<BagOfWordRecord>(sqliteFsql, mysqlFsql);
    allSuccess &= MigrateTableBetweenFsql<ForbiddenWordRecord>(sqliteFsql, mysqlFsql);
    allSuccess &= MigrateTableBetweenFsql<MsgRecord>(sqliteFsql, mysqlFsql);
    allSuccess &= MigrateTableBetweenFsql<BagOfWordVector>(sqliteFsql, mysqlFsql);
    allSuccess &= MigrateTableBetweenFsql<OllamaChatContext>(sqliteFsql, mysqlFsql);
    allSuccess &= MigrateTableBetweenFsql<OllamaGroupSummary>(sqliteFsql, mysqlFsql);

    if (allSuccess)
    {
        Console.WriteLine("\nMigration completed successfully!");
    }
    else
    {
        Console.WriteLine("\nMigration completed with errors. Please check the logs above.");
    }
}

static void ConfigureFreeSql(IFreeSql fsql)
{
    fsql.Aop.ConfigEntityProperty += (s, e) =>
    {
        if (e.Property.PropertyType.IsGenericType && 
            (e.Property.PropertyType.GetGenericTypeDefinition() == typeof(Dictionary<,>) || 
             e.Property.PropertyType.GetGenericTypeDefinition() == typeof(List<>)))
        {
            e.ModifyResult.StringLength = -1;
            e.ModifyResult.IsIgnore = false;
            if (fsql.Ado.DataType == DataType.MySql)
            {
                e.ModifyResult.DbType = "LONGTEXT";
            }
        }
        else if (e.Property.PropertyType == typeof(OllamaSummaryResponse))
        {
            e.ModifyResult.StringLength = -1;
            e.ModifyResult.IsIgnore = false;
            if (fsql.Ado.DataType == DataType.MySql)
            {
                e.ModifyResult.DbType = "LONGTEXT";
            }
        }
        else if (e.Property.PropertyType == typeof(string))
        {
            // Ensure long strings are supported in MySQL by using TEXT/LONGTEXT
            e.ModifyResult.StringLength = -1;
            if (fsql.Ado.DataType == DataType.MySql)
            {
                e.ModifyResult.DbType = "LONGTEXT";
            }
        }
    };

    if (fsql.Ado.DataType == DataType.MySql)
    {
        // Explicitly configure problematic entities to use LONGTEXT for their long string fields
        fsql.CodeFirst.ConfigEntity<BagOfWordRecord>(t => t.Property(x => x.BagOfWordJson).DbType("LONGTEXT"));
        fsql.CodeFirst.ConfigEntity<MsgRecord>(t => t.Property(x => x.TextMsg).DbType("LONGTEXT"));
        fsql.CodeFirst.ConfigEntity<OllamaGroupSummary>(t => t.Property(x => x.Summary).DbType("LONGTEXT"));
    }
}

static void SyncStructures(IFreeSql fsql)
{
    Console.WriteLine("Syncing table structures...");

    fsql.CodeFirst.SyncStructure<UserInfo>();
    fsql.CodeFirst.SyncStructure<PluginPermission>();
    fsql.CodeFirst.SyncStructure<CommandPermission>();
    fsql.CodeFirst.SyncStructure<BagOfWordRecord>();
    fsql.CodeFirst.SyncStructure<ForbiddenWordRecord>();
    fsql.CodeFirst.SyncStructure<MsgRecord>();
    fsql.CodeFirst.SyncStructure<BagOfWordVector>();
    fsql.CodeFirst.SyncStructure<OllamaChatContext>();
    fsql.CodeFirst.SyncStructure<OllamaGroupSummary>();

    if (fsql.Ado.DataType == DataType.MySql)
    {
        ForceColumnLongText<BagOfWordRecord>(fsql, x => x.BagOfWordJson);
        ForceColumnLongText<MsgRecord>(fsql, x => x.TextMsg);
        ForceColumnLongText<OllamaGroupSummary>(fsql, x => x.Summary);
        Console.WriteLine("Forced LONGTEXT on critical columns.");
    }

    Console.WriteLine("Table structures synced.");
}

static void ForceColumnLongText<T>(IFreeSql fsql, System.Linq.Expressions.Expression<Func<T, object>> propertySelector) where T : class
{
    var tableInfo = fsql.CodeFirst.GetTableByEntity(typeof(T));
    // Resolve the actual property name from the expression
    var memberExpr = propertySelector.Body is System.Linq.Expressions.UnaryExpression unary
        ? (System.Linq.Expressions.MemberExpression)unary.Operand
        : (System.Linq.Expressions.MemberExpression)propertySelector.Body;
    var propName = memberExpr.Member.Name;

    if (!tableInfo.ColumnsByCs.TryGetValue(propName, out var col))
    {
        Console.WriteLine($"Warning: Property '{propName}' not found in {typeof(T).Name}, skipping ALTER.");
        return;
    }

    var tableName = tableInfo.DbName;
    var columnName = col.Attribute.Name ?? propName;
    fsql.Ado.ExecuteNonQuery($"ALTER TABLE `{tableName}` MODIFY `{columnName}` LONGTEXT;");
}

static bool MigrateCollectionFromLiteDb<T>(LiteDatabase litedb, IFreeSql fsql, string collectionName) where T : class
{
    Console.WriteLine($"Migrating collection: {collectionName}...");
    try 
    {
        var collection = litedb.GetCollection<T>(collectionName);
        var items = collection.FindAll().ToList();
        
        if (items.Count == 0)
        {
            Console.WriteLine($"Collection {collectionName} is empty, skipping.");
            return true;
        }

        var affrows = fsql.Insert<T>().AppendData(items).ExecuteAffrows();
        Console.WriteLine($"Successfully migrated {affrows} records from {collectionName}.");
        return true;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error migrating {collectionName}: {ex.Message}");
        return false;
    }
}

static bool MigrateTableBetweenFsql<T>(IFreeSql source, IFreeSql target) where T : DatabaseRecordBase
{
    string typeName = typeof(T).Name;
    Console.WriteLine($"Migrating table: {typeName}...");

    try
    {
        var items = source.Select<T>().ToList();
        if (items.Count == 0)
        {
            Console.WriteLine($"Table {typeName} is empty, skipping.");
            return true;
        }

        // 查询目标库中已存在的 DbId，跳过已有记录
        var existingIds = target.Select<T>().ToList(x => x.DbId).ToHashSet();
        var newItems = items.Where(x => !existingIds.Contains(x.DbId)).ToList();

        if (newItems.Count == 0)
        {
            Console.WriteLine($"All records for {typeName} already exist in target, skipping.");
            return true;
        }

        var affrows = 0;
        // 如果数据量较大，分批次插入，避免 MySQL 数据包大小限制 (max_allowed_packet)
        // 且由于数据可能很大，我们每次只插 1 条来确保单条不会超过限制（或者至少能定位到哪条）
        // 这里改为 1 条一组，虽然慢但最稳
        for (int i = 0; i < newItems.Count; i++)
        {
            try 
            {
                var item = newItems[i];
                affrows += target.Insert<T>().AppendData(item).ExecuteAffrows();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inserting record {i+1}/{newItems.Count}: {ex.Message}");
                // 继续下一条，不要因为一条失败就终止整个表
            }
        }
        Console.WriteLine($"Successfully migrated {affrows} records for {typeName}.");
        return true;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error migrating {typeName}: {ex.Message}");
        return false;
    }
}
