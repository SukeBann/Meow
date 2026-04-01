using Autofac;
using Camille.Core.Enum.MiraiBot;
using Camille.Core.MiraiBase.Contract;
using Camille.Imp.MiraiBase;
using Meow.Config;
using Meow.Utils;
using Newtonsoft.Json;
using Serilog;
using Meow.Core;

namespace Meow.Bootstrapper;

/// <summary>
/// 启动器
/// </summary>
public class MeowBootstrapper
{
    private MeowBootstrapper(ILogger log)
    {
        Log = log;
    }

    #region Properties

    private ILogger Log { get; set; }

    /// <summary>
    /// meow工作目录
    /// </summary>
    private string? WorkDir { get; set; }

    /// <summary>
    /// Meow实例
    /// </summary>
    private Core.Meow? Meow { get; set; }

    #endregion

    #region Init

    /// <summary>
    /// 是否以及初始化
    /// </summary>
    private static bool IsInit { get; set; }

    private static ContainerBuilder? _builder;

    /// <summary>
    /// 初始化
    /// </summary>
    /// <returns></returns>
    public static MeowBootstrapper Init()
    {
        if (IsInit)
        {
            throw new InvalidOperationException($"请不要重复初始化{nameof(MeowBootstrapper)}");
        }

        _builder = new ContainerBuilder();
        ConfigurationServices(_builder);
        var meowBootstrapper = new MeowBootstrapper(null!); // 暂时传null，加载配置后再初始化
        IsInit = true;
        return meowBootstrapper;
    }

    /// <summary>
    /// 配置IOC
    /// </summary>
    private static void ConfigurationServices(ContainerBuilder builder)
    {
        var currentDomainBaseDirectory = StaticValue.AppCurrentPath;
        var logger = LoggerCreator.GenerateLoggerConfig(currentDomainBaseDirectory)
            .CreateLogger();
        LoggerCreator.SyncToCamille(logger);
        logger.Information("全局日志模块加载完毕");
        builder.RegisterInstance(logger).As<ILogger>().SingleInstance();
    }

    #endregion

    #region Build

    /// <summary>
    /// 设置工作目录, 配置文件以及bot产生的文件都会存放到此目录中下的对应Meow name文件夹中
    /// </summary>
    /// <returns></returns>
    /// <exception cref="IOException">如果目录不存在则会抛出此异常</exception>
    public MeowBootstrapper ConfigurationBot()
    {
        var config = GetConfig();
        WorkDir = Path.Combine(config.BotWorkDir, config.BotName);
        if (!Directory.Exists(WorkDir))
        {
            Directory.CreateDirectory(WorkDir);
        }

        // 构建 IOC 容器
        if (_builder != null)
        {
            var meowDatabase = new MeowDatabase(WorkDir, config.BotName, null!, config.DatabaseType, config.DbConnectionString);
            _builder.RegisterInstance(meowDatabase).As<MeowDatabase>().SingleInstance();
            Ioc.Container = _builder.Build();
            Log = Ioc.GetService<ILogger>()!;
            meowDatabase.SetLogger(Log); // 设置数据库的日志记录器
            _builder = null; // 释放 builder
        }

        Log.Information("Meow:[{MeowName}]将在此路径中工作: {WorkDir}", config.BotName, WorkDir);

        Meow = new Core.Meow(config, WorkDir);

        return this;
    }

    /// <summary>
    /// 从程序所在目录读取Config.json并反序列化为MeowConfig
    /// </summary>
    /// <returns></returns>
    /// <exception cref="FileNotFoundException">当Config.json文件未找到时抛出此异常</exception>
    /// <exception cref="JsonException">当反序列化Config.json文件失败时抛出此异常</exception>
    private MeowConfig GetConfig()
    {
        var configPath = Path.Combine(StaticValue.AppCurrentPath, "Config.json");
        if (!File.Exists(configPath))
        {
            throw new FileNotFoundException($"配置文件未找到: {configPath}");
        }

        try
        {
            var jsonContent = File.ReadAllText(configPath);
            var config = JsonConvert.DeserializeObject<MeowConfig>(jsonContent) ??
                         throw new JsonException("配置文件反序列化失败");
            return config;
        }
        catch (Exception ex) when (ex is IOException or JsonException)
        {
            throw new Exception($"读取配置文件时发生错误: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 构建
    /// </summary>
    /// <returns></returns>
    public Core.Meow BuildMeow()
    {
        if (Meow is null)
        {
            throw new Exception("构建Meow时发生错误, 请检查构造流程");
        }
        return Meow;
    }

    #endregion
}