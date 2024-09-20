using Serilog;

namespace Meow.Utils;

public static class LoggerCreator
{
    private static string CheckAndCreateDirectory(string basePath, string filePath)
    {
        var logPath = Path.Combine(basePath, "Log");
        var logFilePath = Path.Combine(logPath, filePath);
        if (!Directory.Exists(logPath))
        {
            Directory.CreateDirectory(logPath);
        }

        return logFilePath;
    }

    /// <summary>
    /// 暴露给其他程序集处理log config
    /// </summary>
    public static Func<LoggerConfiguration, LoggerConfiguration>? EditLoggerConfigurationInterface { get; set; }

    /// <summary>
    /// 创建一个不向文件输出的日志, 并调用<see cref="EditLoggerConfigurationInterface"/>附加额外的配置
    /// </summary>
    /// <returns></returns>
    public static LoggerConfiguration GenerateNoIOLoggerConfiguration()
    {
        var loggerConfiguration = new LoggerConfiguration()
            .MinimumLevel.Information();
#if DEBUG
        loggerConfiguration.MinimumLevel.Debug();
#endif
        if (EditLoggerConfigurationInterface is not null)
        {
            loggerConfiguration = EditLoggerConfigurationInterface.Invoke(loggerConfiguration);
        }

        return loggerConfiguration;
    }

    public static LoggerConfiguration GenerateLoggerConfig(string customFilePath)
    {
        var logFilePath = CheckAndCreateDirectory(customFilePath, "MeowLog.log");
        var loggerConfiguration = new LoggerConfiguration()
            .WriteTo.File(logFilePath,
                rollingInterval: RollingInterval.Hour,
                rollOnFileSizeLimit: true,
                fileSizeLimitBytes: 2 * 1024 * 1024,
                retainedFileCountLimit: 10)
            .WriteTo.Console()
            .MinimumLevel.Information();
#if DEBUG
        // loggerConfiguration.MinimumLevel.Debug()
            // .WriteTo.Debug();
#endif
        if (EditLoggerConfigurationInterface is not null)
        {
            loggerConfiguration = EditLoggerConfigurationInterface.Invoke(loggerConfiguration);
        }

        return loggerConfiguration;
    }
}