using Camille.Core.MiraiBase.Contract;
using Camille.Core.MiraiBase.Models.Base;
using Camille.Imp.MiraiBase;
using Meow.Utils;
using Serilog;

namespace Meow.Core;

/// <summary>
/// 万Meow之祖
/// </summary>
public abstract class MeowBase: MiraiBot
{
    protected MeowBase(IMiraiBotConfig miraiBotConfig, string meowName, string workFolder) : base(miraiBotConfig)
    {
        MeowName = meowName;
        WorkFolder = workFolder;
        Log = Ioc.GetService<ILogger>() ?? LoggerCreator.GenerateLoggerConfig(workFolder).CreateLogger();
        Log.Debug("Meow Ctor invoke, work folder: {WorkFolder}", workFolder);
        Database = new MeowDatabase(WorkFolder, MeowName, Log);
    }

    #region Properties

    public string WorkFolder { get; }

    public string MeowName { get; }


    private ILogger Log { get; set; }

    /// <summary>
    /// 数据库
    /// </summary>
    public MeowDatabase Database { get; set; }

    #endregion

    #region Method

    public async Task Login()
    {
        await LinkStart();
        Info("Bot LinkStart invoked");
    }


    #endregion

    #region Log

    /// <summary>
    /// 记录Info 级别的信息
    /// </summary>
    /// <param name="message">要记录的信息</param>
    public void Info(string message)
    {
        Log.Information("[{MeowName}] {Message}", MeowName, message);
    }

    /// <summary>
    /// 记录Error级的信息
    /// </summary>
    /// <param name="message">要记录的信息</param>
    /// <param name="exception">要记录的异常</param>
    public void Error(string message, Exception? exception = null)
    {
        if (exception is null)
        {
            Log.Error("[{MeowName}] {Message}", MeowName, message);
        }
        else
        {
            Log.Error(exception, "[{MeowName}] {Message}", MeowName, message);
        }
    }

    /// <summary>
    /// 记录Debug级别的信息
    /// </summary>
    /// <param name="message">要记录的信息</param>
    public void Debug(string message)
    {
        Log.Debug("[{MeowName}] {Message}", MeowName, message);
    }

    public void Debug<T>(string message, T obj)
    {
        Debug($"[{MeowName}] {message}\n{obj}");
    }

    #endregion
}