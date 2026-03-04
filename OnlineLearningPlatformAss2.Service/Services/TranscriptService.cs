using System.Net.Http.Headers;
using System.Text.Json;
using OnlineLearningPlatformAss2.Service.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace OnlineLearningPlatformAss2.Service.Services;

public class TranscriptService(HttpClient httpClient, IConfiguration configuration, ILogger<TranscriptService> logger) : ITranscriptService
{
    private readonly HttpClient _http = httpClient;
    private readonly IConfiguration _config = configuration;
    private readonly ILogger<TranscriptService> _logger = logger;

    public async Task<string> GenerateTranscriptFromVideoAsync(string videoUrl, bool summarize = true)
    {
        var tempFile = Path.GetTempFileName();
        var baseUrl = _config["WhisperAI:BaseUrl"] ?? "http://localhost:8000/transcribe/";

        try
        {
            _logger.LogInformation("Downloading video from {Url} to {TempFile}", videoUrl, tempFile);
            
            // Download file to temp
            await using (var stream = await _http.GetStreamAsync(videoUrl))
            await using (var fs = File.Create(tempFile))
            {
                await stream.CopyToAsync(fs);
            }

            using var form = new MultipartFormDataContent();

            await using var fs2 = File.OpenRead(tempFile);
            var fileContent = new StreamContent(fs2);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("video/mp4");

            form.Add(fileContent, "file", Path.GetFileName(tempFile));
            form.Add(new StringContent(summarize.ToString().ToLower()), "summarize");

            _logger.LogInformation("Sending POST request to {BaseUrl}", baseUrl);
            var response = await _http.PostAsync(baseUrl, form);

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("Received JSON response: {Json}", json);
            
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // Attempt to find text in various locations (case-insensitive checks)
            string? transcript = null;

            // Helper to get property case-insensitively
            JsonElement GetPropertyIgnoreContent(JsonElement element, string name)
            {
                if (element.ValueKind != JsonValueKind.Object) return default;
                foreach (var prop in element.EnumerateObject())
                {
                    if (string.Equals(prop.Name, name, StringComparison.OrdinalIgnoreCase))
                        return prop.Value;
                }
                return default;
            }

            // 1. Check "data" object (common)
            JsonElement dataEl = GetPropertyIgnoreContent(root, "data");
            if (dataEl.ValueKind == JsonValueKind.Object)
            {
                var textEl = GetPropertyIgnoreContent(dataEl, "text");
                if (textEl.ValueKind == JsonValueKind.String) transcript = textEl.GetString();
                
                if (string.IsNullOrEmpty(transcript))
                {
                    var transEl = GetPropertyIgnoreContent(dataEl, "transcript");
                    if (transEl.ValueKind == JsonValueKind.String) transcript = transEl.GetString();
                }
            }
            
            // 2. Check "transcript" object (as reported in raw output)
            if (string.IsNullOrEmpty(transcript))
            {
                JsonElement transObj = GetPropertyIgnoreContent(root, "transcript");
                if (transObj.ValueKind == JsonValueKind.Object)
                {
                    var fullTextEl = GetPropertyIgnoreContent(transObj, "full_text");
                    if (fullTextEl.ValueKind == JsonValueKind.String) transcript = fullTextEl.GetString();

                    if (string.IsNullOrEmpty(transcript))
                    {
                        var textEl = GetPropertyIgnoreContent(transObj, "text");
                        if (textEl.ValueKind == JsonValueKind.String) transcript = textEl.GetString();
                    }
                }
                else if (transObj.ValueKind == JsonValueKind.String)
                {
                    transcript = transObj.GetString();
                }
            }

            // 3. Check root level
            if (string.IsNullOrEmpty(transcript))
            {
                var textEl = GetPropertyIgnoreContent(root, "text");
                if (textEl.ValueKind == JsonValueKind.String) transcript = textEl.GetString();
            }

            if (!string.IsNullOrEmpty(transcript))
            {
                 if (summarize)
                 {
                     string? summary = null;
                     var sumEl = GetPropertyIgnoreContent(dataEl.ValueKind == JsonValueKind.Object ? dataEl : root, "summary");
                     if (sumEl.ValueKind == JsonValueKind.String) summary = sumEl.GetString();

                     if (!string.IsNullOrEmpty(summary))
                     {
                         transcript += "\n\n### Summary\n" + summary;
                     }
                 }
                 return transcript;
            }
            
            _logger.LogError("Failed to extract transcript from JSON.");
            return string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating transcript from video {Url}", videoUrl);
            throw; // Re-throw to be caught by Hub
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                _logger.LogDebug("Deleting temp file {TempFile}", tempFile);
                File.Delete(tempFile);
            }
        }
    }
}
