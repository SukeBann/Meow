using LiteDB;
using Serilog;

namespace Meow.Utils;

public class MeowDatabase
{
    public ILiteRepository Db { get; private set; }
    
    private ILogger Logger { get; set; }
    
    /// <summary>
    /// 数据库名称
    /// </summary>
    public string DatabaseName { get; private set; }
    
    public MeowDatabase(string folderPath, string databaseName, ILogger logger)
    {
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        var path = Path.Combine(folderPath, $"{databaseName}.db");
        DatabaseName = databaseName;
        Logger = logger;
        Db = new LiteRepository(path);
        logger.Information("数据库加载完毕:{DatabaseName}, Path:{Path}", databaseName, path);
    }
}