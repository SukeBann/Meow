using Autofac;
using Lagrange.Core;
using Lagrange.Core.Common;
using Lagrange.Core.Common.Interface;
using Meow.Utils;
using Serilog;

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
    /// Bot实例
    /// </summary>
    private BotContext? Bot { get; set; }

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
        ConfigurationServices();
        var meowBootstrapper = new MeowBootstrapper(Ioc.GetService<ILogger>());
        IsInit = true;
        return meowBootstrapper;
    }

    /// <summary>
    /// 配置IOC
    /// </summary>
    private static void ConfigurationServices()
    {
        ContainerBuilder builder = new();

        var currentDomainBaseDirectory = StaticValue.AppCurrentPath;
        var logger = LoggerCreator.GenerateLoggerConfig(currentDomainBaseDirectory)
            .CreateLogger();
        logger.Information("全局日志模块加载完毕");
        builder.RegisterInstance(logger).As<ILogger>().SingleInstance();

        var meowDatabase = new MeowDatabase(currentDomainBaseDirectory, "MeowDb", logger);
        builder.RegisterInstance(meowDatabase).As<MeowDatabase>().SingleInstance();

        Ioc.Container = builder.Build();
    }

    #endregion

    #region Build

    /// <summary>
    /// 设置工作目录, 配置文件以及bot产生的文件都会存放到此目录中下的对应Meow name文件夹中
    /// </summary>
    /// <param name="workDIr">路径</param>
    /// <param name="meowName">meowName</param>
    /// <param name="commandPrompt">命令提示符</param>
    /// <param name="commandArgsSeparator">参数分隔符</param>
    /// <returns></returns>
    /// <exception cref="IOException">如果目录不存在则会抛出此异常</exception>
    public MeowBootstrapper ConfigurationBot(string workDIr, string meowName, char commandPrompt,
        char commandArgsSeparator)
    {
        WorkDir = Path.Combine(workDIr, meowName);

        if (!Directory.Exists(WorkDir))
        {
            var messageTemplate = $"无法找到配置文件目录:{WorkDir}";
            throw new IOException(messageTemplate);
        }

        Log.Information("Meow:[{MeowName}]将在此路径中工作: {WorkDir}", meowName, WorkDir);
        Meow = new Core.Meow(meowName, WorkDir, commandPrompt, commandArgsSeparator);
        return this;
    }

    /// <summary>
    /// 构建机器人信息
    /// </summary>
    /// <param name="botConfig"></param>
    /// <param name="keystore"></param>
    /// <param name="deviceInfo"></param>
    /// <returns></returns>
    public MeowBootstrapper SetBotInfo(BotConfig? botConfig = null,
        BotKeystore? keystore = null,
        BotDeviceInfo? deviceInfo = null)
    {
        if (WorkDir is null)
        {
            throw new Exception("你应该先设置工作目录");
        }
        
        if (keystore is not null)
        {
            BotInfoManager.SaveKeystore(WorkDir, keystore);
        }

        var loadKeystore = keystore ?? BotInfoManager.LoadKeystore(WorkDir);

        if (deviceInfo is not null)
        {
            BotInfoManager.SaveDeviceInfo(WorkDir, deviceInfo);
        }

        var botDeviceInfo = deviceInfo ?? BotInfoManager.GetDeviceInfo(WorkDir);

        var config = botConfig ?? new BotConfig();
        Bot = BotFactory.Create(config, botDeviceInfo, loadKeystore);
        return this;
    }

    /// <summary>
    /// 构建
    /// </summary>
    /// <returns></returns>
    public Core.Meow BuildMeow()
    {
        if (Meow is null || Bot is null)
        {
            throw new Exception("构建Meow时发生错误, 请检查构造流程");
        }
        
        Meow.MeowBot = Bot;
        return Meow;
    }

    #endregion
}