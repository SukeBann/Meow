namespace Meow.Utils;

public static class StaticValue
{
    /// <summary>
    /// App可执行程序所在路径
    /// </summary>
    public static string AppCurrentPath { get; set; } = AppDomain.CurrentDomain.BaseDirectory;
}