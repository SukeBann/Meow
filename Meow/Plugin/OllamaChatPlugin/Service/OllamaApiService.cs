using System.Text;
using Meow.Plugin.OllamaChatPlugin.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Meow.Plugin.OllamaChatPlugin.Service;

public class OllamaApiService
{
    private readonly HttpClient _httpClient;
    private readonly Core.Meow _bot;
    private readonly OllamaConfig _config;

    public OllamaApiService(Core.Meow bot, OllamaConfig config)
    {
        _bot = bot;
        _config = config;
        _httpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(2) };
    }

    public async Task<string?> GetChatResponseAsync(List<OllamaMessage> messages)
    {
        try
        {
            var requestBody = new
            {
                model = _config.Model,
                messages = messages,
                stream = false,
                options = new
                {
                    num_predict = _config.MaxTokens
                }
            };

            var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(_config.ApiUrl, content).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                _bot.Error($"Ollama API Error: {response.StatusCode}, {error}");
                return null;
            }

            var responseJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return ParseChatResponse(responseJson);
        }
        catch (Exception e)
        {
            _bot.Error("Exception while calling Ollama API", e);
            return null;
        }
    }

    private string? ParseChatResponse(string responseJson)
    {
        if (string.IsNullOrWhiteSpace(responseJson))
        {
            return null;
        }

        try
        {
            var result = JObject.Parse(responseJson);
            return result["message"]?["content"]?.ToString();
        }
        catch (JsonReaderException)
        {
            var sb = new StringBuilder();
            var lines = responseJson
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                try
                {
                    var chunk = JObject.Parse(line);
                    var content = chunk["message"]?["content"]?.ToString();

                    if (!string.IsNullOrEmpty(content))
                    {
                        sb.Append(content);
                    }

                    if (chunk["done"]?.Value<bool>() == true)
                    {
                        break;
                    }
                }
                catch (JsonReaderException ex)
                {
                    _bot.Error($"Failed to parse Ollama response chunk: {line}", ex);
                }
            }

            return sb.Length > 0 ? sb.ToString() : null;
        }
    }
}

public class OllamaMessage
{
    [JsonProperty("role")]
    public string Role { get; set; } = string.Empty;

    [JsonProperty("content")]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// 消息产生或接收时间 (UTC)
    /// </summary>
    [JsonProperty("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
