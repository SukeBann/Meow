using Camille.Core.Enum.MiraiBot;
using Camille.Core.MiraiBase.Contract;
using Camille.Core.Models;
using Newtonsoft.Json;

namespace Meow.Config;

/// <summary>
/// 表示 Meow 配置类
/// </summary>
public class MeowConfig : IMiraiBotConfig
{
    [JsonConstructor]
    public MeowConfig(
        string botWorkDir,
        string botName,
        char commandPrompt,
        char commandArgsSeparator,
        long botQq,
        string verifyKey,
        string host,
        string databaseType = "Sqlite",
        string? dbConnectionString = null)
    {
        BotWorkDir = botWorkDir;
        BotName = botName;
        CommandPrompt = commandPrompt;
        CommandArgsSeparator = commandArgsSeparator;
        BotQq = botQq;
        VerifyKey = verifyKey;
        Host = host;
        DatabaseType = databaseType;
        DbConnectionString = dbConnectionString;
        
        // 默认初始化
        ReceiveAdapterServerAddress = host;
        ApiAdapterServerAddress = host;
    }

    /// <summary>
    /// 数据库类型 (Sqlite, MySql)
    /// </summary>
    public string DatabaseType { get; set; }

    /// <summary>
    /// 数据库连接字符串 (如果是 MySql 则需要)
    /// </summary>
    public string? DbConnectionString { get; set; }

    /// <summary>
    /// 获取或设置 Bot 工作目录
    /// </summary>
    public string BotWorkDir { get; set; }

    /// <summary>
    /// 获取或设置 Bot 名称
    /// </summary>
    public string BotName { get; set; }

    /// <summary>
    /// 获取或设置命令提示符
    /// </summary>
    public char CommandPrompt { get; set; }

    /// <summary>
    /// 获取或设置命令参数分隔符
    /// </summary>
    public char CommandArgsSeparator { get; set; }

    /// <summary>
    /// 获取或设置 Bot QQ 号
    /// </summary>
    public long BotQq { get; set; }

    /// <summary>
    /// 获取或设置 Camille 验证密钥
    /// </summary>
    public string VerifyKey { get; set; }

    /// <summary>
    /// 获取或设置 Camille 主机地址 (例如 localhost:8089)
    /// </summary>
    public string Host { get; set; }

    #region IMiraiBotConfig Implementation

    [JsonIgnore]
    public long QQ => BotQq;

    [JsonIgnore]
    public ReceiveAdapterType ReceiveAdapterType { get; private set; } = ReceiveAdapterType.Websocket;

    [JsonIgnore]
    public AdapterServerAddress? ReceiveAdapterServerAddress { get; private set; }

    [JsonIgnore]
    public ApiAdapterType ApiAdapterType { get; private set; } = ApiAdapterType.Http;

    [JsonIgnore]
    public AdapterServerAddress? ApiAdapterServerAddress { get; private set; }

    public IMiraiBotConfig AddReceiveAdapter(ReceiveAdapterType receiveAdapterType, AdapterServerAddress address)
    {
        ReceiveAdapterType = receiveAdapterType;
        ReceiveAdapterServerAddress = address;
        return this;
    }

    public IMiraiBotConfig AddApiAdapter(ApiAdapterType apiAdapterType, AdapterServerAddress address)
    {
        ApiAdapterType = apiAdapterType;
        ApiAdapterServerAddress = address;
        return this;
    }

    #endregion
}