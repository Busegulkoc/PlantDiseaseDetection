using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.IO;
using System;
using System.Text.Json;

namespace FruitDiseaseDetection.Services
{
    public class PredictionService
    {
        private readonly HttpClient _httpClient;

        public PredictionService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<object> PredictAsync(byte[] imageBytes, string fileName)
        {
            try
            {
                using (var content = new MultipartFormDataContent())
                {
                    var fileContent = new ByteArrayContent(imageBytes);
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
                    content.Add(fileContent, "file", fileName);

                    var response = await _httpClient.PostAsync("http://127.0.0.1:8000/predict", content);
                    response.EnsureSuccessStatusCode();
                    var result = await response.Content.ReadAsStringAsync();

                    // JSON stringini nesne olarak dön
                    return JsonSerializer.Deserialize<object>(result);
                }
            }
            catch (Exception ex)
            {
                return new { error = "Error" };
            }
        }
    }
}
