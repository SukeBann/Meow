using Lagrange.Core;
using Lagrange.Core.Common.Interface.Api;
using Lagrange.Core.Message;
using Meow.Bootstrapper;
using Meow.Utils;
using Serilog;

namespace Meow.Core;

/// <summary>
/// 万Meow之祖
/// </summary>
public abstract class MeowBase
{
    protected MeowBase(string meowName, string workFolder)
    {
        MeowName = meowName;
        WorkFolder = workFolder;
        Log = LoggerCreator.GenerateLoggerConfig(workFolder).CreateLogger();
        Log.Debug("Meow Ctor invoke, work folder: {WorkFolder}", workFolder);
        Database = new MeowDatabase(WorkFolder, MeowName, Log);
    }

    #region Properties

    public string WorkFolder { get; }

    public string MeowName { get; }

    /// <summary>
    /// <see cref="Lagrange.Core.BotContext"/>提供了bot的基本设施
    /// Meow是在其基础上实现的
    /// </summary>
    public BotContext MeowBot { get; set; }

    private ILogger Log { get; set; }
    
    /// <summary>
    /// 数据库
    /// </summary>
    public MeowDatabase Database { get; set; }

    #endregion

    #region Method

    public async Task Login()
    {
        if (BotInfoManager.KeystoreIsExist(WorkFolder))
        {
            await MeowBot.LoginByPassword();
            return;
        }

        var fetchQrCode = await MeowBot.FetchQrCode();
        if (fetchQrCode != null)
        {
            var path = Path.Combine(WorkFolder, "qr.png");
            await File.WriteAllBytesAsync(path, fetchQrCode.Value.QrCode);
            Info($"GetQrCodeUrl: {fetchQrCode?.Url}, Image:{path}");
            await MeowBot.LoginByQrCode();
        }
        else
        {
            Error("获取二维码识别, 无法成功登录");
        }
    }

    /// <summary>
    /// 发送消息
    /// </summary>
    /// <param name="messageChain">发送消息</param>
    public async Task SendMessage(MessageChain messageChain)
    {
        await MeowBot.SendMessage(messageChain);
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