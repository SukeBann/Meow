namespace Meow.Utils;

public static class StringExtensions
{
    /// <summary>
    /// 尝试将字符串转换为枚举值。
    /// </summary>
    /// <typeparam name="T">枚举的类型.</typeparam>
    /// <param name="str">要转换的字符串.</param>
    /// <param name="result">当此方法返回时，如果转换成功，则包含相当于字符串的枚举值；如果转换失败，则包含 T 的默认值.</param>
    /// <returns>
    /// true: 如果字符串已成功转换为枚举值；
    /// <br/>false: 转换失败；
    /// </returns>
    public static bool TryConvertToEnum<T>(this string str, out T result) where T : struct, Enum
    {
        return Enum.TryParse(str, true, out result);
    }
}