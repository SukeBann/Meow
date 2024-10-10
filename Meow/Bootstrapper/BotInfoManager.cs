using System.Text.Json;
using System.Text.Json.Serialization;
using Lagrange.Core.Common;

namespace Meow.Bootstrapper;

/// <summary>
/// Bot配置信息管理
/// </summary>
public static class BotInfoManager
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        ReferenceHandler = ReferenceHandler.Preserve
    };

    /// <summary>
    /// 检查配置文件目录[不存在会创建路径], 并返回合并的配置文件路径
    /// </summary>
    /// <param name="baseFolder">bot工作目录</param>
    /// <param name="filePath">文件路径</param>
    /// <returns></returns>
    private static string GetConfigPath(string baseFolder, string filePath)
    {
        var configFolder = Path.Combine(baseFolder, "config");
        if (!Directory.Exists(configFolder))
        {
            Directory.CreateDirectory(configFolder);
        }

        return Path.Combine(configFolder, filePath);
    }

    /// <summary>
    /// 获取设备信息
    /// </summary>
    /// <returns></returns>
    public static BotDeviceInfo GetDeviceInfo(string baseFolder)
    {
        var deviceInfoPath = GetConfigPath(baseFolder, "DeviceInfo.json");
        if (File.Exists(deviceInfoPath))
        {
            var info = JsonSerializer.Deserialize<BotDeviceInfo>(File.ReadAllText(deviceInfoPath));
            if (info != null) return info;

            info = BotDeviceInfo.GenerateInfo();
            File.WriteAllText(deviceInfoPath, JsonSerializer.Serialize(info));
            return info;
        }

        var deviceInfo = BotDeviceInfo.GenerateInfo();
        File.WriteAllText(deviceInfoPath, JsonSerializer.Serialize(deviceInfo));
        return deviceInfo;
    }

    /// <summary>
    /// 保存设备信息
    /// </summary>
    /// <returns></returns>
    public static void SaveDeviceInfo(string baseFolder, BotDeviceInfo deviceInfo)
    {
        var deviceInfoPath = GetConfigPath(baseFolder, "DeviceInfo.json");
        File.WriteAllText(deviceInfoPath, JsonSerializer.Serialize(deviceInfo));
    }

    /// <summary>
    /// 保存Keystore
    /// </summary>
    /// <param name="baseFolder">bot工作目录</param>
    /// <param name="keystore"></param>
    public static void SaveKeystore(string baseFolder, BotKeystore keystore)
    {
        var keystorePath = GetConfigPath(baseFolder, "Keystore.json");
        File.WriteAllText(keystorePath, JsonSerializer.Serialize(keystore));
    }

    /// <summary>
    /// 查询key是否存在
    /// </summary>
    /// <returns></returns>
    public static bool KeystoreIsExist(string baseFolder)
    {
        var keystorePath = GetConfigPath(baseFolder, "Keystore.json");
        return File.Exists(keystorePath);
    }

    /// <summary>
    /// 获取Keystore
    /// </summary>
    /// <param name="baseFolder">bot工作目录</param>
    public static BotKeystore LoadKeystore(string baseFolder)
    {
        try
        {
            var keystorePath = GetConfigPath(baseFolder, "Keystore.json");
            if (!KeystoreIsExist(baseFolder))
            {
                return new BotKeystore();
            }

            var text = File.ReadAllText(keystorePath);
            return JsonSerializer.Deserialize<BotKeystore>(text, JsonSerializerOptions) ?? new BotKeystore();
        }
        catch
        {
            return new BotKeystore();
        }
    }
}