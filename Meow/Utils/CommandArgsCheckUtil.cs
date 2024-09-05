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

    /// <summary>
    /// 分割数量范围
    /// </summary>
    private Range SpiltRange { get; set; }

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
    /// <param name="splitRange">分割的长度范围, 用于后续检查</param>
    /// <param name="errorMsg">如果分割错误的提示信息</param>
    /// <returns></returns>
    public CommandArgsCheckUtil SplitArgsAndCheckLength(char splitSymbol,
        int spiltCount,
        Range splitRange,
        string errorMsg)
    {
        SplitResult = null;
        if (CheckFailed)
        {
            return this;
        }

        if (splitRange.Start.Value < 0)
        {
            SetCheckFailed("分割数量范围不能小于0开始");
            return this;
        }

        if (splitRange.Start.Value > spiltCount || splitRange.End.Value < spiltCount)
        {
            SetCheckFailed("分割数量不能超出指定范围");
            return this;
        }

        NeedSpilt = true;
        SplitResult = Args!.Split(splitSymbol, spiltCount).ToList();
        if (splitRange.Start.Value > SplitResult.Count || splitRange.End.Value < SplitResult.Count)
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

    /// <summary>
    /// 当指定索引位置的参数为特定值时，使用正则表达式检查目标索引位置的参数。
    /// </summary>
    /// <param name="parmaIndex">条件参数的索引位置</param>
    /// <param name="param">条件参数的特定值</param>
    /// <param name="targetIndex">目标参数的索引位置</param>
    /// <param name="pattern">用于验证目标参数的正则表达式</param>
    /// <param name="errorTip">如果检查失败，要显示的错误提示信息</param>
    /// <returns>当前的 <see cref="CommandArgsCheckUtil"/> 实例</returns>
    /// <remarks>当条件参数值为特定值时，检查目标参数是否符合正则表达式。如果检查失败，设置错误提示信息并返回当前实例。</remarks>
    public CommandArgsCheckUtil RegexWhenParamIs(int parmaIndex, string param, int targetIndex, string pattern,
        string errorTip)
    {
        // 检查是否有之前的检查失败情况，如果有则直接返回
        if (CheckFailed)
        {
            return this;
        }

        // 检查是否需要进行分割操作，如果不需要则记录错误并返回
        if (!NeedSpilt)
        {
            SetCheckFailed("命令在未分割的情况下检查索引位置参数");
            return this;
        }

        // 检查分割结果是否有效，如果无效则返回
        if (CheckSpiltValid(parmaIndex, nameof(RegexWhenParamIs)))
        {
            return this;
        }

        // 如果分割结果的数量小于目标索引+1，说明目标参数不存在，此时不进行检查，直接返回
        if (SplitResult!.Count < targetIndex + 1)
        {
            return this;
        }

        // 使用正则表达式创建一个匹配条件的实例
        var regex = new Regex(pattern);

        // 检查分割后的参数是否符合预期的值，如果不符合则返回
        if (SplitResult![parmaIndex] != param)
        {
            return this;
        }

        // 检查目标参数是否符合正则表达式的规则，如果符合则返回
        if (regex.IsMatch(SplitResult[targetIndex]))
        {
            return this;
        }

        // 如果以上所有检查都未通过，则记录错误信息并返回
        SetCheckFailed(errorTip);
        return this;
    }
}