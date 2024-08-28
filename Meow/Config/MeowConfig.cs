namespace Meow.Config;

/// <summary>
/// 表示 Meow 配置类
/// </summary>
/// <param name="botWorkDir">Bot 工作目录</param>
/// <param name="botName">Bot 名称</param>
/// <param name="commandPrompt">命令提示符</param>
/// <param name="commandArgsSeparator">命令参数分隔符</param>
public class MeowConfig(string botWorkDir, string botName, char commandPrompt, char commandArgsSeparator)
{
    /// <summary>
    /// 获取或设置 Bot 工作目录
    /// </summary>
    public string BotWorkDir { get; set; } = botWorkDir;

    /// <summary>
    /// 获取或设置 Bot 名称
    /// </summary>
    public string BotName { get; set; } = botName;

    /// <summary>
    /// 获取或设置命令提示符
    /// </summary>
    public char CommandPrompt { get; set; } = commandPrompt;

    /// <summary>
    /// 获取或设置命令参数分隔符
    /// </summary>
    public char CommandArgsSeparator { get; set; } = commandArgsSeparator;
}