using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Lagrange.Core.Message;
using Masuit.Tools;

namespace Meow.Utils;

/// <summary>
/// 参数检查类
/// </summary>
public class CommandArgsCheckUtil
{
    public CommandArgsCheckUtil(MessageChain sourceMessage, string? args)
    {
        SourceMessage = sourceMessage;
        Args = args;

        if (!args.IsNullOrEmpty())
        {
            return;
        }

        SetCheckFailed("命令参数不能为空");
    }

    #region Properties

    /// <summary>
    /// 源消息
    /// </summary>
    private MessageChain SourceMessage { get; set; }

    /// <summary>
    /// 被检查的参数
    /// </summary>
    private string? Args { get; set; }

    /// <summary>
    /// 是否检查不通过
    /// </summary>
    private bool CheckFailed { get; set; }

    /// <summary>
    /// 错误信息
    /// </summary>
    private string? ErrorMsg { get; set; }

    /// <summary>
    /// 是否需要分割参数
    /// </summary>
    private bool NeedSpilt { get; set; }

    /// <summary>
    /// 参数分割结果
    /// </summary>
    private List<string>? SplitResult { get; set; }

    #endregion

    /// <summary>
    /// 开始检查
    /// </summary>
    /// <param name="errorMsg">如果检查失败则是错误信息</param>
    /// <param name="resultMessageChain">如果检查失败则为包含失败结果的消息链</param>
    /// <param name="args">传入的参数</param>
    /// <param name="splitResult">分割结果</param>
    /// <returns></returns>
    public bool IsSuccess(out string errorMsg,
        out MessageChain resultMessageChain,
        out string args,
        out List<string>? splitResult)
    {
        errorMsg = string.Empty;
        resultMessageChain = SourceMessage;
        args = string.Empty;
        splitResult = default;

        if (NeedSpilt)
        {
            if (SplitResult is null || splitResult?.Count < 1)
            {
                SetCheckFailed($"{nameof(IsSuccess)}需要分割参数但分割失败");
            }

            splitResult = SplitResult;
        }

        if (CheckFailed)
        {
            errorMsg = ErrorMsg ?? "未知错误";
            resultMessageChain = SourceMessage.CreateSameTypeTextMessage(ErrorMsg ?? "未知错误");
            return false;
        }

        args = Args;
        return true;
    }

    /// <summary>
    /// 设置参数检查失败
    /// </summary>
    /// <param name="errorMsg">错误信息</param>
    private void SetCheckFailed(string errorMsg)
    {
        CheckFailed = true;
        ErrorMsg = $"参数检查错误: {errorMsg}";
    }

    /// <summary>
    /// 分割参数并检查是否符合期望的长度
    /// </summary>
    /// <param name="splitSymbol">分割符号</param>
    /// <param name="spiltCount">分割长度</param>
    /// <param name="errorMsg">如果分割错误的提示信息</param>
    /// <returns></returns>
    public CommandArgsCheckUtil SplitArgsAndCheckLength(char splitSymbol,
        int spiltCount,
        string errorMsg)
    {
        SplitResult = null;
        if (CheckFailed)
        {
            return this;
        }

        NeedSpilt = true;
        SplitResult = Args!.Split(splitSymbol, spiltCount).ToList();
        if (SplitResult.Count != spiltCount)
        {
            SetCheckFailed(errorMsg);
        }

        return this;
    }

    /// <summary>
    /// 检查分割参数时对索引的检查
    /// </summary>
    /// <param name="index"></param>
    /// <param name="methodName"></param>
    /// <returns>true:检查成功</returns>
    /// <returns>false:检查失败</returns>
    private bool CheckSpiltValid(int index, string methodName)
    {
        if (NeedSpilt && SplitResult?.Count < 1)
        {
            SetCheckFailed($"[{methodName}] 索引分割失败 分割结果数量为0");
            return true;
        }

        if (index + 1 > SplitResult?.Count)
        {
            SetCheckFailed($"[{methodName}] 索引超出参数列表范围");
            return true;
        }

        return false;
    }

    /// <summary>
    /// 使用给定的列表匹配目标索引处的参数是否符合指定长度
    /// </summary>
    /// <param name="index">要匹配哪个元素</param>
    /// <param name="minLength">最短长度</param>
    /// <param name="maxLength">最大长度</param>
    /// <returns></returns>
    public CommandArgsCheckUtil ArgListLength(int index, int maxLength, int minLength = 0)
    {
        if (CheckFailed)
        {
            return this;
        }

        if (CheckSpiltValid(index, nameof(ArgListLength)))
        {
            return this;
        }

        var target = SplitResult?[index] ?? string.Empty;
        if (target.Length < minLength || target.Length > maxLength)
        {
            SetCheckFailed($"[{nameof(ArgListLength)}] 索引{index}的参数长度应该符合最小长度{maxLength},最大长度{maxLength}");
            return this;
        }

        return this;
    }

    /// <summary>
    /// 使用给定的列表匹配目标索引处的参数是否包含在列表中
    /// </summary>
    /// <param name="index">要匹配哪个元素</param>
    /// <param name="value">匹配列表</param>
    /// <returns></returns>
    public CommandArgsCheckUtil ArgListMatch(int index, List<string> value)
    {
        if (CheckFailed)
        {
            return this;
        }

        if (CheckSpiltValid(index, nameof(ArgListMatch)))
        {
            return this;
        }

        if (!value.Contains(SplitResult![index]))
        {
            SetCheckFailed($"[{nameof(ArgListMatch)}] 索引{index}的参数应该为{string.Join(" 或 ", value)}");
            return this;
        }

        return this;
    }

    /// <summary>
    /// 使用正则表达式匹配目标索引处的参数是否包含在列表中, 如果不在就表示参数验证失败
    /// </summary>
    /// <param name="index">要匹配哪个元素</param>
    /// <param name="pattern">正则匹配项</param>
    /// <returns></returns>
    public CommandArgsCheckUtil ArgListRegexMatch(int index, string pattern, string errorTip)
    {
        if (CheckFailed)
        {
            return this;
        }

        if (CheckSpiltValid(index, nameof(ArgListRegexMatch)))
        {
            return this;
        }

        var result = SplitResult![index];
        var regex = new Regex(pattern);

        if (!regex.IsMatch(result))
        {
            SetCheckFailed($"[{nameof(ArgListRegexMatch)}] 索引{index}的参数应该为{errorTip}, 正则表达式:{pattern}");
            return this;
        }

        return this;
    }
}