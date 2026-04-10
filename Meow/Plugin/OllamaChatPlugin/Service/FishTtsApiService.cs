using System.Net;
using System.Text;
using Meow.Plugin.OllamaChatPlugin.Models;
using Newtonsoft.Json;

namespace Meow.Plugin.OllamaChatPlugin.Service;

public class FishTtsApiService
{
    private static readonly HttpClient SharedHttpClient = new(new SocketsHttpHandler()
    {
        Proxy = new WebProxy("http://localhost:7890"),
        UseProxy = true,
    })
    {
        Timeout = TimeSpan.FromSeconds(45),
        
    };

    private readonly Core.Meow _bot;
    private readonly FishTtsConfig _config;
    private readonly string _apiKey;
    private readonly string _referenceId;

    public FishTtsApiService(Core.Meow bot, FishTtsConfig config)
    {
        _bot = bot;
        _config = config;
        _apiKey = config.ApiKey;
        _referenceId = config.ReferenceId;
    }

    /// <summary>
    /// 调用 Fish Audio TTS 接口，将文本转为语音并保存到指定路径
    /// </summary>
    public async Task<bool> SynthesizeToFileAsync(string text, string outputPath)
    {
        try
        {
            var requestBody = new
            {
                text,
                reference_id = _referenceId,
                format = _config.Format
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, _config.ApiUrl);
            request.Headers.Add("Authorization", $"Bearer {_apiKey}");
            request.Headers.Add("model", _config.Model);
            request.Content = new StringContent(
                JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

            
            var response = await SharedHttpClient.SendAsync(request).ConfigureAwait(false);

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