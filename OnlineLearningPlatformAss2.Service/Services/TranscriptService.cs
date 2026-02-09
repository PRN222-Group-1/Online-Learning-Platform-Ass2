using System.Net.Http.Headers;
using System.Text.Json;
using OnlineLearningPlatformAss2.Service.Services.Interfaces;
using Microsoft.Extensions.Configuration;

namespace OnlineLearningPlatformAss2.Service.Services;

public class TranscriptService(HttpClient httpClient, IConfiguration configuration) : ITranscriptService
{
    private readonly HttpClient _http = httpClient;
    private readonly string _baseUrl = "http://100.86.222.32:8000/transcribe/";

    public async Task<string> GenerateTranscriptFromVideoAsync(string videoUrl, bool summarize = true)
    {
        var tempFile = Path.GetTempFileName();

        try
        {
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

            var response = await _http.PostAsync(_baseUrl, form);

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            
            using var doc = JsonDocument.Parse(json);
            
            if (doc.RootElement.TryGetProperty("data", out var dataElement))
            {
                if (dataElement.TryGetProperty("text", out var textElement))
                {
                     var transcript = textElement.GetString() ?? string.Empty;
                     if (summarize && dataElement.TryGetProperty("summary", out var summaryElement))
                     {
                         transcript += "\n\n### Summary\n" + (summaryElement.GetString() ?? string.Empty);
                     }
                     return transcript;
                }
            }
            
            return string.Empty;
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }
}
