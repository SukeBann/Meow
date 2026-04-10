using System.Text;
using Meow.Plugin.OllamaChatPlugin.Models;
using Newtonsoft.Json;

namespace Meow.Plugin.OllamaChatPlugin.Service;

public class FishTtsApiService
{
    private readonly HttpClient _httpClient;
    private readonly Core.Meow _bot;
    private readonly FishTtsConfig _config;

    public FishTtsApiService(Core.Meow bot, FishTtsConfig config)
    {
        _bot = bot;
        _config = config;
        _httpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(2) };
    }

    /// <summary>
    /// 从文本文件读取 API Key
    /// </summary>
    private string ReadApiKey()
    {
        if (!File.Exists(_config.ApiKeyFilePath))
        {
            throw new FileNotFoundException($"API Key 文件未找到: {_config.ApiKeyFilePath}");
        }

        var key = File.ReadAllText(_config.ApiKeyFilePath).Trim();
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new InvalidOperationException("API Key 文件内容为空");
        }

        return key;
    }

    /// <summary>
    /// 从文本文件读取 Reference ID（音色标识）
    /// </summary>
    private string ReadReferenceId()
    {
        if (!File.Exists(_config.ReferenceIdFilePath))
        {
            throw new FileNotFoundException($"Reference ID 文件未找到: {_config.ReferenceIdFilePath}");
        }

        var id = File.ReadAllText(_config.ReferenceIdFilePath).Trim();
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new InvalidOperationException("Reference ID 文件内容为空");
        }

        return id;
    }

    /// <summary>
    /// 调用 Fish Audio TTS 接口，将文本转为语音并保存到指定路径
    /// </summary>
    /// <param name="text">要合成的文本</param>
    /// <param name="outputPath">输出音频文件路径</param>
    /// <returns>成功返回 true</returns>
    public async Task<bool> SynthesizeToFileAsync(string text, string outputPath)
    {
        try
        {
            var apiKey = ReadApiKey();
            var referenceId = ReadReferenceId();

            var requestBody = new
            {
                text,
                reference_id = referenceId,
                format = _config.Format
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, _config.ApiUrl);
            request.Headers.Add("Authorization", $"Bearer {apiKey}");
            request.Headers.Add("model", _config.Model);
            request.Content = new StringContent(
                JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                _bot.Error($"Fish TTS API Error: {response.StatusCode}, {error}");
                return false;
            }

            var audioBytes = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
            await File.WriteAllBytesAsync(outputPath, audioBytes).ConfigureAwait(false);

            return true;
        }
        catch (Exception e)
        {
            _bot.Error("Exception while calling Fish TTS API", e);
            return false;
        }
    }
}
