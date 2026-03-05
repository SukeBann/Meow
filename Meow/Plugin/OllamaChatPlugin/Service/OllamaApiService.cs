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
            var result = JObject.Parse(responseJson);
            return result["message"]?["content"]?.ToString();
        }
        catch (Exception e)
        {
            _bot.Error("Exception while calling Ollama API", e);
            return null;
        }
    }
}

public class OllamaMessage
{
    [JsonProperty("role")]
    public string Role { get; set; }

    [JsonProperty("content")]
    public string Content { get; set; }
}
